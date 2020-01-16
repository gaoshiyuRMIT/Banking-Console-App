using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;


namespace Banking.DBManager.Impl
{
    public interface ITransactionManagerImpl
    {
        public void AddTransaction(char type, int accNo, int? destAccNo,
            decimal amount, string comment, DateTime time, SqlCommand command);
        public void AddTransaction(char type, int accNo, int? destAccNo,
            decimal amount, string comment, DateTime time);
        public List<Transaction> GetTransactionsForAccount(int accNo, int pageSize,
            int page);
        // count withdrawals + transfers
        public int CountChargedTransactions();
    }

    public class TransactionManagerImpl : DBManager, ITransactionManagerImpl
    {
        public TransactionManagerImpl(string connS) : base(connS, "[Transaction]")
        {
        }

        private static Transaction GetTransactionFromReader(SqlDataReader reader)
        {
            Transaction t = new Transaction
            {
                TransactionType = reader["TransactionType"]
                    .ToString().ToCharArray()[0],
                AccountNumber = (int)reader["AccountNumber"],
                Amount = (decimal)reader["Amount"]
            };

            t.DestinationAccountNumber =
                reader["DestinationAccountNumber"] is DBNull ?
                    (int?)null : (int)reader["DestinationAccountNumber"];
            t.Comment =
                reader["Comment"] is DBNull ? null : (string)reader["Comment"];
            t.TransactionTimeUtc =
                DateTime.SpecifyKind((DateTime)reader["TransactionTimeUtc"],
                    DateTimeKind.Utc);
            
            return t;
        }

        public void AddTransaction(char type, int accNo, int? destAccNo,
            decimal amount, string comment, DateTime time, SqlCommand command)
        {
            command.CommandText = $@"insert into {TableName}
(TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc)
values (@Type, @AccNo, @DestAccNo, @Amount, @Comment, @Time)";

            DBUtil.AddSqlParam(command.Parameters,
                new Dictionary<string, object>
                {
                    { "Type", type },
                    {"AccNo", accNo },
                    {"DestAccNo", (object)destAccNo ?? DBNull.Value },
                    {"Amount", amount },
                    {"Comment", (object)comment ?? DBNull.Value },
                    {"Time", time }
                });

            command.ExecuteNonQuery();
        }

        public void AddTransaction(char type, int accNo, int? destAccNo,
            decimal amount, string comment, DateTime time)
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
offset @Offset rows
fetch next @PageSize rows only";

                DBUtil.AddSqlParam(command.Parameters,
                    new Dictionary<string, object>
                    {
                        {"AccNo", accNo },
                        {"Offset", offset },
                        {"PageSize", pageSize }
                    });

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