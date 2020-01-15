using System;
using System.Collections.Generic;

namespace Banking {
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public List<Account> Accounts { get; set; } =
            new List<Account>();
    }
    public class Account
    {
        public int AccountNumber { get; set; }
        public char AccountType { get; set; }
        public int CustomerID { get; set; }
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; } =
            new List<Transaction>();
    }
    public class Transaction
    {
        private DateTime _transactionTimeUtc;

        public DateTime TransactionTimeUtc {
            get => _transactionTimeUtc;
            set => _transactionTimeUtc = value.ToUniversalTime();
        }
        public char TransactionType { get; set; }
        public int AccountNumber { get; set; }
        public int? DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }

        // helper properties
        public DateTime TransactionTimeLocal =>
            _transactionTimeUtc.ToLocalTime();
    }
    public class Login
    {
        public string LoginID { get; set; }
        public int CustomerID { get; set; }
        public string PasswordHash { get; set; }
    }
}