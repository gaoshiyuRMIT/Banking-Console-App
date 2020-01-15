using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

using Banking.DBManager.Impl;

namespace Banking.DBManager
{
    delegate bool CheckTransaction(Transaction t, out Exception e);

    public interface ITransactionManager
    {
        public static char[] Types { get; }
        public static Dictionary<char, decimal> ServiceFee { get; }
        public static int NFreeTransactions { get; }

        public void AddTransaction(Transaction t);
        public List<Transaction> GetTransactionsForAccount(int accNo,
            int pageSize, int page);
    }

    public class TransactionManager : ITransactionManager
    {
        public static char[] Types { get; } = { 'D', 'W', 'T', 'S' };
        public static Dictionary<char, decimal> ServiceFee { get; } =
            new Dictionary<char, decimal> { { 'W', 0.1M }, { 'T', 0.2M } };
        public static int NFreeTransactions { get; } = 4;
        private ITransactionManagerImpl Impl { get; }

        public TransactionManager(ITransactionManagerImpl impl)
        {
            Impl = impl;
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
                string msg = "for transfer transaction, "
                    + "destination account number must be specified"
                    + " and 4 digits long";
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
            if (nWT >= NFreeTransactions &&
                (t.TransactionType == 'W' || t.TransactionType == 'T'))
            {
                Impl.AddTransaction('S', t.AccountNumber,
                    t.DestinationAccountNumber, ServiceFee[t.TransactionType],
                    "", DateTime.UtcNow);
            }            
        }
        public List<Transaction> GetTransactionsForAccount(int accNo,
            int pageSize, int page)
        {
            if (pageSize <= 0 || page <= 0)
                throw new ArgumentException(
                    "page and page size should be greater than 0");
            return Impl.GetTransactionsForAccount(accNo, pageSize, page);
        }

    }
}
