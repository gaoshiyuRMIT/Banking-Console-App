using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager.Impl {

    public interface IAccountManagerImpl {
        public void AddAccount(int custId, int accNo, char type,
            decimal balance);
        public Account GetAccountByAccountNumber(int accNo);
        public List<int> GetAccountNumbersForCustomer(string custId);
        public void Deposit(int accNo, decimal amount, string comment);
        public void WithDraw(int accNo, decimal amount, string comment);
        public void Transfer(int srcNo, int destNo, decimal amount,
            string comment);
    }

    public class AccountManagerImpl : DBManager, IAccountManagerImpl
    {
        private ITransactionManagerImpl TMImpl;

        public AccountManagerImpl(string connS, ITransactionManagerImpl tmi)
            : base(connS, "Account")
        {
            TMImpl = tmi;
        }

        private static Account GetAccountFromReader(SqlDataReader reader)
        {
            return new Account
            {
                AccountNumber = (int)reader["AccountNumber"],
                AccountType = reader["AccountType"].ToString().ToCharArray()[0],
                CustomerID = (int)reader["CustomerID"],
                Balance = (decimal)reader["Balance"]
            };
        }

        public void AddAccount(int custId, int accNo, char type, decimal balance)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"insert into {TableName}
(AccountNumber, AccountType, CustomerID, Balance)
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

        private void Deposit(int accNo, decimal amount, SqlCommand command)
        {
            command.CommandText = $@"update {TableName}
set Balance += @Amount
where AccountNumber = @AccNo";
            if (!command.Parameters.Contains("Amount"))
                command.Parameters.Add("Amount", SqlDbType.Money)
                       .Value = amount;
            if (!command.Parameters.Contains("AccNo"))
                command.Parameters.AddWithValue("AccNo", accNo);

            command.ExecuteNonQuery();
        }

        public void Deposit(int accNo, decimal amount, string comment) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction("Deposit");
                command.Transaction = transaction;
                try
                {
                    command.Parameters.AddWithValue("AccNo", accNo);
                    command.Parameters.Add("Amount", SqlDbType.Money)
                        .Value = amount;
                    Deposit(accNo, amount, command);
                    TMImpl.AddTransaction('D', accNo, accNo, amount, comment,
                        DateTime.UtcNow, command);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void WithDraw(int accNo, decimal amount, SqlCommand command)
        {
            command.CommandText = $@"update {TableName}
set Balance -= @Amount
where AccountNumber = @AccNo";
            if (!command.Parameters.Contains("AccNo"))
                command.Parameters.AddWithValue("AccNo", accNo);
            if (!command.Parameters.Contains("Amount"))
                command.Parameters.Add("Amount", SqlDbType.Money)
                    .Value = amount;
            command.ExecuteNonQuery();
        }

        public void WithDraw(int accNo, decimal amount, string comment) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction("Withdraw");
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    SqlParameter pAccNo = new SqlParameter(
                        "AccNo", accNo);
                    SqlParameter pAmount = new SqlParameter(
                        "Amount", SqlDbType.Money);
                    pAmount.Value = amount;
                    command.Parameters.Add(pAccNo);
                    command.Parameters.Add(pAmount);

                    WithDraw(accNo, amount, command);
                    TMImpl.AddTransaction('W', accNo, accNo, amount, comment,
                        DateTime.UtcNow, command);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void Transfer(int srcNo, int destNo, decimal amount,
            SqlCommand command)
        {
            command.CommandText = $@"update {TableName}
set Balance -= @Amount where AccountNumber = @SrcNo";
            if (!command.Parameters.Contains("Amount"))
                command.Parameters.Add("Amount", SqlDbType.Money)
                    .Value = amount;
            if (!command.Parameters.Contains("SrcNo"))
                command.Parameters.AddWithValue("SrcNo", srcNo);
            command.ExecuteNonQuery();

            command.CommandText = $@"update {TableName}
set Balance += @Amount where AccountNumber = @DestNo";
            if (!command.Parameters.Contains("DestNo"))
                command.Parameters.AddWithValue("DestNo", destNo);
            command.ExecuteNonQuery();
        }

        public void Transfer(int srcNo, int destNo, decimal amount,
            string comment)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.Connection = conn;
                SqlTransaction transaction = conn.BeginTransaction("Transfer");
                command.Transaction = transaction;
                try
                {
                    command.Parameters.Add("Amount", SqlDbType.Money)
                        .Value = amount;

                    Transfer(srcNo, destNo, amount, command);
                    TMImpl.AddTransaction('T', srcNo, destNo, amount, comment,
                        DateTime.UtcNow, command);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}