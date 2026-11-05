using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Claims.IntegrationTests;

public class ValidationTests(ClaimsApiFixture fixture)
{
    private static readonly DateTime Tomorrow = DateTime.UtcNow.Date.AddDays(1);

    private readonly HttpClient _client = fixture.CreateClient();
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreateCover_ReturnsProblem_WhenStartDateIsInThePast()
    {
        var request = new { startDate = Tomorrow.AddDays(-2), endDate = Tomorrow, type = "Yacht" };

        var response = await _client.PostAsJsonAsync("/Covers", request, _cancellationToken);

        await AssertProblemAsync(response, "StartDate");
    }

    [Fact]
    public async Task CreateCover_ReturnsProblem_WhenPeriodExceedsOneYear()
    {
        var request = new { startDate = Tomorrow, endDate = Tomorrow.AddYears(1).AddDays(1), type = "Yacht" };

        var response = await _client.PostAsJsonAsync("/Covers", request, _cancellationToken);

        await AssertProblemAsync(response, "EndDate");
    }

    [Fact]
    public async Task ComputePremium_ReturnsProblem_WhenPeriodIsInvalid()
    {
        var url = $"/Covers/compute?startDate={Tomorrow:yyyy-MM-dd}&endDate={Tomorrow:yyyy-MM-dd}&coverType=Yacht";

        var response = await _client.PostAsync(url, null, _cancellationToken);

        await AssertProblemAsync(response, "EndDate");
    }

    [Fact]
    public async Task CreateClaim_ReturnsProblem_WhenDamageIsTooHighAndDateIsOutsideTheCover()
    {
        var cover = await CreateCoverAsync();
        var request = new
        {
            coverId = cover.GetProperty("id").GetString(),
            created = Tomorrow.AddDays(31),
            name = "Hull damage",
            type = "Fire",
            damageCost = 100_001m
        };

        var response = await _client.PostAsJsonAsync("/Claims", request, _cancellationToken);

        await AssertProblemAsync(response, "DamageCost", "Created");
    }

    [Fact]
    public async Task CreateClaim_ReturnsProblem_WhenCoverDoesNotExist()
    {
        var request = new { coverId = "unknown", created = Tomorrow, name = "Hull damage", type = "Fire", damageCost = 5_000m };

        var response = await _client.PostAsJsonAsync("/Claims", request, _cancellationToken);

        await AssertProblemAsync(response, "CoverId");
    }

    [Fact]
    public async Task CreateCover_KeepsItsDates_WhenSentWithoutOffset()
    {
        var request = new { startDate = $"{Tomorrow:yyyy-MM-dd}", endDate = $"{Tomorrow.AddDays(30):yyyy-MM-dd}", type = "Yacht" };

        var response = await _client.PostAsJsonAsync("/Covers", request, _cancellationToken);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<JsonElement>(_cancellationToken);

        var read = await _client.GetFromJsonAsync<JsonElement>($"/Covers/{created.GetProperty("id").GetString()}", _cancellationToken);
        Assert.Equal(Tomorrow, read.GetProperty("startDate").GetDateTime().ToUniversalTime());
    }

    private async Task<JsonElement> CreateCoverAsync()
    {
        var request = new { startDate = Tomorrow, endDate = Tomorrow.AddDays(30), type = "Yacht" };

        var response = await _client.PostAsJsonAsync("/Covers", request, _cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>(_cancellationToken);
    }

    private async Task AssertProblemAsync(HttpResponseMessage response, params string[] properties)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_cancellationToken);
        var errors = problem.GetProperty("errors");
        Assert.All(properties, property => Assert.True(errors.TryGetProperty(property, out _), $"No error for {property}."));
    }
}
