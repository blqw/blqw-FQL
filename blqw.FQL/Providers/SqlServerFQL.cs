using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace blqw.Data
{
    class SqlServerFQL : IFQLProvider
    {
        internal SqlServerFQL()
        {

        }

        public string ParameterPrefix
        {
            get { return "@"; }
        }

        public System.Data.Common.DbParameter CreateDbParameter(object value)
        {
            return new System.Data.SqlClient.SqlParameter() { Value = value };
        }

        public System.Data.Common.DbParameter CreateDbParameter(DbType dbType)
        {
            return new System.Data.SqlClient.SqlParameter() { DbType = dbType,Size = 10 };
        }
    }
}
