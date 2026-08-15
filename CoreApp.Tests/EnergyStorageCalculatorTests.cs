using CoreApp;
using Entities_DTOs;
using Xunit;

namespace CoreApp.Tests;

public class EnergyStorageCalculatorTests
{
    [Fact]
    public void ApplyGeneratedEnergy_WithAvailableCapacity_StoresEverything()
    {
        var battery = NewBattery(current: 2m, capacity: 10m);

        var overflow = EnergyStorageCalculator.ApplyGeneratedEnergy(
            battery, 3m, new DateTime(2026, 8, 14));

        Assert.Equal(0m, overflow);
        Assert.Equal(5m, battery.CurrentEnergyMWh);
        Assert.Equal(3m, battery.TotalGeneratedMWh);
        Assert.Equal(0m, battery.TotalSaturationLossMWh);
    }

    [Fact]
    public void ApplyGeneratedEnergy_AboveCapacity_RecordsSaturationLoss()
    {
        var battery = NewBattery(current: 8m, capacity: 10m);

        var overflow = EnergyStorageCalculator.ApplyGeneratedEnergy(
            battery, 5m, new DateTime(2026, 8, 14));

        Assert.Equal(3m, overflow);
        Assert.Equal(10m, battery.CurrentEnergyMWh);
        Assert.Equal(5m, battery.TotalGeneratedMWh);
        Assert.Equal(3m, battery.TotalSaturationLossMWh);
    }

    [Fact]
    public void ApplyGeneratedEnergy_WithInvalidBatteryState_Throws()
    {
        var battery = NewBattery(current: 11m, capacity: 10m);

        Assert.Throws<ArgumentException>(() =>
            EnergyStorageCalculator.ApplyGeneratedEnergy(battery, 1m, DateTime.Now));
    }

    private static Battery NewBattery(decimal current, decimal capacity)
    {
        return new Battery
        {
            CurrentEnergyMWh = current,
            MaximumCapacityMWh = capacity,
            Status = "Active"
        };
    }
}
