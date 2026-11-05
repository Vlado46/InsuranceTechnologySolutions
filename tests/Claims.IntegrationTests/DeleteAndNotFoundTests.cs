using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Claims.IntegrationTests;

public class DeleteAndNotFoundTests(ClaimsApiFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("/Claims/unknown")]
    [InlineData("/Covers/unknown")]
    public async Task Get_ReturnsNotFound_WhenIdIsUnknown(string url)
    {
        var response = await _client.GetAsync(url, _cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/Claims/unknown")]
    [InlineData("/Covers/unknown")]
    public async Task Delete_ReturnsNotFound_WhenIdIsUnknown(string url)
    {
        var response = await _client.DeleteAsync(url, _cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_AndRemovesTheCover()
    {
        var coverId = await CreateCoverAsync();

        var response = await _client.DeleteAsync($"/Covers/{coverId}", _cancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var read = await _client.GetAsync($"/Covers/{coverId}", _cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_AndRemovesTheClaim()
    {
        var claimId = await CreateClaimAsync(await CreateCoverAsync());

        var response = await _client.DeleteAsync($"/Claims/{claimId}", _cancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var read = await _client.GetAsync($"/Claims/{claimId}", _cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
    }

    private async Task<string> CreateCoverAsync()
    {
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var request = new { startDate, endDate = startDate.AddDays(30), type = "Yacht" };

        return await PostAndReadIdAsync("/Covers", request);
    }

    private async Task<string> CreateClaimAsync(string coverId)
    {
        var request = new
        {
            coverId,
            created = DateTime.UtcNow.Date.AddDays(2),
            name = "Hull damage",
            type = "Fire",
            damageCost = 5_000m
        };

        return await PostAndReadIdAsync("/Claims", request);
    }

    private async Task<string> PostAndReadIdAsync(string url, object request)
    {
        var response = await _client.PostAsJsonAsync(url, request, _cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_cancellationToken);
        return body.GetProperty("id").GetString()!;
    }
}
