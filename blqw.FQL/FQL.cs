using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace blqw
{
    /// <summary> 用于格式化sql字符串的静态类
    /// </summary>
    public static class FQL
    {
        
        public static IFQLBuilder CreateBuilder(IFQLProvider provider)
        {
            return new FQLBuilder(null, provider, null, null, null, 0);
        }

        public static IFQLBuilder CreateBuilder(IFQLProvider provider, string firstConnector)
        {
            return new FQLBuilder(firstConnector, provider, null, null, null, 0);
        }


        /// <summary> 使用 指定的IFQLProvider 作为格式化机制,格式sql语句
        /// </summary>
        /// <param name="provider">用于格式化sql语句的格式化机制</param>
        /// <param name="sql">待格式化的sql语句</param>
        /// <param name="args">包含零个或多个Sql参数</param>
        public static IFQLResult Format(IFQLProvider provider, string sql, params object[] args)
        {
            return Format(provider, 0, sql, args);
        }

        /// <summary> 使用 指定的IFQLProvider 作为格式化机制,格式sql语句
        /// </summary>
        /// <param name="provider">用于格式化sql语句的格式化机制</param>
        /// <param name="sql">待格式化的sql语句</param>
        /// <param name="args">包含零个或多个Sql参数</param>
        /// <param name="propPrefix">参数名称前缀</param>
        public static IFQLResult Format(IFQLProvider provider, int startNumber, string sql, object[] args)
        {
            if (sql == null || sql.Length == 0)
            {
                throw new ArgumentNullException("sql");
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            var sqlLength = sql.Length;
            if (sqlLength < 3 || args == null || args.Length == 0)
            {
                return new FQLResult(provider, sql, null, null, 0);
            }
            var argsCount = args.Length;
            var command = new FQLCommand() {
                Values = new Dictionary<string, DbParameter>(argsCount),
                Provider = provider,
                SqlBuffer = new StringBuilder(sqlLength + argsCount * 3),
                Arguments = args,
            };
            var buffer = command.SqlBuffer;
            unsafe
            {
                fixed (char* p = sql)
                {
                    var curr = 0;
                    var state = 0;              //状态机 0:一般,  1:出现了{, 2:出现了:
                    string number = null;
                    for (int i = 0; i < sqlLength; i++)
                    {
                        var c = p[i];
                        switch (c)
                        {
                            case '{':
                                if (state != 0)
                                {
                                    if (state == 1 && curr == i) //如果是两个 {{
                                    {
                                        state = 0;
                                        break;
                                    }
                                    throw new FormatException("意外的'{'符号");
                                }
                                buffer.Append(sql, curr, i - curr);
                                state = 1;
                                curr = i + 1;
                                break;
                            case ':':
                                if (state == 1)
                                {
                                    number = new string(p, curr, i - curr);
                                    curr = i + 1;
                                    state = 2;
                                }
                                else if (state == 2)
                                {
                                    throw new FormatException("意外的':'符号");
                                }
                                break;
                            case '}':
                                if (state == 0)
                                {
                                    if (p[i + 1] == '}')
                                    {
                                        i++;
                                        buffer.Append(sql, curr, i - curr);
                                        curr = i + 1;
                                    }
                                    break;
                                }
                                string format;
                                if (state == 1)
                                {
                                    number = new string(p, curr, i - curr);
                                    format = string.Empty;
                                }
                                else if (curr == i)
                                {
                                    format = string.Empty;
                                }
                                else
                                {
                                    format = new string(p, curr, i - curr);
                                }
                                command.AppendFormat(number, format, startNumber);
                                state = 0;
                                curr = i + 1;
                                break;
                            default:
                                break;
                        }
                    }
                    switch (state)
                    {
                        case 1:
                            throw new FormatException("意外的'{'符号");
                        case 2:
                            throw new FormatException("没有'}'符号");
                        default:
                            break;
                    }
                    var count = sqlLength - curr;
                    if (count > 0)
                    {
                        buffer.Append(sql, curr, sqlLength - curr);
                    }
                }
            }
            return new FQLResult(provider, buffer.ToString(), command.GetParameters(), command.Callback, args.Length);
        }
    }
}
