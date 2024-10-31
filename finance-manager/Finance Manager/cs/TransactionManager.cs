using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_Manager.cs
{
    public class TransactionManager
    {
        public List<Transaction> Transactions { get; set; }

        public TransactionManager() { }
        public TransactionManager(params Transaction[] transactions) 
        {
            Transactions = transactions.ToList();
        }

        public List<Transaction> GetTransactionsFromSQL(string command)
        {
            List<Transaction> transactions = new List<Transaction>();

            using (SqlDataReader sqlDataReader = SqlHelper.ExecuteReader(command))
            {
                while (sqlDataReader.Read())
                {
                    transactions.Add(new Transaction(sqlDataReader));
                }
            }
            return transactions;
        }
    }
}
