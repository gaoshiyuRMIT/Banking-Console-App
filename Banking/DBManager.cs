using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;


namespace Banking {

    public class DBManager {
        public string TableName { get; }
        public string ConnStr { get; }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        public DBManager(string connS, string tn) {
            ConnStr = connS;
            TableName = tn;
        }
    }

    public class CustomerManager : DBManager {
        public CustomerManager(string connS) : base(connS, "Customer") {
        }

        public void AddCustomer(string id, string name, string address,
            string city, string postcode) {
            using (var conn = GetConnection())
            {
                conn.Open();
                // insert
            }
        }
    }

    public class AccountManager : DBManager {
        public AccountManager(string connS) : base(connS, "Account") {
        }

        public void AddAccount(string custId, string accNo, char type, double balance) {
        }

        public Account GetAccountByAccountNumber(string accNo) {
        }

        public string[] GetAccountNumbersForCustomer(string custId) {}

        public void Deposit(string accNo, double amount) {}

        public void WithDraw(string accNo, double amount) {}

        public void Transfer(string srcNo, string destNo, double amount) {
        }
    }

    public class TransactionManager : DBManager {
        public TransactionManager(string connS) : base(connS, "[Transaction]") {
        }

        public void AddTransaction(char type, string accNo, string destAccNo,
            double amount, string comment, DateTime time) {
        }

        public List<Transaction > GetTransactionsForCustomer(string custId, int pageSize,
            int page) {
        }
    }
}