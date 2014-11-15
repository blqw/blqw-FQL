using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace blqw
{
    public class SqlServerFQL : IFQLProvider
    {
        public readonly static SqlServerFQL Instance = new SqlServerFQL();

        public string ParameterPrefix
        {
            get { return "@"; }
        }

        public System.Data.Common.DbParameter CreateDbParameter(object value)
        {
            if (value == null)
            {
                return null;
            }
            return new System.Data.SqlClient.SqlParameter() { Value = value, Size = -1 };
        }

        public System.Data.Common.DbParameter CreateDbParameter(DbType dbType, Type type)
        {
            return new System.Data.SqlClient.SqlParameter() { DbType = dbType, Size = -1 };
        }
    }
}
