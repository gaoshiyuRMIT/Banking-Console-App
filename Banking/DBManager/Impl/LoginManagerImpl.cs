using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;


namespace Banking.DBManager.Impl
{
    public interface ILoginManagerImpl
    {
        public void AddLogin(int custId, string loginId, string pwdHash);
        public string GetCredential(string loginId, out int custId);
        public int CountLogin();
    }

    public class LoginManagerImpl : DBManager, ILoginManagerImpl
    {
        public LoginManagerImpl(string connS) : base(connS, "Login")
        {
        }

        public void AddLogin(int custId, string loginId, string pwdHash)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"insert into {TableName}
(LoginID, CustomerID, PasswordHash)
values (@LoginId, @CustId, @PwdHash)";
                command.Parameters.AddWithValue("LoginId", loginId);
                command.Parameters.AddWithValue("CustId", custId);
                command.Parameters.AddWithValue("PwdHash", pwdHash);

                command.ExecuteNonQuery();
            }
        }

        /*
         * returns password hash;
         * throws KeyNotFoundException if no records with this loginId exist
         */
        public string GetCredential(string loginId, out int custId)
        {
            custId = -1;

            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $@"select CustomerID, PasswordHash from {TableName}
where LoginID = @LoginId";
                command.Parameters.AddWithValue("LoginId", loginId);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    custId = (int)reader["CustomerID"];
                    return (string)reader["PasswordHash"];
                }
                reader.Close();
            }
            throw new KeyNotFoundException(
                "no customer exists with specified login id");
        }

        public int CountLogin()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                SqlCommand command = conn.CreateCommand();
                command.CommandText = $"select count(*) from {TableName}";
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return (int)reader[0];
                }
                reader.Close();
            }
            throw new BankingException("sql count() returns 0 rows");
        }

    }
}