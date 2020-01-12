using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Banking.DBManager;

namespace Banking
{
    public class Driver
    {
        private static IConfigurationRoot Config { get; } =
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static string ConnectionString { get; } =
            Config["ConnectionString"];
        private static string CustomerURL { get; } = Config["CustomerAPI"];
        private static string LoginURL { get; } = Config["LoginAPI"];
        private static int TransactionPageSize { get; } = 4;

        public static AccountManager AMgr { get; }
            = new AccountManager(ConnectionString);
        public static TransactionManager TMgr { get; }
            = new TransactionManager(ConnectionString);
        public static CustomerManager CMgr { get; }
            = new CustomerManager(ConnectionString);
        public static LoginManager LMgr { get; }
            = new LoginManager(ConnectionString);

        public void InitCustomerData() {
        }

        public void InitLoginData() {
        }

        public Account GetPagedStatementByAccountNumber(int accNo, int page)
        {
            Account a = AMgr.GetAccountByAccountNumber(accNo);
            if (a is null)
                throw new KeyNotFoundException(
                    "no account with this account number exists");

            List<Transaction> tHistory = TMgr.GetTransactionsForAccount(accNo,
                TransactionPageSize, page);
            a.Transactions = tHistory;

            return a;
        }

        /*
         * if login is successful,
         *      returns true
         *      out param 'cust' holds the customer who is logged in
         * else
         *      returns false
         *      out param 'cust' = null
         */
        public bool Login(string loginId, string pwd, out Customer cust) {
            return LMgr.CheckCredential(loginId, pwd, out cust);
        }
    }
}
