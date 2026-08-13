using CoreApp;
using Entities_DTOs;
using Xunit;

namespace CoreApp.Tests;

public class TurbineMetricsCalculatorTests
{
    private static readonly DateTime PeriodEnd = new(2026, 8, 12, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Calculate_WithoutEvents_ReturnsFullAvailabilityAndUndefinedMeans()
    {
        var result = TurbineMetricsCalculator.Calculate([], [], PeriodEnd);

        Assert.Equal(100, result.AvailabilityPercent);
        Assert.Equal(0, result.UnavailabilityPercent);
        Assert.Equal(720, result.UptimeHours);
        Assert.Equal(0, result.DowntimeHours);
        Assert.Null(result.MeanTimeBetweenFailuresHours);
        Assert.Null(result.MeanTimeToRepairHours);
    }

    [Fact]
    public void Calculate_OverlappingFailureAndMaintenance_CountsDowntimeOnce()
    {
        var failureStart = PeriodEnd.AddDays(-10);
        var failures = new[]
        {
            new Failure
            {
                FailureDate = failureStart,
                Status = "Resolved",
                UpdatedAt = failureStart.AddHours(4)
            }
        };
        var maintenances = new[]
        {
            new Maintenance
            {
                ActualStartDate = failureStart.AddHours(2),
                ActualEndDate = failureStart.AddHours(6),
                Status = "Completed"
            }
        };

        var result = TurbineMetricsCalculator.Calculate(failures, maintenances, PeriodEnd);

        Assert.Equal(6, result.DowntimeHours);
        Assert.Equal(714, result.UptimeHours);
        Assert.Equal(99.17, result.AvailabilityPercent);
        Assert.Equal(1, result.FailureCount);
        Assert.Equal(1, result.ResolvedFailureCount);
        Assert.Equal(714, result.MeanTimeBetweenFailuresHours);
        Assert.Equal(4, result.MeanTimeToRepairHours);
    }

    [Fact]
    public void Calculate_OpenFailure_UsesPeriodEndWithoutInventingRepairDuration()
    {
        var failures = new[]
        {
            new Failure
            {
                FailureDate = PeriodEnd.AddHours(-12),
                Status = "InProgress"
            }
        };

        var result = TurbineMetricsCalculator.Calculate(failures, [], PeriodEnd);

        Assert.Equal(12, result.DowntimeHours);
        Assert.Equal(708, result.UptimeHours);
        Assert.Equal(1, result.FailureCount);
        Assert.Equal(0, result.ResolvedFailureCount);
        Assert.Null(result.MeanTimeToRepairHours);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(367)]
    public void Calculate_InvalidPeriod_Throws(int periodDays)
    {
        Assert.Throws<ArgumentException>(() =>
            TurbineMetricsCalculator.Calculate([], [], PeriodEnd, periodDays));
    }
}
