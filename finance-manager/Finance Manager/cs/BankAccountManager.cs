using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_Manager.cs
{
    public class BankAccountManager
    {
        public List<BankAccount> Accounts { get; set; }

        public BankAccountManager() { }
        public BankAccountManager(params BankAccount[] accounts) 
        {
            Accounts = accounts.ToList();
        }

        public List<BankAccount> GetAccountsFromSQL(string command)
        {
            List<BankAccount> accounts = new List<BankAccount>();

            using (SqlDataReader sqlDataReader = SqlHelper.ExecuteReader(command))
            {
                while (sqlDataReader.Read())
                {
                    accounts.Add(new BankAccount(sqlDataReader));
                }
            }
            return accounts;
        }
    }
}
