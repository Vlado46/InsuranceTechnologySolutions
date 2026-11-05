namespace Claims.Domain;

public class Cover
{
    public required string Id { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public CoverType Type { get; init; }

    public decimal Premium { get; init; }
}

public enum CoverType
{
    Yacht = 0,
    PassengerShip = 1,
    ContainerShip = 2,
    BulkCarrier = 3,
    Tanker = 4
}
