using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Humanizer;

namespace BoothDotDev.Extensions;

/// <summary>
///     Provides extension methods for describing a <see cref="Streak" />'s cadence for display.
/// </summary>
public static class StreakExtensions
{
    /// <param name="streak">The streak, with <see cref="Streak.CurrentDraft" /> loaded.</param>
    extension(Streak streak)
    {
        /// <summary>
        ///     Describes a streak's cadence for its badge, e.g. "daily", "every 2 weeks", "ongoing".
        /// </summary>
        /// <returns>The cadence label.</returns>
        public string CadenceLabel()
        {
            if (streak.Mode == StreakMode.CheckOut)
            {
                return "ongoing";
            }

            var unit = streak.CadenceUnit!.Value;
            var interval = streak.CadenceInterval!.Value;

            return (unit, interval) switch
            {
                (StreakCadenceUnit.Day, 1) => "daily",
                (StreakCadenceUnit.Week, 1) => "weekly",
                (StreakCadenceUnit.Month, 1) => "monthly",
                (StreakCadenceUnit.Month, 3) => "quarterly",
                (StreakCadenceUnit.Year, 1) => "annually",
                _ => $"every {interval} {unit.ToString().ToLowerInvariant().ToQuantity(interval, ShowQuantityAs.None)}"
            };
        }

        /// <summary>
        ///     Describes the unit a streak's count is measured in, e.g. "days", "weeks".
        /// </summary>
        /// <param name="count">The count being labelled, so the unit is correctly singular or plural.</param>
        /// <returns>The unit label.</returns>
        public string UnitLabel(int count)
        {
            var unit = streak.Mode == StreakMode.CheckOut ? StreakCadenceUnit.Day : streak.CadenceUnit!.Value;
            return unit.ToString().ToLowerInvariant().ToQuantity(count, ShowQuantityAs.None);
        }
    }
}
