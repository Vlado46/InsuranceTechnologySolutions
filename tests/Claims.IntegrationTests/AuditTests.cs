using System.Net.Http.Json;
using System.Text.Json;
using Claims.Application.Contracts;
using Claims.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.IntegrationTests;

public class AuditTests(ClaimsApiFixture fixture)
{
    private static readonly DateTime Tomorrow = DateTime.UtcNow.Date.AddDays(1);

    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task Audit_IsStoredInTheBackground_AfterCreateAndDelete()
    {
        var client = fixture.CreateClient();

        var coverId = await CreateCoverAsync(client);
        await client.DeleteAsync($"/Covers/{coverId}", _cancellationToken);

        var timeout = TimeSpan.FromSeconds(10);
        var started = DateTime.UtcNow;
        while (DateTime.UtcNow - started < timeout)
        {
            await using var context = await fixture.CreateAuditContextAsync();
            var operations = await context.CoverAudits
                .Where(entry => entry.CoverId == coverId)
                .Select(entry => entry.HttpRequestType)
                .ToListAsync(_cancellationToken);

            if (operations.Count == 2)
            {
                Assert.Contains("POST", operations);
                Assert.Contains("DELETE", operations);
                return;
            }

            await Task.Delay(100, _cancellationToken);
        }

        Assert.Fail("Audit entries were not stored in time.");
    }

    [Fact]
    public async Task Audit_IsNotLost_WhenTheApiShutsDown()
    {
        string coverId;
        await using (var factory = fixture.CreateFactory())
        {
            coverId = await CreateCoverAsync(factory.CreateClient());
        }

        await using var context = await fixture.CreateAuditContextAsync();
        Assert.True(await context.CoverAudits.AnyAsync(entry => entry.CoverId == coverId, _cancellationToken));
    }

    [Fact]
    public async Task Enqueue_CompletesWithoutWaitingForAWorker()
    {
        var entry = new AuditEntry(AuditTarget.Claim, "claim-1", AuditOperation.Create, DateTime.UtcNow);

        var enqueue = new AuditQueue().EnqueueAsync(entry);

        Assert.True(enqueue.IsCompletedSuccessfully);
    }

    private async Task<string> CreateCoverAsync(HttpClient client)
    {
        var request = new { startDate = Tomorrow, endDate = Tomorrow.AddDays(30), type = "Yacht" };

        var response = await client.PostAsJsonAsync("/Covers", request, _cancellationToken);
        response.EnsureSuccessStatusCode();

        var cover = await response.Content.ReadFromJsonAsync<JsonElement>(_cancellationToken);
        return cover.GetProperty("id").GetString()!;
    }
}
