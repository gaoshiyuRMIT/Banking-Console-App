using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SimpleHashing;

namespace Banking.DBManager
{
    delegate bool CheckLogin(Login l, out Exception e);

    public class LoginManager
    {
        internal LoginManagerImpl Impl { get; }
        private CustomerManager CMgr { get; }

        public LoginManager(string connS)
        {
            Impl = new LoginManagerImpl(connS);
            CMgr = new CustomerManager(connS);
        }

        private bool CheckLoginID(Login l, out Exception err)
        {
            err = null;
            if (l.LoginID.Length != 8)
            {
                err = new ArgumentException("login id must be 8 digits long");
                return false;
            }
            return true;
        }

        private bool CheckCustomerID(Login l, out Exception err)
        {
            err = null;
            if (l.CustomerID < 0 || l.CustomerID >= 10000)
            {
                err = new ArgumentOutOfRangeException(null,
                    "customer id must be 4 digits long");
                return false;
            }
            return true;
        }

        public void AddLogin(Login l)
        {
            Exception err;
            CheckLogin[] rules = { CheckLoginID, CheckCustomerID };
            foreach (var check in rules)
                if (!check(l, out err))
                    throw err;

            Customer c = CMgr.GetCustomerByCustomerID(l.CustomerID);
            if (c is null)
                throw new KeyNotFoundException(@"customer with this id as
specified in the login detail does not exist. please add relevant customers first.");

            Impl.AddLogin(l.CustomerID, l.LoginID, l.PasswordHash);
        }

        public bool CheckCredential(string loginId, string pwdInput,
            out Customer cust)
        {
            int custId;
            cust = null;
            string pwdHash = Impl.GetCredential(loginId, out custId);
            if (PBKDF2.Verify(pwdHash, pwdInput))
            {
                cust = CMgr.GetCustomerByCustomerID(custId);
                return true;
            }
            return false;
        }
    }

    internal class LoginManagerImpl : DBManager
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
    }
}
