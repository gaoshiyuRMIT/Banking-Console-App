using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

using Banking.DBManager.Impl;

namespace Banking.DBManager
{
    delegate bool CheckAccount(Account a, out Exception err);

    public interface IAccountManager
    {
        public void AddAccount(Account a);
        public Account GetAccountByAccountNumber(int accNo);
        public void Deposit(int accNo, double amount, string comment);
        public void WithDraw(int accNo, double amount, string comment);
        public void Transfer(int srcNo, int destNo, double amount,
            string comment);
    }

    public class AccountManager : IAccountManager
    {
        private IAccountManagerImpl Impl { get; }
        public static Dictionary<char, double> MinBalance { get; } =
            new Dictionary<char, double> { { 'S', 0 }, { 'C', 200 } };
        public static Dictionary<char, double> MinOpeningBalance { get; } =
            new Dictionary<char, double> { { 'S', 100 }, { 'C', 500 } };
        public static char[] Types { get; } = { 'S', 'C' };

        public AccountManager(IAccountManagerImpl i)
        {
            Impl = i;
        }

        private bool CheckMinBalance(Account a, out Exception err)
        {
            err = null;
            if (a.Balance < MinOpeningBalance[a.AccountType])
            {
                err = new BalanceTooLowException("account balance lower"
                    + " than minimum opening balance allowed");
                return false;
            }
            return true;
        }

        private bool CheckAccountNumber(Account a, out Exception err)
        {
            err = null;
            if (a.AccountNumber < 0 || a.AccountNumber >= 10000)
            {
                err = new ArgumentOutOfRangeException(
                    null, "account number must be 4 digits long");
                return false;
            }
            return true;
        }

        private bool CheckCustomerID(Account a, out Exception err)
        {
            err = null;
            if (a.CustomerID < 0 || a.CustomerID >= 10000)
            {
                err = new ArgumentOutOfRangeException(
                    null, "customer id must be 4 digits long");
                return false;
            }
            return true;
        }

        private bool CheckAccountType(Account a, out Exception err )
        {
            err = null;
            if (!Array.Exists(Types, x => x == a.AccountType))
            {
                err = new ArgumentException(
                    string.Format("account type must be one of ({0})",
                        string.Join(", ", Types)));
                return false;
            }
            return true;
        }

        public void AddAccount(Account a)
        {
            Exception err;
            CheckAccount[] rules =
            {
                CheckAccountType, CheckCustomerID,
                CheckAccountNumber, CheckMinBalance
            };
            foreach (var check in rules)
            {
                if (!check(a, out err))
                    throw err;
            }

            Account acc = Impl.GetAccountByAccountNumber(a.AccountNumber);
            if (acc != null)
                throw new DuplicateAccountException(
                    "an account with this account number already exists");

            Impl.AddAccount(a.CustomerID, a.AccountNumber, a.AccountType,
                a.Balance);
        }

        public Account GetAccountByAccountNumber(int accNo)
        {
            return Impl.GetAccountByAccountNumber(accNo);
        }

        public void Deposit(int accNo, double amount, string comment)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "amount to deposit must be greater than 0");

            Account acc = Impl.GetAccountByAccountNumber(accNo);
            if (acc is null)
                throw new ArgumentException(
                    "the specified account does not exist");

            Impl.Deposit(accNo, amount, comment);
        }

        public void WithDraw(int accNo, double amount, string comment)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "amount to withdraw must be greater than 0");

            Account acc = Impl.GetAccountByAccountNumber(accNo);
            if (acc is null)
                throw new ArgumentException(
                    "the specified account does not exist");

            if (acc.Balance - amount < MinBalance[acc.AccountType])
                throw new BalanceTooLowException(
                    "after withdrawal, remaining balance "
                    + "would be lower than the minimum balance allowed.");

            Impl.WithDraw(accNo, amount, comment);
        }

        public void Transfer(int srcNo, int destNo, double amount,
            string comment)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "amount to transfer must be greater than 0");

            Account srcAcc = Impl.GetAccountByAccountNumber(srcNo);
            if (srcAcc is null)
                throw new ArgumentException(
                    "account to transfer from does not exist");

            Account destAcc = Impl.GetAccountByAccountNumber(destNo);
            if (destAcc is null)
                throw new ArgumentException(
                    "account to transfer to does not exist");

            if (srcAcc.Balance - amount < MinBalance[srcAcc.AccountType])
                throw new BalanceTooLowException(
                    "after transfer, remaining balance would be lower"
                    + " than the minimum balance allowed.");

            Impl.Transfer(srcNo, destNo, amount, comment);
        }
    }
}
