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
                DBUtil.AddSqlParam(command.Parameters,
                    new Dictionary<string, object>
                    {
                        ["Id"] = id
                    });

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

                DBUtil.AddSqlParam(command.Parameters,
                    new Dictionary<string, object>
                    {
                        ["Id"] = id,
                        ["Name"] = name,
                        ["Address"] = (object)address ?? DBNull.Value,
                        ["City"] = (object)city ?? DBNull.Value,
                        ["Postcode"] = (object)postcode ?? DBNull.Value
                    });

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