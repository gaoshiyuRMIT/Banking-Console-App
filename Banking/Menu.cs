using System;
namespace Banking
{
    public class Menu
    {
        private DBManager DBmanager { get; }

        public Menu(string connS, string tn)
        {
            DBmanager = new DBManager(connS, tn);
        }

        public void Run()
        {
            while (true)
            {
                Console.Write(
                    @"Welcome to NWBA Banking System
                     ===============================
                    1. Login
                    2. Quit
                    Select an option: 
                    "
                    );

                var input = Console.ReadLine();
                Console.WriteLine();

                if (!int.TryParse(input, out var option) || !option.IsInRange(1, 2))
                {
                    Console.WriteLine("Invalid input.");
                    Console.WriteLine();
                    continue;
                }

                switch (option)
                {
                    case 1:
                        //Login();
                        LoginMenu();
                        break;
                    case 2:
                        Console.WriteLine("Thank you for using NWBA Banking System");
                        return;

                }
            }
        }

        public void LoginMenu()
        {
            while (true)
            {
                Console.Write(
                    @"Welcome to NWBA Banking System
                     ===============================
                    1. Check Balance
                    2. Transaction History
                    3. Withdraw
                    4. Deposit
                    5. Transfer to Account
                    6. Quit
                    Select an option: 
                    "
                    );

                var input = Console.ReadLine();
                Console.WriteLine();

                if (!int.TryParse(input, out var option) || !option.IsInRange(1, 2))
                {
                    Console.WriteLine("Invalid input.");
                    Console.WriteLine();
                    continue;
                }
                switch (option)
                {
                    case 1:
                        CheckBalance();
                        break;
                    case 2:
                        CheckTransactionHistory();
                        break;
                    case 3:
                        Withdraw();
                        break;
                    case 4:
                        Deposit();
                    case 5:
                        Transfer();
                    case 6:
                        Console.WriteLine("Thank you for using NWBA Banking System");
                        return;

                }


            }


        }
    }
}
