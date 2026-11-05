namespace Claims.Domain;

/// <summary>
/// Computes the premium of a cover. The day rate depends on the cover type and gets cheaper in three
/// phases: the first 30 days, the following 150 days, and every day after that.
/// </summary>
public static class PremiumCalculator
{
    private const decimal BaseDayRate = 1250m;
    private const int FirstPhaseDays = 30;
    private const int SecondPhaseDays = 150;

    /// <param name="startDate">First insured day.</param>
    /// <param name="endDate">Day the cover ends. It is not charged, so 1 to 31 January is 30 days.</param>
    /// <param name="coverType">Type of the insured object.</param>
    public static decimal Compute(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var insuredDays = Math.Max((endDate.Date - startDate.Date).Days, 0);

        var firstPhase = Math.Min(insuredDays, FirstPhaseDays);
        var secondPhase = Math.Min(insuredDays - firstPhase, SecondPhaseDays);
        var thirdPhase = insuredDays - firstPhase - secondPhase;

        var (secondPhaseDiscount, thirdPhaseDiscount) = GetDiscounts(coverType);
        var dayRate = BaseDayRate * GetTypeMultiplier(coverType);

        return firstPhase * dayRate
            + secondPhase * dayRate * (1 - secondPhaseDiscount)
            + thirdPhase * dayRate * (1 - thirdPhaseDiscount);
    }

    private static decimal GetTypeMultiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m
    };

    // The third phase discount comes on top of the second one: 5% + 3% for yachts, 2% + 1% for the rest.
    private static (decimal SecondPhase, decimal ThirdPhase) GetDiscounts(CoverType coverType) =>
        coverType == CoverType.Yacht ? (0.05m, 0.08m) : (0.02m, 0.03m);
}
