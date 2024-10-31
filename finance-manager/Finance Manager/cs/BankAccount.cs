using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_Manager
{
    public class BankAccount
    {
        public int AccountID { get; }
        public int AccountNumber { get; set; }
        public int BSB { get; set; }
        public string Name { get; set; }
        public string Bank { get; set; }
        public string AccountType { get; set; }

        public BankAccount(int accountID, string name)
        {
            AccountID = accountID;
            Name = name;
        }
        public BankAccount(int accountID, int accountNumber, int BSB, string name, string bank, string accountType)
        {
            AccountID = accountID;
            AccountNumber = accountNumber;
            this.BSB = BSB;
            Name = name;
            Bank = bank;
            AccountType = accountType;
        }
        public BankAccount(SqlDataReader sqlDataReader)
        {
            AccountID = sqlDataReader.GetInt32(0);
            AccountNumber = sqlDataReader.GetInt32(1);
            BSB = sqlDataReader.GetInt32(2);
            Name = sqlDataReader.GetString(3);
            Bank = sqlDataReader.GetString(4);
            AccountType = sqlDataReader.GetString(5);
        }
    }
}
