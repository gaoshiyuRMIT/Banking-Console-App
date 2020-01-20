using System;
using System.Collections.Generic;

namespace Banking.UINS
{
    public partial class UI
    {
        private void DisplayStatements(Account acc)
        {
            Console.WriteLine($"=== Statements of Account {acc.AccountNumberStr} ===");

            Console.WriteLine("Balance: {0}", acc.Balance);
            foreach (Transaction t in acc.Transactions)
            {
                // avoid printing "null"
                string destS;
                if (t.DestinationAccountNumber is null)
                    destS = string.Empty;
                else
                    destS = $", Destination Account No: {t.DestinationAccountNumberStr}";

                string comment = t.Comment ?? string.Empty;

                // display datetime with local time zone
                string dtS = t.TransactionTimeLocal.ToString(Transaction.DateTimeFormat);

                Console.WriteLine($"Type: {t.TransactionType}, Account No: {t.AccountNumberStr}" +
                    $"{destS}, Amount: {t.Amount}, Time: {dtS}, Comment: {comment}");
            }
        }

        private string GetCommentFromInput()
        {
            Console.Write("Enter comment:    ");
            return Console.ReadLine();
        }


        private int GetAccountNumberFromInput()
        {
            string line;
            int accNo;
            while (true)
            {
                Console.Write("Enter account number:    ");
                line = Console.ReadLine();
                if (!Int32.TryParse(line, out accNo))
                {
                    Console.WriteLine("please enter a whole number");
                    continue;
                }
                if (accNo < 0 || accNo >= 10000)
                {
                    Console.WriteLine("account number must be between 1 and 9999");
                    continue;
                }
                return accNo;
            }
        }

        private decimal GetAmountFromInput()
        {
            string line;
            decimal amount;
            while (true)
            {
                Console.Write("Enter amount:    ");
                line = Console.ReadLine();
                if (!Decimal.TryParse(line, out amount))
                {
                    Console.WriteLine("please enter a decimal");
                    continue;
                }
                if (amount < 0)
                {
                    Console.WriteLine("amount must be greater than zero");
                    continue;
                }
                return amount;
            }
        }

        // returns account number
        private int GetAccountNumberFromInput(List<Account> accs)
        {
            while (true)
            {
                Console.WriteLine("=========== Accounts ============");
                for (int i = 0; i < accs.Count; i++)
                    Console.WriteLine("{3}.    {0} Account {1}\n\tBalance:    {2}",
                        accs[i].AccountTypeStr, accs[i].AccountNumberStr,
                        accs[i].Balance, i + 1);
                Console.WriteLine("=================================");
                Console.Write("Your choice:    ");
                string line = Console.ReadLine();
                int option;
                if (!Int32.TryParse(line, out option))
                {
                    Console.WriteLine("Please enter a whole number");
                    continue;
                }
                if (option < 0 || option > accs.Count)
                {
                    Console.WriteLine("Please enter a number between 1 and " +
                        "{0} to choose from the accounts");
                    continue;
                }
                return accs[option - 1].AccountNumber;
            }
        }

        private void GetLoginDetailFromInput(out string loginId, out string pwd)
        {
            string line;
            while (true)
            {
                Console.Write("Enter Login ID:    ");
                line = Console.ReadLine();
                if (line.Length != 8)
                {
                    Console.WriteLine("login ID should be 8 digits long");
                    continue;
                }
                loginId = line;
                Console.Write("Enter Password:    ");
                if (string.IsNullOrEmpty(line))
                {
                    Console.WriteLine("password can not be empty");
                    continue;
                }
                line = Console.ReadLine();
                pwd = line;
                break;
            }
        }

        private string GetOptionFromInput(string[] menu)
        {
            while (true)
            {
                Console.WriteLine("================ Menu ===============");
                for (int i = 0; i < menu.Length; i++)
                {
                    Console.WriteLine("{0}.    {1}", i + 1, menu[i]);
                }
                Console.WriteLine("=====================================");
                Console.Write("Your choice:    ");
                string line = Console.ReadLine();
                int option;
                if (!Int32.TryParse(line, out option))
                {
                    Console.WriteLine("Please enter a whole number");
                    continue;
                }
                if (option < 1 || option > menu.Length)
                {
                    Console.WriteLine(
                        "Please enter a number between 1 and {0} to choose from the menu",
                        menu.Length);
                    continue;
                }
                return menu[option - 1];
            }
        }
    }
}
