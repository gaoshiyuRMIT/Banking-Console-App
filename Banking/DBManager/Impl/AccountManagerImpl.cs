using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager.Impl {

    public interface IAccountManagerImpl {
        public void AddAccount(int custId, int accNo, char type,
            double balance);
        public Account GetAccountByAccountNumber(int accNo);
        public List<int> GetAccountNumbersForCustomer(string custId);
        public void Deposit(int accNo, double amount, string comment);
        public void WithDraw(int accNo, double amount, string comment);
        public void Transfer(int srcNo, int destNo, double amount,
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

        private void Deposit(int accNo, double amount, SqlCommand command)
        {
            command.CommandText = $@"update {TableName}
set Balance += @Amount
where AccountNumber = @AccNo";
            command.Parameters.Add("Amount", SqlDbType.Money)
                   .Value = amount;
            command.Parameters.AddWithValue("AccNo", accNo);

            command.ExecuteNonQuery();
        }

        public void Deposit(int accNo, double amount, string comment) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    Deposit(accNo, amount, command);
                    TMImpl.AddTransaction('D', accNo, 0, amount, comment,
                        DateTime.UtcNow);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        private void WithDraw(int accNo, double amount, SqlCommand command)
        {
            command.CommandText = $@"update {TableName}
set Balance -= @Amount
where AccountNumber = @AccNo";
            command.Parameters.Add("Amount", SqlDbType.Money)
                   .Value = amount;
            command.Parameters.AddWithValue("AccNo", accNo);

            command.ExecuteNonQuery();
        }

        public void WithDraw(int accNo, double amount, string comment) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction();
                command.Connection = conn;
                command.Transaction = transaction;
                try
                {
                    WithDraw(accNo, amount, command);
                    TMImpl.AddTransaction('W', accNo, 0, amount, comment,
                        DateTime.UtcNow, command);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        private void Transfer(int srcNo, int destNo, double amount,
            SqlCommand command)
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
        }

        public void Transfer(int srcNo, int destNo, double amount,
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
                    Transfer(srcNo, destNo, amount, command);
                    TMImpl.AddTransaction('T', srcNo, destNo, amount, comment,
                        DateTime.UtcNow, command);
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