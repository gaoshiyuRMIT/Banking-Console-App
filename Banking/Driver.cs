using System;
using Microsoft.Extensions.Configuration;


namespace Banking
{
    public class Driver
    {
        private static IConfigurationRoot Config { get; }
        private static string ConnectionString { get; } =
            Config["ConnectionString"];
        private static string CustomerURL { get; } = Config["CustomerAPI"];
        private static string LoginURL { get; } = Config["LoginAPI"];

        public void InitCustomerData() {
        }

        public void InitLoginData() {
        }

        public bool Login(string id, string pwd, out string custId) {}

    }
}
