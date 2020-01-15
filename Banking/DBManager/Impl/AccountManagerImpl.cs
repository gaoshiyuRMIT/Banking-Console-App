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
        public void WithDraw(int accNo, decimal amount, string comment, decimal fee);
        public void Transfer(int srcNo, int destNo, decimal amount,
            string comment, decimal fee);
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
                DBUtil.AddSqlParam(command.Parameters, new Dictionary<string, object>
                {
                    ["AccNo"] = accNo, ["Type"] = type,
                    ["Balance"] = balance, ["CustId"] = custId
                });
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
                DBUtil.AddSqlParam(command.Parameters, new Dictionary<string, object>
                {
                    ["AccNo"] = accNo
                });

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
                DBUtil.AddSqlParam(command.Parameters,
                    new Dictionary<string, object> { { "CustId", custId } });

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
            DBUtil.AddSqlParam(command.Parameters,
                new Dictionary<string, object>
                {
                    ["Amount"] = amount, ["AccNo"] = accNo
                });

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
                    Deposit(accNo, amount, command);
                    TMImpl.AddTransaction('D', accNo, null, amount, comment,
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
            DBUtil.AddSqlParam(command.Parameters,
                new Dictionary<string, object>
                {
                    ["Amount"] = amount,
                    ["AccNo"] = accNo
                });
            command.ExecuteNonQuery();
        }

        public void WithDraw(int accNo, decimal amount, string comment, decimal serviceFee) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction("Withdraw");
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    WithDraw(accNo, amount, command);
                    TMImpl.AddTransaction('W', accNo, null, amount, comment,
                        DateTime.UtcNow, command);
                    if (serviceFee > 0)
                    {
                        TMImpl.AddTransaction('S', accNo, null, serviceFee,
                            comment, DateTime.UtcNow, command);
                        WithDraw(accNo, serviceFee, command);
                    }

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
            DBUtil.AddSqlParam(command.Parameters,
                new Dictionary<string, object>
                {
                    ["Amount"] = amount,
                    ["SrcNo"] = srcNo
                });
            command.ExecuteNonQuery();

            command.CommandText = $@"update {TableName}
set Balance += @Amount where AccountNumber = @DestNo";
            DBUtil.AddSqlParam(command.Parameters,
                new Dictionary<string, object>
                {
                    ["DestNo"] = destNo
                });
            command.ExecuteNonQuery();
        }

        public void Transfer(int srcNo, int destNo, decimal amount,
            string comment, decimal serviceFee)
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
                    if (serviceFee > 0)
                    {
                        TMImpl.AddTransaction('S', srcNo, destNo, serviceFee,
                            comment, DateTime.UtcNow, command);
                        WithDraw(srcNo, serviceFee, command);
                    }

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