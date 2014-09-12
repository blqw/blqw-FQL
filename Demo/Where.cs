using blqw.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw
{
    /// <summary> 用于生成简单查询的sql语句的对象
    /// </summary>
    public struct Where
    {
        #region 私有
        private ArrayList _parameters;
        private StringBuilder _buffer;
        private int _appendCount;
        private void Append(string separator, string where, params object[] args)
        {
            if (where == null || where.Length == 0 || args == null || args.Length == 0)
            {
                return;
            }

            if (_buffer == null)
            {
                _buffer = new StringBuilder();
            }
            else
            {
                _buffer.Append(' ');
                _buffer.Append(separator);
            }

            _buffer.Append(' ');
            if (args == null || args.Length == 0)
            {
                _buffer.Append(where);
            }
            else
            {
                var r = FQL.Format(FQL.CurrentFQLProvider, where, args, _appendCount++.ToString() + "_");
                _buffer.Append(r.CommandText);
                if (_parameters == null)
                {
                    _parameters = new ArrayList(args.Length);
                }
                _parameters.AddRange(r.DbParameters);
            }
        }

        #endregion


        /// <summary> 增加and条件的where语句
        /// </summary>
        /// <param name="where">可格式化的sql语句</param>
        /// <param name="args">参数</param>
        public void And(string where, params object[] args)
        {
            Append("AND", where, args);
        }

        /// <summary> 增加or条件的where语句
        /// </summary>
        /// <param name="where">可格式化的sql语句</param>
        /// <param name="args">参数</param>
        public void Or(string where, params object[] args)
        {
            Append("OR", where, args);
        }

        /// <summary> 仅返回条件部分的sql语句,可以指定前缀,默认where 也可为空
        /// </summary>
        public string ToSql(string prefix = " WHERE")
        {
            if (_buffer == null || _buffer.Length == 0)
            {
                return "";
            }
            else if (prefix == null || prefix.Length == 0)
            {
                return _buffer.ToString();
            }
            else
            {
                return prefix + _buffer.ToString();
            }
        }

        public DbParameter[] Parameters
        {
            get
            {
                return (DbParameter[])_parameters.ToArray(typeof(DbParameter));
            }
        }
    }
}
