﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace blqw.Data
{
    public class SqlServerFQL : IFQLProvider
    {
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
            return new System.Data.SqlClient.SqlParameter() { Value = value };
        }

        public System.Data.Common.DbParameter CreateDbParameter(DbType dbType, Type type)
        {
            return new System.Data.SqlClient.SqlParameter() { DbType = dbType };
        }
    }
}
