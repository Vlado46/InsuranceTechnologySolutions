namespace Claims.Application.Interfaces;

public interface IAuditer
{
    void AuditClaim(string id, string httpRequestType);

    void AuditCover(string id, string httpRequestType);
}
