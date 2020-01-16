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

        public string CustomerIDStr
        {
            get => CustomerID.ToString().PadLeft(4, '0');
        }

    }
    public class Account
    {
        public int AccountNumber { get; set; }
        public char AccountType { get; set; }
        public int CustomerID { get; set; }
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; } =
            new List<Transaction>();

        public string AccountNumberStr
        {
            get => AccountNumber.ToString().PadLeft(4, '0');
        }
        public string AccountTypeStr
        {
            get => AccountType == 'C' ? "Checking" : "Savings";
        }

    }
    public class Transaction
    {
        public const string DateTimeFormat = "dd/MM/yyyy hh:mm:ss tt";

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
        public string AccountNumberStr
        {
            get => AccountNumber.ToString().PadLeft(4, '0');
        }
        public string DestinationAccountNumberStr
        {
            get => DestinationAccountNumber.ToString().PadLeft(4, '0');
        }
    }
    public class Login
    {
        public string LoginID { get; set; }
        public int CustomerID { get; set; }
        public string PasswordHash { get; set; }
    }
}