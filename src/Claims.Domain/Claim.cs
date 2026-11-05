using System.Text.Json.Serialization;

namespace Claims.Domain
{
    public class Claim
    {
        public required string Id { get; init; }

        public required string CoverId { get; init; }

        public DateTime Created { get; init; }

        public required string Name { get; init; }

        public ClaimType Type { get; init; }

        public decimal DamageCost { get; init; }
    }

    public enum ClaimType
    {
        Collision = 0,
        Grounding = 1,
        BadWeather = 2,
        Fire = 3
    }
}
