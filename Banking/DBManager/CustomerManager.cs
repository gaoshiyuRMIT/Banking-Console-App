using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

using Banking.DBManager.Impl;

namespace Banking.DBManager
{
    delegate bool CheckCustomer(Customer c, out Exception e);

    public interface ICustomerManager
    {
        public void AddCustomer(Customer c);
        public Customer GetCustomerByCustomerID(int custId);
        public bool AnyCustomer();
    }

    public class CustomerManager : ICustomerManager
    {
        private ICustomerManagerImpl Impl { get; }

        public CustomerManager(ICustomerManagerImpl impl)
        {
            Impl = impl;
        }

        private bool CheckCustomerID(Customer c, out Exception err)
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

        public bool AnyCustomer()
        {
            return Impl.CountCustomer() > 0;
        }
    }



}
