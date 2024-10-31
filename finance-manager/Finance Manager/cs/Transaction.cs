using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Finance_Manager
{
    public class Transaction
    {
        public int TransactionID { get; set;  }
        public int AccountID { get; }
        public string AccountName { get; }
        public DateTime Date { get; }
        public string DateString { get; }
        public decimal Amount { get; }
        public string Description { get; }

        public SqlParameter[] ToSqlParameterArray()
        {
            SqlParameter[] parameters = new SqlParameter[5]
            {
                 new SqlParameter("@TransactionID", null),
                 new SqlParameter("@AccountID", AccountID),
                 new SqlParameter("@Date", Date),
                 new SqlParameter("@Amount", Amount),
                 new SqlParameter("@Description", Description),
            };

            try { parameters[0].Value = TransactionID; } catch { }
            return parameters;
        }
        /// <summary>
        /// A new transaction without a transactionID.
        /// </summary>
        /// <param name="accountID">The bank account.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount transferred.</param>
        /// <param name="description">A description of the transaction.</param>
        public Transaction(int accountID, DateTime date, decimal amount, string description)
        {
            AccountID = accountID;
            Date = date;
            DateString = date.ToString("yyyy/MM/dd");
            Amount = amount;
            Description = description;
        }
        public Transaction(int transactionID, int accountID, string accountName, DateTime date, decimal amount, string description)
        {
            TransactionID = transactionID;
            AccountID = accountID;
            AccountName = accountName;
            Date = date;
            DateString = date.ToString("yyyy/MM/dd");
            Amount = amount;
            Description = description;
        }

        public Transaction(SqlDataReader sqlDataReader)
        {
            TransactionID = sqlDataReader.GetInt32(0);
            AccountID = sqlDataReader.GetInt32(2);
            AccountName = sqlDataReader.GetString(5);
            Date = sqlDataReader.GetDateTime(1);
            Amount = sqlDataReader.GetDecimal(4);
            Description = sqlDataReader.GetString(3);
        }
    }
}
