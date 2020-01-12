using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager
{
    delegate bool CheckCustomer(Customer c, out Exception e);

    public class CustomerManager
    {
        internal CustomerManagerImpl Impl { get; }

        public CustomerManager(string connS)
        {
            Impl = new CustomerManagerImpl(connS);
        }

        public bool CheckCustomerID(Customer c, out Exception err)
        {
            err = null;
            if (c.CustomerID < 0 || c.CustomerID >= 10000)
            {
                err = new ArgumentOutOfRangeException(null,
                    "customer id must be 4 digits long");
                return false;
            }
            return true;
        }

        public void AddCustomer(Customer c)
        {
            Exception err;
            CheckCustomer[] rules = { CheckCustomerID };
            foreach (var check in rules)
                if (!check(c, out err))
                    throw err;

            Customer customer = Impl.GetCustomerByCustomerID(c.CustomerID);
            if (customer != null)
                throw new DuplicateCustomerException(
                    "a customer with the specified CustomerID already exists.");

            Impl.AddCustomer(c.CustomerID, c.Name, c.Address, c.City,
                c.PostCode);
        }

        public Customer GetCustomerByCustomerID(int custId)
        {
            return Impl.GetCustomerByCustomerID(custId);
        }
    }

    internal class CustomerManagerImpl : DBManager {

        public CustomerManagerImpl(string connS) : base(connS, "Customer") {
        }

        private static Customer GetCustomerFromReader(SqlDataReader reader)
        {
            return new Customer
            {
                CustomerID = (int)reader["CustomerID"],
                Name = (string)reader["Name"],
                Address = (string)reader["Address"],
                City = (string)reader["City"],
                PostCode = (string)reader["PostCode"]
            };
        }

        public Customer GetCustomerByCustomerID(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"select * from {TableName}
where CustomerID = @Id";
                command.Parameters.AddWithValue("Id", id);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return GetCustomerFromReader(reader);
                reader.Close();
            }
            return null;
        }
        public void AddCustomer(int id, string name, string address,
            string city, string postcode) {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"insert into {TableName}
(CustomerID, Name, Address, City, PostCode)
values (@Id, @Name, @Address, @City, @Postcode)";
                command.Parameters.AddWithValue("Id", id);
                command.Parameters.AddWithValue("Name", name);
                command.Parameters.AddWithValue("Address", address);
                command.Parameters.AddWithValue("City", city);
                command.Parameters.AddWithValue("Postcode", postcode);

                command.ExecuteNonQuery();
            }
        }


    }

}
