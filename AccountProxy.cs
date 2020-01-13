using System;
namespace Banking.DBManager
{

    public class AccountProxy : AccountManager
    {
        public AccountProxy(string connS) : base(connS)
        {

        }

       

        public interface IAcountManager

        {
            public void Deposit(int accNo, double amount, string comment);

            public void WithDraw(int accNo, double amount, string comment);

            public void Transfer(int srcNo, int destNo, double amount,
                string comment);

        }

        class Account : IAcountManager
        {
            private int IAccNo;
            private double IAmount;
            private string IComment;

            public void Deposit(int accNo, double amount, string comment)
            {

                IAccNo = accNo;
                IAmount = amount;
                IComment = comment;

                return;

            }

            public void Transfer(int srcNo, int destNo, double amount, string comment)
            {

            }

            public void WithDraw(int accNo, double amount, string comment)
            {


            }
        }

        class AccountManagerProxy : IAcountManager
        {
            private Account _account = new Account();

            public void Deposit(int accNo, double amount, string comment)
            {
                ((IAcountManager)_account).Deposit(accNo, amount, comment);
            }

            public void Transfer(int srcNo, int destNo, double amount, string comment)
            {
                ((IAcountManager)_account).Transfer(srcNo, destNo, amount, comment);
            }

            public void WithDraw(int accNo, double amount, string comment)
            {
                ((IAcountManager)_account).WithDraw(accNo, amount, comment);
            }
        }

    }

}


   

    

