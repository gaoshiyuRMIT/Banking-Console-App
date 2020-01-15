using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Banking.DBManager;
using Banking.DBManager.Impl;

namespace Banking
{
    public class Driver
    {
        private static int TransactionPageSize { get; } = 4;
        private static IConfigurationRoot Config { get; } =
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private string ConnectionString { get; } =
            Config["ConnectionString"];
        private string CustomerURL { get; } = Config["CustomerAPI"];
        private string LoginURL { get; } = Config["LoginAPI"];
        private HttpClient Client { get; } = new HttpClient();

        private IAccountManager AMgr { get; }
        private ITransactionManager TMgr { get; }    
        private ICustomerManager CMgr { get; }
        private ILoginManager LMgr { get; }



        public Driver(bool noInit = false)
        {
            ITransactionManagerImpl tmi =
                new TransactionManagerImpl(ConnectionString);
            IAccountManagerImpl ami =
                new AccountManagerImpl(ConnectionString, tmi);
            ICustomerManagerImpl cmi =
                new CustomerManagerImpl(ConnectionString);
            ILoginManagerImpl lmi =
                new LoginManagerImpl(ConnectionString);

            AMgr = new AccountManager(ami);
            CMgr = new CustomerManager(cmi);
            TMgr = new TransactionManager(tmi);
            LMgr = new LoginManager(lmi, cmi);

            if (!noInit)
                InitDataFromWebService();
        }

        public void GetJsonFromWebService(out string custJson,
            out string loginJson)
        {
            custJson = "[]";
            loginJson = "[]";

            if (!CMgr.AnyCustomer())
                custJson = Client.GetStringAsync(CustomerURL).Result;
            if (!LMgr.AnyLogin())
                loginJson = Client.GetStringAsync(LoginURL).Result;
        }

        public void InitCustomerFromJson(string custJson)
        {
            List<Customer> customers =
                new JsonUtil().Deserialize<List<Customer>>(custJson);
            // add deposit transaction
            customers.ForEach(c =>
                c.Accounts.ForEach(a =>
                    a.Transactions.ForEach(t =>
                    {
                        t.TransactionType = 'D';
                        t.AccountNumber = a.AccountNumber;
                        t.DestinationAccountNumber = a.AccountNumber;
                        t.Amount = a.Balance;
                        t.Comment = "account opening deposit";
                    })));
            customers.ForEach(AddCustomerRecursively);
        }

        public void InitLoginFromJson(string loginJson)
        {
            List<Login> logins =
                new JsonUtil().Deserialize<List<Login>>(loginJson);
            logins.ForEach(LMgr.AddLogin);
        }

        public void InitDataFromWebService()
        {
            string cj, lj;
            GetJsonFromWebService(out cj, out lj);
            InitCustomerFromJson(cj);
            InitLoginFromJson(lj);
        }


        public void AddCustomerRecursively(Customer c)
        {
            CMgr.AddCustomer(c);
            foreach (Account a in c.Accounts)
            {
                AMgr.AddAccount(a);
                foreach (Transaction t in a.Transactions)
                {
                    TMgr.AddTransaction(t);
                }
            }
        }

        public Account GetAccountByAccountNumber(int accNo)
        {
            return AMgr.GetAccountByAccountNumber(accNo);
        }

        public void Deposit(int accNo, decimal amount, string comment)
        {
            AMgr.Deposit(accNo, amount, comment);
        }

        public void WithDraw(int accNo, decimal amount, string comment)
        {
            AMgr.WithDraw(accNo, amount, comment);
        }

        public void Transfer(int srcNo, int destNo, decimal amount,
            string comment)
        {
            AMgr.Transfer(srcNo, destNo, amount, comment);
        }

        public Customer GetCustomerByCustomerID(int custId)
        {
            return CMgr.GetCustomerByCustomerID(custId);
        }

        /*
         * returns whether login is successful
         */
        public bool Login(string loginId, string pwd, out Customer cust)
        {
            return LMgr.Login(loginId, pwd, out cust);
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

    }
}
