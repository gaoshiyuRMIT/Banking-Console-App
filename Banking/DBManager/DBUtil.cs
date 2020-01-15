using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Banking.DBManager
{
    public static class DBUtil
    {
        public static void AddSqlParam(SqlParameterCollection pc,
            Dictionary<string, object> vars)
        {
            foreach (var pair in vars)
            {
                if (pc.Contains(pair.Key))
                    pc[pair.Key].Value = pair.Value;
                else
                    pc.AddWithValue(pair.Key, pair.Value);
            }
        }

    }
}
