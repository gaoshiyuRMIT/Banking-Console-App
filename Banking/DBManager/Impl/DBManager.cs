using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager.Impl
{
    internal class DBManager {
        public string TableName { get; }
        public string ConnStr { get; }

        public SqlConnection GetConnection() => new SqlConnection(ConnStr);

        public DBManager(string connS, string tn) {
            ConnStr = connS;
            TableName = tn;
        }
    }

}
