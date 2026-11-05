using Claims.Domain;
using Xunit;

namespace Claims.UnitTests.Domain;

public class PremiumCalculatorTests
{
    private static readonly DateTime StartDate = new(2027, 1, 1);

    public static TheoryData<CoverType, int, decimal> Premiums => new()
    {
        // Yacht: 1375 a day, -5% from day 31, -8% from day 181
        { CoverType.Yacht, 1, 1_375m },
        { CoverType.Yacht, 30, 41_250m },
        { CoverType.Yacht, 31, 42_556.25m },
        { CoverType.Yacht, 180, 237_187.50m },
        { CoverType.Yacht, 181, 238_452.50m },
        { CoverType.Yacht, 365, 471_212.50m },
        { CoverType.Yacht, 366, 472_477.50m },

        // Passenger ship: 1500 a day, -2% from day 31, -3% from day 181
        { CoverType.PassengerShip, 30, 45_000m },
        { CoverType.PassengerShip, 31, 46_470m },
        { CoverType.PassengerShip, 180, 265_500m },
        { CoverType.PassengerShip, 181, 266_955m },
        { CoverType.PassengerShip, 365, 534_675m },

        // Tanker: 1875 a day
        { CoverType.Tanker, 30, 56_250m },
        { CoverType.Tanker, 180, 331_875m },
        { CoverType.Tanker, 366, 670_162.50m },

        // Other types: 1625 a day
        { CoverType.ContainerShip, 30, 48_750m },
        { CoverType.ContainerShip, 180, 287_625m },
        { CoverType.BulkCarrier, 365, 579_231.25m }
    };

    [Theory]
    [MemberData(nameof(Premiums))]
    public void Compute_ReturnsProgressivePremium(CoverType coverType, int days, decimal expected)
    {
        var premium = PremiumCalculator.Compute(StartDate, StartDate.AddDays(days), coverType);

        Assert.Equal(expected, premium);
    }

    [Fact]
    public void Compute_CountsCalendarDays_AndIgnoresTheTimeOfDay()
    {
        var premium = PremiumCalculator.Compute(StartDate.AddHours(8), StartDate.AddDays(30).AddHours(20), CoverType.Yacht);

        Assert.Equal(41_250m, premium);
    }

    [Fact]
    public void Compute_ReturnsZero_WhenEndDateIsBeforeStartDate()
    {
        var premium = PremiumCalculator.Compute(StartDate, StartDate.AddDays(-1), CoverType.Yacht);

        Assert.Equal(0m, premium);
    }
}
