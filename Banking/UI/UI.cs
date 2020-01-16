using System;
using System.Collections.Generic;


namespace Banking.UINS
{
    public partial class UI
    {
        private Customer customer = null;
        private Driver driver;

        public UI()
        {
            try
            {
                driver = new Driver();
            }
            catch (BankingException)
            {
                Console.WriteLine("Initialization failed.");
                throw;
            }
        }


        public void Index()
        {
            string[] menu =
            {
                "Login", "Quit"
            };
            string option;

            while (true)
            {
                option = GetOptionFromInput(menu);
                if (option == "Login")
                {
                    bool sustain = Login();
                    if (sustain)
                    {
                        Console.Clear();
                        continue;
                    }
                }
                return;
            }
        }


        public bool Login()
        {
            string loginId, pwd;
            while (true)
            {
                GetLoginDetailFromInput(out loginId, out pwd);
                bool success;
                try
                {
                    success = driver.Login(loginId, pwd, out customer);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Login failed. {e.Message}");
                    continue;
                }
                if (!success)
                {
                    Console.WriteLine("Login failed. Please check your credentials.");
                    continue;
                }
                break;
            }
            Console.WriteLine("Welcome, {0}!", customer.Name);
            string[] menu =
            {
                "Deposit", "Withdraw", "Transfer", "My Statements",
                "Logout", "Quit"
            };
            string option;
            while (true)
            {
                option = GetOptionFromInput(menu);
                if (option == "Logout")
                {
                    return true;
                }
                else if (option == "Quit")
                    return false;
                else if (option == "Transfer")
                {
                    Transfer();
                }
                else if (option == "My Statements")
                {
                    MyStatements();
                }
                else if (option == "Withdraw")
                {
                    Withdraw();
                }
                else if (option == "Deposit")
                {
                    Deposit();
                }
            }
        }



        public void Transfer()
        {
            List<Account> accs = driver.GetAccountsForCustomer(customer.CustomerID);
            Console.WriteLine("Source:  ");
            int accNo = GetAccountNumberFromInput(accs);
            Console.Write("Destination:  ");
            int destAccNo = GetAccountNumberFromInput();
            decimal amount = GetAmountFromInput();
            string comment = GetCommentFromInput();
            try
            {
                driver.Transfer(accNo, destAccNo, amount, comment);
                Console.WriteLine("Transfer success.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Transfer failed.");
                Console.WriteLine(e.Message);
            }
        }

        public void Withdraw()
        {
            List<Account> accs = driver.GetAccountsForCustomer(customer.CustomerID);
            int accNo = GetAccountNumberFromInput(accs);
            decimal amount = GetAmountFromInput();
            string comment = GetCommentFromInput();
            try
            {
                driver.WithDraw(accNo, amount, comment);
                Console.WriteLine("Withdrawal success.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Withdrawal failed. {e.Message}");
            }
        }

        public void Deposit()
        {
            List<Account> accs = driver.GetAccountsForCustomer(customer.CustomerID);
            int accNo = GetAccountNumberFromInput(accs);
            decimal amount = GetAmountFromInput();
            string comment = GetCommentFromInput();
            try
            {
                driver.Deposit(accNo, amount, comment);
                Console.WriteLine("Deposit success.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Deposit failed. {e.Message}");
            }

        }


        public void MyStatements()
        {
            List<Account> accs = driver.GetAccountsForCustomer(customer.CustomerID);
            int accNo = GetAccountNumberFromInput(accs);
            Account acc;
            int page = 1;
            while (true)
            {
                acc = driver.GetPagedStatementByAccountNumber(accNo, page);
                DisplayStatements(acc);
                string option;
                // prepare menu
                List<string> menuL = new List<string> { "Back" };
                if (page > 1)
                    menuL.Add("Previous");
                if (acc.Transactions.Count == 4)
                    menuL.Add("Next");
                // prepare menu done

                Console.WriteLine($"(page {page})");
                option = GetOptionFromInput(menuL.ToArray());
                if (option == "Back")
                    return;
                else if (option == "Previous")
                {
                    page--;
                    continue;
                }
                else
                {
                    page++;
                    continue;
                }
            }
        }
    }
}
