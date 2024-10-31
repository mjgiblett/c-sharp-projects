using System;
using System.Collections.Generic;

namespace Finance_Manager
{
    /// <summary>
    /// Includes utility methods intended to use or modify <see cref="DateTime"/> structures.
    /// </summary>
    public static class DateTimeModifier
    {
        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="DateTime"/> structures containing
        /// every month between the <paramref name="minimum"/> and <paramref name="maximum"/> bounds, 
        /// including those two months.
        /// </summary>
        /// <param name="minimum">The lower bound.</param>
        /// <param name="maximum">The upper bound.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="DateTime"/> structures in ascending order.</returns>
        public static List<DateTime> MonthsBetweenDates(DateTime minimum, DateTime maximum)
        {
            List<DateTime> months = new List<DateTime>();
            DateTime iterator = minimum < maximum ? FirstDayOfMonth(minimum) : FirstDayOfMonth(maximum);
            DateTime limiter = maximum > minimum ? FirstDayOfMonth(maximum) : FirstDayOfMonth(minimum);
            while (iterator <= limiter)
            {
                months.Add(iterator);
                iterator = iterator.AddMonths(1);
            }
            return months;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="DateTime"/> structure with the specified year and month 
        /// while setting the day to 1. 
        /// </summary>
        /// <param name="date">The initial date.</param>
        /// <returns>The first day of the month.</returns>
        public static DateTime FirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
    }
}
