using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager.Impl
{
    public interface ICustomerManagerImpl
    {
        public Customer GetCustomerByCustomerID(int id);
        public void AddCustomer(int id, string name, object address,
            object city, object postcode);
        public int CountCustomer();
    }

    public class CustomerManagerImpl : DBManager, ICustomerManagerImpl
    {

        public CustomerManagerImpl(string connS) : base(connS, "Customer")
        {
        }

        private static Customer GetCustomerFromReader(SqlDataReader reader)
        {
            Customer c = new Customer();
            c.CustomerID = (int)reader["CustomerID"];
            c.Name = (string)reader["Name"];
            c.Address =
                reader["Address"] is DBNull ? null : (string)reader["Address"];
            c.City =
                reader["City"] is DBNull ? null : (string)reader["City"];
            c.PostCode =
                reader["PostCode"] is DBNull ? null : (string)reader["PostCode"];
            return c;
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

        public void AddCustomer(int id, string name, object address,
            object city, object postcode)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"insert into {TableName}
(CustomerID, Name, Address, City, PostCode)
values (@Id, @Name, @Address, @City, @Postcode)";
                command.Parameters.Add("Id", SqlDbType.Int).Value = id;
                command.Parameters.Add("Name", SqlDbType.NVarChar)
                    .Value = name;
                command.Parameters.AddWithValue("Address",
                    address ?? DBNull.Value);
                command.Parameters.Add("City", SqlDbType.NVarChar)
                    .Value = city ?? DBNull.Value;
                command.Parameters.Add("Postcode", SqlDbType.NVarChar)
                    .Value = postcode ?? DBNull.Value;

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