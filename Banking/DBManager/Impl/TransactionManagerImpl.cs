using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;


namespace Banking.DBManager.Impl
{
    public interface ITransactionManagerImpl
    {
        public void AddTransaction(char type, int accNo, int destAccNo,
            double amount, string comment, DateTime time, SqlCommand command);
        public void AddTransaction(char type, int accNo, int destAccNo,
            double amount, string comment, DateTime time);
        public List<Transaction> GetTransactionsForAccount(int accNo, int pageSize,
            int page);
        // count withdrawals + transfers
        public int CountChargedTransactions();
    }

    internal class TransactionManagerImpl : DBManager, ITransactionManagerImpl
    {
        public TransactionManagerImpl(string connS) : base(connS, "[Transaction]")
        {
        }

        private static Transaction GetTransactionFromReader(SqlDataReader reader)
        {
            return new Transaction
            {
                TransactionType = (char)reader["TransactionType"],
                AccountNumber = (int)reader["AccountNumber"],
                DestinationAccountNumber = (int)reader["DestinationAccountNumber"],
                Amount = (double)reader["Amount"],
                Comment = (string)reader["Comment"],
                TransactionTimeUtc = (DateTime)reader["TransactionTimeUtc"]
            };
        }

        public void AddTransaction(char type, int accNo, int destAccNo,
            double amount, string comment, DateTime time, SqlCommand command)
        {
            command.CommandText = $@"insert into {TableName} (TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc)
values (@Type, @AccNo, @DestAccNo, @Amount, @Comment, @Time)";
            command.Parameters.AddWithValue("Type", type);
            command.Parameters.AddWithValue("AccNo", accNo);
            command.Parameters.AddWithValue("DestAccNo", destAccNo);
            command.Parameters.Add("Amount", SqlDbType.Money)
                .Value = amount;
            command.Parameters.AddWithValue("Comment", comment);
            command.Parameters.AddWithValue("Time", time);

            command.ExecuteNonQuery();
        }

        public void AddTransaction(char type, int accNo, int destAccNo,
            double amount, string comment, DateTime time)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                AddTransaction(type, accNo, destAccNo, amount, comment, time,
                    command);
            }
        }

        public List<Transaction> GetTransactionsForAccount(int accNo, int pageSize,
            int page)
        {
            List<Transaction> history = new List<Transaction>();
            int offset = (page - 1) * pageSize;
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"select * from {TableName}
where AccountNumber = @AccNo
order by TransactionTimeUtc desc
offset @Offset
fetch next @PageSize only";
                command.Parameters.AddWithValue("AccNo", accNo);
                command.Parameters.AddWithValue("Offset", offset);
                command.Parameters.AddWithValue("PageSize", pageSize);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transaction t = GetTransactionFromReader(reader);
                    history.Add(t);
                }
                reader.Close();
            }
            return history;
        }

        public int CountChargedTransactions()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"select count(*)
from {TableName} where TransactionType in ('W', 'T')";

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return (int)reader[0];
                reader.Close();
            }
            throw new BankingException("sql count returns 0 rows");
        }
    }

}