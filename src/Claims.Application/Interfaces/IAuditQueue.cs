using Claims.Application.Contracts;

namespace Claims.Application.Interfaces;

/// <summary>Hands audit entries over for storage, so that a request does not wait for the audit database.</summary>
public interface IAuditQueue
{
    /// <summary>
    /// Completes as soon as the entry is queued. Takes no cancellation token on purpose: once a change
    /// is saved, a client disconnecting must not cost us its audit entry.
    /// </summary>
    ValueTask EnqueueAsync(AuditEntry entry);
}
