using Claims.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Infrastructure.Auditing;

/// <summary>Stores the entries from <see cref="AuditQueue"/> in the audit database.</summary>
public class AuditWorker(
    AuditQueue queue,
    IDbContextFactory<AuditContext> contextFactory,
    ILogger<AuditWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // No stoppingToken here: on shutdown the queue gets completed and we finish what is left in it.
        await foreach (var entry in queue.Reader.ReadAllAsync(CancellationToken.None))
        {
            try
            {
                await StoreAsync(entry);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not store audit entry {@Entry}", entry);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Complete();
        return base.StopAsync(cancellationToken);
    }

    private async Task StoreAsync(AuditEntry entry)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var httpRequestType = entry.Operation == AuditOperation.Create ? "POST" : "DELETE";
        if (entry.Target == AuditTarget.Claim)
        {
            context.ClaimAudits.Add(new ClaimAudit
            {
                ClaimId = entry.EntityId,
                Created = entry.Created,
                HttpRequestType = httpRequestType
            });
        }
        else
        {
            context.CoverAudits.Add(new CoverAudit
            {
                CoverId = entry.EntityId,
                Created = entry.Created,
                HttpRequestType = httpRequestType
            });
        }

        await context.SaveChangesAsync();
    }
}
