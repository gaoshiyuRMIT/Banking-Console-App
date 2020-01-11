using System;
namespace Banking
{
    public class BankingException : Exception
    {
        public BankingException() : base()
        {
        }

        public BankingException(string msg) : base(msg)
        {
        }

        public BankingException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }

    public class BalanceTooLowException : BankingException
    {
        public BalanceTooLowException() : base()
        {
        }

        public BalanceTooLowException(string msg) : base(msg)
        {
        }

        public BalanceTooLowException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }

    public class DuplicateAccountException : BankingException
    {
        public DuplicateAccountException() : base()
        {
        }

        public DuplicateAccountException(string msg) : base(msg)
        {
        }

        public DuplicateAccountException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }

    public class DuplicateCustomerException : BankingException
    {
        public DuplicateCustomerException() : base()
        {
        }

        public DuplicateCustomerException(string msg) : base(msg)
        {
        }

        public DuplicateCustomerException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }

}
