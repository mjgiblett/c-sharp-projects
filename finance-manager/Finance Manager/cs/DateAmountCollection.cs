using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Finance_Manager
{
    /// <summary>
    /// Represents a collection of <see cref="DateAmount"/> structures, 
    /// typically used to express data on a 2D line graph.
    /// </summary>
    internal abstract class DateAmountCollection
    {
        /// <summary>
        /// The name of the collection. Used as the name of the <see cref="LineSeries"/> on a graph. 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The <see cref="List{T}"/> of <see cref="DateAmount"/> structures used to 
        /// construct a <see cref="LineSeries"/>. 
        /// </summary>
        public List<DateAmount> Collection { get; set; }

        protected virtual List<DateAmount> GetCollection(List<DateTime> months, List<decimal> amounts)
        {
            List<DateAmount> collection = new List<DateAmount>();

            foreach (var point in months.Zip(amounts, Tuple.Create))
            {
                DateAmount dateAmount = new DateAmount(point.Item1, point.Item2);
                collection.Add(dateAmount);
            }

            return collection;
        }
        protected List<decimal> GetAmounts(Transaction[] transactions, List<DateTime> months)
        {
            List<decimal> amounts = Enumerable.Repeat(decimal.Zero, months.Count).ToList();

            foreach (Transaction transaction in transactions.OrderBy(x => x.Date))
            {
                int i = months.IndexOf(DateTimeModifier.FirstDayOfMonth(transaction.Date));
                amounts[i] = ChangeAmount(amounts[i], transaction.Amount);
            }

            return amounts;
        }
        protected abstract decimal ChangeAmount(decimal amount, decimal modifier);
        /// <summary>
        /// Converts the <see cref="DateAmountCollection.Collection"/> property to a 
        /// <see cref="LineSeries"/>.
        /// </summary>
        /// <param name="colour">The colour of the <see cref="LineSeries"/>. </param>
        /// <returns>A <see cref="LineSeries"/> which can express data on a 2D line graph </returns>
        public LineSeries ToLineSeries(Color colour)
        {
            LineSeries lineSeries = new LineSeries
            {
                Label = Name,
                ItemsSource = Collection,
                XBindingPath = "Date",
                YBindingPath = "Amount",
                Interior = new SolidColorBrush(colour)
            };
            return lineSeries;
        }

        public DateAmountCollection() { }
        public DateAmountCollection(params Transaction[] transactions)
        {
            List<DateTime> months = DateTimeModifier.MonthsBetweenDates(transactions.Max(x => x.Date), transactions.Min(x => x.Date));
            List<decimal> amounts = GetAmounts(transactions, months);

            Collection = GetCollection(months, amounts);
        }

        /// <summary>
        /// Represents a collection of <see cref="DateAmount"/> structures containing the total balance of 
        /// all <see cref="Transaction"/>s over time.
        /// </summary>
        internal sealed class Balance : DateAmountCollection
        {
            protected override List<DateAmount> GetCollection(List<DateTime> months, List<decimal> amounts)
            {
                decimal balance = 0;
                List<DateAmount> collection = new List<DateAmount>();

                foreach (var point in months.Zip(amounts, Tuple.Create))
                {
                    balance += point.Item2;
                    collection.Add(new DateAmount(point.Item1, balance));
                }

                return collection;
            }
            protected override decimal ChangeAmount(decimal amount, decimal modifier)
            {
                return amount + modifier;
            }

            public Balance() : base() { Name = "Balance"; }
            public Balance(params Transaction[] transactions) : base(transactions) { Name = "Balance"; }

        }
        /// <summary>
        /// Represents a collection of <see cref="DateAmount"/> structures containing the total credit of 
        /// all <see cref="Transaction"/>s over time.
        /// </summary>
        internal sealed class Credit : DateAmountCollection
        {
            protected override decimal ChangeAmount(decimal amount, decimal modifier)
            {
                if (modifier > 0) { return amount + modifier; }
                return amount;
            }

            public Credit() : base() { Name = "Credit"; }
            public Credit(params Transaction[] transactions) : base(transactions) { Name = "Credit"; }
        }
        /// <summary>
        /// Represents a collection of <see cref="DateAmount"/> structures containing the total debit of 
        /// all <see cref="Transaction"/>s over time.
        /// </summary>
        internal sealed class Debit : DateAmountCollection
        {
            protected override decimal ChangeAmount(decimal amount, decimal modifier)
            {
                if (modifier < 0) { return amount - modifier; }
                return amount;
            }

            public Debit() : base() { Name = "Debit"; }
            public Debit(params Transaction[] transactions) : base(transactions) { Name = "Debit"; }
        }
    }
}
