using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager.Impl
{
    public interface ICustomerManagerImpl
    {
        public Customer GetCustomerByCustomerID(int id);
        public void AddCustomer(int id, string name, string address,
            string city, string postcode);
        public int CountCustomer();
    }

    internal class CustomerManagerImpl : DBManager, ICustomerManagerImpl
    {

        public CustomerManagerImpl(string connS) : base(connS, "Customer")
        {
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
            string city, string postcode)
        {
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

        public int CountCustomer()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $"select count(*) from {TableName}";

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return (int)reader[0];
                reader.Close();
            }
            throw new BankingException("sql count() returns 0 rows");
        }
    }
}