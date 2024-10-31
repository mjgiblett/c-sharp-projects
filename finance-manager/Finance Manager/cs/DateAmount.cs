using System;

namespace Finance_Manager
{
    /// <summary>
    /// Represents a 2D data point: an amount at a date.
    /// </summary>
    internal struct DateAmount
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public DateAmount(DateTime date, decimal amount)
        {
            Date = date;
            Amount = amount;
        }
    }
}