using Entities_DTOs;

namespace CoreApp
{
    public static class EnergyStorageCalculator
    {
        public static decimal ApplyGeneratedEnergy(Battery battery, decimal generatedEnergy, DateTime updatedAt)
        {
            ArgumentNullException.ThrowIfNull(battery);

            if (generatedEnergy < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(generatedEnergy));
            }

            if (battery.MaximumCapacityMWh < 0 || battery.CurrentEnergyMWh < 0 ||
                battery.CurrentEnergyMWh > battery.MaximumCapacityMWh)
            {
                throw new ArgumentException("El estado energético de la batería no es válido", nameof(battery));
            }

            var availableSpace = battery.MaximumCapacityMWh - battery.CurrentEnergyMWh;
            var stored = Math.Min(generatedEnergy, availableSpace);
            var overflow = generatedEnergy - stored;

            battery.CurrentEnergyMWh += stored;
            battery.TotalGeneratedMWh += generatedEnergy;
            battery.TotalSaturationLossMWh += overflow;
            battery.UpdatedAt = updatedAt;
            return overflow;
        }
    }
}
