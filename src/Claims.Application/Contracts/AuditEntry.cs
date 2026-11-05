namespace Claims.Application.Contracts;

/// <summary>A change that has to end up in the audit log.</summary>
/// <param name="Target">Kind of entity that changed.</param>
/// <param name="EntityId">Id of the entity that changed.</param>
/// <param name="Operation">What happened to the entity.</param>
/// <param name="Created">When the change happened, not when the entry gets stored.</param>
public record AuditEntry(AuditTarget Target, string EntityId, AuditOperation Operation, DateTime Created);

public enum AuditTarget
{
    Claim,
    Cover
}

public enum AuditOperation
{
    Create,
    Delete
}
