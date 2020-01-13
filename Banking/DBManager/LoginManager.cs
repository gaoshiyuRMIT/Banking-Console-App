using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SimpleHashing;

using Banking.DBManager.Impl;

namespace Banking.DBManager
{
    delegate bool CheckLogin(Login l, out Exception e);

    public interface ILoginManager
    {
        public void AddLogin(Login l);
        public bool Login(string loginId, string pwdInput,
            out Customer cust);
        public bool AnyLogin();
    }

    public class LoginManager : ILoginManager
    {
        private ILoginManagerImpl Impl { get; }
        private CustomerManager CMgr { get; }

        public LoginManager(ILoginManagerImpl impl, ICustomerManagerImpl cmi)
        {
            Impl = impl;
            CMgr = new CustomerManager(cmi);
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

        public bool Login(string loginId, string pwdInput,
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

        public bool AnyLogin()
        {
            return Impl.CountLogin() > 0;
        }
    }

}
