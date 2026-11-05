using System.Threading.Channels;
using Claims.Application.Contracts;
using Claims.Application.Interfaces;

namespace Claims.Infrastructure.Auditing;

/// <summary>
/// In-memory queue between the requests and <see cref="AuditWorker"/>. Entries still in the queue are
/// lost if the process crashes. When the queue is full, requests wait instead of dropping entries.
/// </summary>
public class AuditQueue : IAuditQueue
{
    private const int Capacity = 10_000;

    private readonly Channel<AuditEntry> _channel = Channel.CreateBounded<AuditEntry>(Capacity);

    public ChannelReader<AuditEntry> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(AuditEntry entry) => _channel.Writer.WriteAsync(entry);

    // TryComplete, because a host can be stopped more than once.
    public void Complete() => _channel.Writer.TryComplete();
}
