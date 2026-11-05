using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Application.Contracts;
using Claims.Domain;
using Xunit;

namespace Claims.IntegrationTests;

public class ClaimsApiTests(ClaimsApiFixture fixture)
{
    private static readonly DateTime Tomorrow = DateTime.UtcNow.Date.AddDays(1);

    // The API writes enums as text, so the test client has to read them the same way.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = fixture.CreateClient();
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cover_CanBeCreatedAndReadBack()
    {
        var cover = await CreateCoverAsync();

        Assert.Equal(41_250m, cover.Premium);
        Assert.Equal(cover, await GetAsync<CoverResponse>($"/Covers/{cover.Id}"));
        Assert.Contains(cover, await GetAsync<List<CoverResponse>>("/Covers"));
    }

    [Fact]
    public async Task Claim_CanBeCreatedAndReadBack()
    {
        var cover = await CreateCoverAsync();
        var request = new CreateClaimRequest(cover.Id, Tomorrow.AddDays(1), "Hull damage", ClaimType.Fire, 5_000m);

        var response = await _client.PostAsJsonAsync("/Claims", request, JsonOptions, _cancellationToken);
        response.EnsureSuccessStatusCode();
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(JsonOptions, _cancellationToken);

        Assert.Equal(cover.Id, claim!.CoverId);
        Assert.Equal(claim, await GetAsync<ClaimResponse>($"/Claims/{claim.Id}"));
        Assert.Contains(claim, await GetAsync<List<ClaimResponse>>("/Claims"));
    }

    [Fact]
    public async Task ComputePremium_ReturnsPremium()
    {
        var url =$"/Covers/compute?startDate={Tomorrow:yyyy-MM-dd}&endDate={Tomorrow.AddDays(30):yyyy-MM-dd}&coverType=Tanker";

        var response = await _client.PostAsync(url, null, _cancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.Equal(56_250m, await response.Content.ReadFromJsonAsync<decimal>(_cancellationToken));
    }

    private async Task<CoverResponse> CreateCoverAsync()
    {
        var request = new CreateCoverRequest(Tomorrow, Tomorrow.AddDays(30), CoverType.Yacht);

        var response = await _client.PostAsJsonAsync("/Covers", request, JsonOptions, _cancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CoverResponse>(JsonOptions, _cancellationToken))!;
    }

    private async Task<T> GetAsync<T>(string url) =>
        (await _client.GetFromJsonAsync<T>(url, JsonOptions, _cancellationToken))!;
}
