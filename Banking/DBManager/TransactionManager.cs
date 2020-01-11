using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager
{
    delegate bool CheckTransaction(Transaction t, out Exception e);

    public class TransactionManager
    {
        internal TransactionManagerImpl Impl { get; }
        public static char[] Types { get; } = { 'D', 'W', 'T', 'S' };
        public static double WithdrawFee { get; } = 0.1;
        public static double TransferFee { get; } = 0.2;
        public static int NFreeTransactions { get; } = 4;

        public TransactionManager(string connS)
        {
            Impl = new TransactionManagerImpl(connS);
        }

        private bool CheckTransactionType(Transaction t, out Exception err)
        {
            err = null;
            if (!Array.Exists(Types, x => x == t.TransactionType))
            {
                string m = string.Format("transaction type must be one of ({0}).",
                    string.Join(", ", Types));
                err = new ArgumentException(m);
                return false;
            }
            return true;
        }

        private bool CheckAmount(Transaction t, out Exception err)
        {
            err = null;
            if (t.Amount <= 0)
            {
                err = new ArgumentOutOfRangeException(
                    null, "transaction amount must be greater than zero");
                return false;
            }
            return true;
        }

        private bool CheckAccountNumber(Transaction t, out Exception err)
        {
            err = null;
            if (t.AccountNumber < 0 || t.AccountNumber >= 10000)
            {
                err = new ArgumentOutOfRangeException(
                    null, "account number must be 4 digits long");
                return false;
            }
            return true;
        }

        private bool CheckDestinationAccountNumber(Transaction t,
            out Exception err)
        {
            err = null;
            int destNo = t.DestinationAccountNumber;
            if (t.TransactionType == 'T' && !(destNo >= 0 && destNo < 10000))
            {
                string msg = @"for transfer transaction, destination account number
must be specified and 4 digits long";
                err = new ArgumentException(msg);
                return false;
            }
            return true;
        }

        public void AddTransaction(Transaction t)
        {
            Exception err;
            CheckTransaction[] rules =
            {
                CheckTransactionType,
                CheckAccountNumber,
                CheckAccountNumber
            };
            foreach (var check in rules)
                if (!check(t, out err))
                    throw err;

            int nWT = Impl.CountChargedTransactions();

            Impl.AddTransaction(t.TransactionType, t.AccountNumber,
                                t.DestinationAccountNumber, t.Amount,
                                t.Comment, t.TransactionTimeUtc);
            if (nWT >= NFreeTransactions)
            {
                if (t.TransactionType == 'W')
                    Impl.AddTransaction('S', t.AccountNumber,
                        t.DestinationAccountNumber,
                        WithdrawFee, "", DateTime.UtcNow);
                else if (t.TransactionType == 'T')
                    Impl.AddTransaction('S', t.AccountNumber,
                        t.DestinationAccountNumber,
                        TransferFee, "", DateTime.UtcNow);    
            }            
        }
        public List<Transaction> GetTransactionsForAccount(int accNo, int pageSize,
    int page)
        {
            if (pageSize <= 0 || page <= 0)
                throw new ArgumentException(
                    "page and page size should be greater than 0");
            return Impl.GetTransactionsForAccount(accNo, pageSize, page);
        }

    }

    internal class TransactionManagerImpl : DBManager
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
            double amount, string comment, DateTime time)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
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
        }

        // TODO: check page & pageSize > 0
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
