using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager
{
    delegate bool CheckAccount(Account a, out Exception err);

    public class AccountManager
    {
        internal AccountManagerImpl Impl { get; }
        private TransactionManager TMgr { get; }
        public static Dictionary<char, double> MinBalance { get; } =
            new Dictionary<char, double> { { 'S', 0 }, { 'C', 200 } };
        public static Dictionary<char, double> MinOpeningBalance { get; } =
            new Dictionary<char, double> { { 'S', 100 }, { 'C', 500 } };
        public static char[] Types { get; } = { 'S', 'C' };

        public AccountManager(string connS)
        {
            Impl = new AccountManagerImpl(connS);
            TMgr = new TransactionManager(connS);
        }

        private bool CheckMinBalance(Account a, out Exception err)
        {
            err = null;
            if (a.Balance < MinOpeningBalance[a.AccountType])
            {
                err = new BalanceTooLowException(@"account balance lower than
minimum opening balance allowed");
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

            Impl.Deposit(accNo, amount);
            Transaction depositT = new Transaction
            {
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'D',
                AccountNumber = accNo,
                Amount = amount,
                Comment = comment
            };
            TMgr.AddTransaction(depositT);
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
                throw new BalanceTooLowException(@"after withdrawal,
remaining balance would be lower than the minimum balance allowed.");

            Impl.WithDraw(accNo, amount);
            Transaction t = new Transaction
            {
                TransactionType = 'W',
                AccountNumber = accNo,
                Amount = amount,
                TransactionTimeUtc = DateTime.UtcNow,
                Comment = comment
            };
            TMgr.AddTransaction(t);
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
                throw new BalanceTooLowException(@"after transfer,
remaining balance would be lower than the minimum balance allowed.");

            Impl.Transfer(srcNo, destNo, amount);
            Transaction t = new Transaction
            {
                TransactionTimeUtc = DateTime.UtcNow,
                AccountNumber = srcNo,
                DestinationAccountNumber = destNo,
                Amount = amount,
                Comment = comment,
                TransactionType = 'T'
            };
            TMgr.AddTransaction(t);
        }
    }

    internal class AccountManagerImpl : DBManager {
        public AccountManagerImpl(string connS) : base(connS, "Account") {
        }

        private static Account GetAccountFromReader(SqlDataReader reader)
        {
            return new Account
            {
                AccountNumber = (int)reader["AccountNumber"],
                AccountType = (char)reader["AccountType"],
                CustomerID = (int)reader["CustomerID"],
                Balance = (double)reader["Balance"]
            };
        }

        public void AddAccount(int custId, int accNo, char type, double balance) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"insert into {TableName} (AccountNumber, AccountType, CustomerID, Balance
values (@AccNo, @Type, @CustId, @Balance)";
                command.Parameters.AddWithValue("AccNo", accNo);
                command.Parameters.AddWithValue("Type", type);
                command.Parameters.Add("Balance", SqlDbType.Money)
                    .Value = balance;
                command.Parameters.AddWithValue("CustId", custId);

                command.ExecuteNonQuery();
            }
        }

        public Account GetAccountByAccountNumber(int accNo) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"select * from {TableName}
where AccountNumber = @AccNo";
                command.Parameters.AddWithValue("AccNo", accNo);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return GetAccountFromReader(reader);
                reader.Close();
            }
            return null;
        }

        public List<int> GetAccountNumbersForCustomer(string custId)
        {
            List<int> accNos = new List<int>();
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"select AccountNumber
from {TableName} where CustomerID = @CustId";
                command.Parameters.AddWithValue("CustId", custId);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    accNos.Add((int)reader["AccountNumber"]);
                }
                reader.Close();
            }
            return accNos;
        }

        public void Deposit(int accNo, double amount) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"update {TableName}
set Balance += @Amount
where AccountNumber = @AccNo";
                command.Parameters.Add("Amount", SqlDbType.Money)
                       .Value = amount;
                command.Parameters.AddWithValue("AccNo", accNo);

                command.ExecuteNonQuery();
            }
        }

        public void WithDraw(int accNo, double amount) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"update {TableName}
set Balance -= @Amount
where AccountNumber = @AccNo";
                command.Parameters.Add("Amount", SqlDbType.Money)
                       .Value = amount;
                command.Parameters.AddWithValue("AccNo", accNo);

                command.ExecuteNonQuery();
            }
        }

        public void Transfer(int srcNo, int destNo, double amount) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction("Transfer");
                command.Connection = conn;
                command.Transaction = transaction;
                try
                {
                    command.CommandText = $@"update {TableName}
set Balance += @Amount where AccountNumber = @SrcNo";
                    command.Parameters.AddWithValue("SrcNo", srcNo);
                    command.Parameters.Add("Amount", SqlDbType.Money).Value = amount;
                    command.ExecuteNonQuery();

                    command.CommandText = $@"update {TableName}
set Balance -= @Amount where AccountNumber = @DestNo";
                    command.Parameters.AddWithValue("DestNo", destNo);
                    command.Parameters.Add("Amount", SqlDbType.Money).Value = amount;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
    }
}
