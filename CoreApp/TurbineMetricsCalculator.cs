using Entities_DTOs;

namespace CoreApp
{
    /// <summary>
    /// Métricas operativas calculadas exclusivamente con intervalos reales registrados.
    /// No se estiman horas de reparación ni se usa el año de fabricación como disponibilidad.
    /// </summary>
    public sealed class TurbineOperationalMetrics
    {
        public DateTime PeriodStart { get; init; }
        public DateTime PeriodEnd { get; init; }
        public int PeriodDays { get; init; }
        public double AvailabilityPercent { get; init; }
        public double UnavailabilityPercent { get; init; }
        public double UptimeHours { get; init; }
        public double DowntimeHours { get; init; }
        public int FailureCount { get; init; }
        public int ResolvedFailureCount { get; init; }
        public double? MeanTimeBetweenFailuresHours { get; init; }
        public double? MeanTimeToRepairHours { get; init; }
        public string CalculationBasis { get; init; } = string.Empty;
    }

    public static class TurbineMetricsCalculator
    {
        public static TurbineOperationalMetrics Calculate(
            IEnumerable<Failure>? failures,
            IEnumerable<Maintenance>? maintenances,
            DateTime periodEnd,
            int periodDays = 30)
        {
            if (periodDays <= 0 || periodDays > 366)
            {
                throw new ArgumentException("El período de métricas debe estar entre 1 y 366 días");
            }

            var periodStart = periodEnd.AddDays(-periodDays);
            var failureList = (failures ?? Array.Empty<Failure>())
                .Where(f => f != null && !string.Equals(f.Status, "Cancelled", StringComparison.Ordinal))
                .ToList();
            var maintenanceList = (maintenances ?? Array.Empty<Maintenance>())
                .Where(m => m != null && !string.Equals(m.Status, "Cancelled", StringComparison.Ordinal))
                .ToList();

            var downtimeIntervals = new List<(DateTime Start, DateTime End)>();

            foreach (var failure in failureList)
            {
                var end = string.Equals(failure.Status, "Resolved", StringComparison.Ordinal) && failure.UpdatedAt.HasValue
                    ? failure.UpdatedAt.Value
                    : periodEnd;
                AddClampedInterval(downtimeIntervals, failure.FailureDate, end, periodStart, periodEnd);
            }

            foreach (var maintenance in maintenanceList.Where(m => m.ActualStartDate.HasValue))
            {
                var end = maintenance.ActualEndDate ??
                    (string.Equals(maintenance.Status, "InProgress", StringComparison.Ordinal)
                        ? periodEnd
                        : maintenance.ActualStartDate!.Value);
                AddClampedInterval(downtimeIntervals, maintenance.ActualStartDate!.Value, end, periodStart, periodEnd);
            }

            var downtimeHours = MergeAndMeasureHours(downtimeIntervals);
            var totalHours = TimeSpan.FromDays(periodDays).TotalHours;
            downtimeHours = Math.Clamp(downtimeHours, 0, totalHours);
            var uptimeHours = totalHours - downtimeHours;

            var failuresInPeriod = failureList
                .Where(f => f.FailureDate >= periodStart && f.FailureDate <= periodEnd)
                .ToList();
            var resolvedRepairHours = failuresInPeriod
                .Where(f => string.Equals(f.Status, "Resolved", StringComparison.Ordinal) &&
                            f.UpdatedAt.HasValue &&
                            f.UpdatedAt.Value > f.FailureDate)
                .Select(f => (f.UpdatedAt!.Value - f.FailureDate).TotalHours)
                .ToList();

            var availability = totalHours == 0 ? 0 : uptimeHours / totalHours * 100;
            var failureCount = failuresInPeriod.Count;

            return new TurbineOperationalMetrics
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PeriodDays = periodDays,
                AvailabilityPercent = Math.Round(availability, 2),
                UnavailabilityPercent = Math.Round(100 - availability, 2),
                UptimeHours = Math.Round(uptimeHours, 2),
                DowntimeHours = Math.Round(downtimeHours, 2),
                FailureCount = failureCount,
                ResolvedFailureCount = resolvedRepairHours.Count,
                MeanTimeBetweenFailuresHours = failureCount == 0
                    ? null
                    : Math.Round(uptimeHours / failureCount, 2),
                MeanTimeToRepairHours = resolvedRepairHours.Count == 0
                    ? null
                    : Math.Round(resolvedRepairHours.Average(), 2),
                CalculationBasis = "Últimos días del período: indisponibilidad derivada de fallas no canceladas y mantenimientos con fechas reales; los intervalos superpuestos se cuentan una sola vez."
            };
        }

        private static void AddClampedInterval(
            ICollection<(DateTime Start, DateTime End)> intervals,
            DateTime start,
            DateTime end,
            DateTime periodStart,
            DateTime periodEnd)
        {
            if (start == default || end <= start || start >= periodEnd || end <= periodStart)
            {
                return;
            }

            intervals.Add((start < periodStart ? periodStart : start, end > periodEnd ? periodEnd : end));
        }

        private static double MergeAndMeasureHours(IEnumerable<(DateTime Start, DateTime End)> intervals)
        {
            var ordered = intervals.OrderBy(interval => interval.Start).ToList();
            if (ordered.Count == 0)
            {
                return 0;
            }

            var total = TimeSpan.Zero;
            var currentStart = ordered[0].Start;
            var currentEnd = ordered[0].End;

            foreach (var interval in ordered.Skip(1))
            {
                if (interval.Start <= currentEnd)
                {
                    if (interval.End > currentEnd)
                    {
                        currentEnd = interval.End;
                    }
                    continue;
                }

                total += currentEnd - currentStart;
                currentStart = interval.Start;
                currentEnd = interval.End;
            }

            total += currentEnd - currentStart;
            return total.TotalHours;
        }
    }
}
