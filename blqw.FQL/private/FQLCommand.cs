using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace blqw
{
    /// <summary> 格式化Sql字符串时的辅助对象
    /// </summary>
    struct FQLCommand
    {
        /// <summary> 空格分隔符
        /// </summary>
        private readonly static char[] Separator = { ' ' };

        public IFQLProvider Provider; //用于格式化sql的机制
        public Dictionary<string, DbParameter> Values; //用于保存格式化中产生的参数
        public ThreadStart Callback; //用于保存out参数返回值的回调
        public StringBuilder SqlBuffer; //用于追加sql语句的字符串缓存
        public object[] Arguments; //用于存放所有的参数


        private string _name; //{0:out xxx} xxx部分
        private bool _isOut;  //{0:out xxx} out部分 也可以是ref
        private bool _isIn;   //{0:ref xxx} ref部分 或者{0:xxx} 没有谓词
        private string _formatString; //{0:out xxx} `out xxx` 部分
        private string _number; //{0:ref xxx} 数字部分
        private int _enumerIndex; //如果参数是可遍历对象(IEnumerable) 用于表示索引

        public void AppendFormat(string number, string format, int offset = 0)
        {

            _enumerIndex = 0;
            _formatString = number + ":" + format;
            //将number转为整型,且不能大于Arguments的个数
            int index;
            if (int.TryParse(number, out index) && index >= 0)
            {
                if (index >= Arguments.Length)
                {
                    throw new FormatException(GetErrorMessage("参数索引{" + number + "}过大"));
                }
            }
            else
            {
                throw new FormatException(GetErrorMessage("参数索引{" + number + "}错误"));
            }
            _number = offset == 0 ? number : (index + offset).ToString();

            //使用number得到参数
            var value = Arguments[index];
            ParseFormat(format);
            #region FormatNull
            //如果参数为null,则不能不是返回参数,也不能有name
            if (value == null || value is DBNull)
            {
                if (_name != null)
                {
                    throw new FormatException(GetErrorMessage("无法从<null>或<DBNull>中获取" + _name + "属性"));
                }
                if (_isOut)
                {
                    throw new FormatException(GetErrorMessage("无法从<null>或<DBNull>中推断返回值的类型"));
                }
                AppendValues((object)null);
                return;
            }
            #endregion

            //得到表示参数类型的TypeInfo对象
            var typeInfo = TypesHelper.GetTypeInfo(value.GetType());
            #region FormatSystemType
            if (typeInfo.IsSpecialType) //处理系统类型 ,不能有Name
            {
                if (_name != null)
                {
                    throw new FormatException(GetErrorMessage(typeInfo.DisplayName + "不支持参数名称"));
                }
                AppendValues(value);
                return;
            }
            #endregion

            //处理各种类型的参数
            switch (typeInfo.TypeCodes)
            {
                case TypeCodes.IList:
                case TypeCodes.IListT:
                    if (_name != null)
                    {
                        throw new FormatException(GetErrorMessage("当参数是 IEnumerable 类型时,不能使用参数名"));
                    }
                    AppendValues(value);
                    return;
                case TypeCodes.IDictionary:
                case TypeCodes.IDictionaryT:
                    var dict = value as IDictionary;
                    if (dict == null)
                    {
                        throw new FormatException(GetErrorMessage("当参数实现 IDictionary<,> 同时也必须实现 IDictionary"));
                    }
                    if (_name == null)
                    {
                        throw new FormatException(GetErrorMessage("当参数是 IDictionary 类型时,必须设定键名称"));
                    }
                    if (dict.Contains(_name) == false)
                    {
                        throw new FormatException(GetErrorMessage("没有找到元素 " + _name));
                    }
                    value = dict[_name];
                    if (_isOut)
                    {
                        if (dict.IsReadOnly)
                        {
                            throw new ReadOnlyException(GetErrorMessage("参数为只读,不能使用关键字out/ref"));
                        }
                        if (value == null)
                        {
                            throw new FormatException(GetErrorMessage("无法从<NULL>中推断返回值的类型"));
                        }
                        var p = value as DbParameter;
                        if (p != null)
                        {
                            AppendValues(p);
                            return;
                        }
                        p = AppendValues(value as Type ?? value.GetType());
                        if (p != null)
                        {
                            var name = _name;
                            Callback += delegate { dict[name] = p.Value; };
                        }
                    }
                    else
                    {
                        AppendValues(value);
                    }
                    return;
                case TypeCodes.DbParameter:
                    var dp = (DbParameter)value;
                    if (dp.ParameterName != null && _name != null)
                    {
                        throw new FormatException("DbParameter名称已存在,命名参数无效");
                    }
                    if (dp.ParameterName == null)
                    {
                        if (_name == null)
                        {
                            throw new FormatException("DbParameter参数必须命名");
                        }
                        dp.ParameterName = _name;
                    }
                    if (format.Length > 0)
                    {
                        dp.Direction = 0;
                        if (_isIn)
                        {
                            dp.Direction |= ParameterDirection.Input;
                        }
                        if (_isOut)
                        {
                            dp.Direction |= ParameterDirection.Output;
                        }
                    }
                    AppendValues(dp);
                    return;
                case TypeCodes.AnonymousType:
                    AppendObjectByName(value, typeInfo);
                    return;
                default:
                    if (value is IEnumerable)
                    {
                        if (_name != null)
                        {
                            throw new FormatException(GetErrorMessage("当参数是 IEnumerable 类型时,不能使用参数名"));
                        }
                        AppendValues(value);
                    }
                    else
                    {
                        AppendObjectByName(value, typeInfo);
                    }
                    return;
            }
        }

        public DbParameter[] GetParameters()
        {
            var length = Arguments.Length;
            for (int i = 0; i < length; i++)
            {
                var arg = Arguments[i] as DbParameter;
                if (arg != null)
                {
                    var key = "$" + arg.ParameterName;
                    DbParameter p;
                    if (Values.TryGetValue(key, out p) == false)
                    {
                        if (arg.Value == null)
                        {
                            arg.Value = DBNull.Value;
                        }
                        Values.Add(key, arg);
                    }
                    else if (object.ReferenceEquals(p, arg) == false)
                    {
                        throw new FormatException("多个参数名称相同");
                    }
                }
            }
            var ps = new DbParameter[Values.Count];
            Values.Values.CopyTo(ps, 0);
            return ps;
        }

        private void AddParameter(string name, DbParameter p)
        {
            if (p == null)
            {
                SqlBuffer.Append("NULL");
                return;
            }
            SqlBuffer.Append(Provider.ParameterPrefix);
            SqlBuffer.Append(p.ParameterName);
            if (p.Value == null)
            {
                p.Value = DBNull.Value;
            }
            Values.Add(name, p);
        }
        private DbParameter AppendValues(DbParameter value)
        {
            if (value.ParameterName == null)
            {
                value.ParameterName = GetParameterName();
            }
            var key = "$" + value.ParameterName;
            DbParameter p;
            if (Values.TryGetValue(key, out p) == false)
            {
                if (value.Value == null)
                {
                    value.Value = DBNull.Value;
                }
                Values.Add(key, value);
            }
            else if (object.ReferenceEquals(p, value) == false)
            {
                throw new FormatException("多个参数名称相同");
            }
            SqlBuffer.Append(Provider.ParameterPrefix);
            SqlBuffer.Append(value.ParameterName);
            return null;
        }
        /// <summary> 追加一个值到参数
        /// </summary>
        private DbParameter AppendValues(object value)
        {
            var dp = value as DbParameter;
            #region DbParameter
            if (dp != null)
            {
                AppendValues(dp);
                return null;
            }
            #endregion

            string pname = GetParameterName();

            if (Values.ContainsKey(pname))
            {
                SqlBuffer.Append(Provider.ParameterPrefix);
                SqlBuffer.Append(Values[pname].ParameterName);
                return null;
            }

            var t = value as Type;
            #region Type
            if (t != null)
            {
                if (!_isOut)
                {
                    throw new FormatException(GetErrorMessage("当参数是 Type 类型时,必须使用关键字out或ref"));
                }
                if (t == typeof(object))
                {
                    throw new FormatException(GetErrorMessage("无法从 typeof(object) 推断返回值的类型"));
                }
                var p = Provider.CreateDbParameter(Convert2.TypeToDbType(t), t);
                p.ParameterName = pname;
                AddParameter(pname, p);
                p.Direction = (_isIn) ? ParameterDirection.InputOutput : ParameterDirection.Output;
                return p;
            }
            #endregion

            var em = value as IEnumerable;
            #region IEnumerable
            if (em != null && value is string == false) //string实现接口IEnumerable
            {
                if (_isOut)
                {
                    throw new FormatException(GetErrorMessage("当参数是 IEnumerable 类型时,不能使用关键字out/ref"));
                }
                var ee = em.GetEnumerator();
                if (ee.MoveNext())
                {
                    _enumerIndex++;
                    AppendValues(ee.Current);
                    while (ee.MoveNext())
                    {
                        _enumerIndex++;
                        SqlBuffer.Append(',');
                        AppendValues(ee.Current);
                    }
                }
                else
                {
                    throw new FormatException(GetErrorMessage("当参数是 IEnumerable 类型时,元素个数不能为0"));
                }
                return null;
            }
            #endregion

            {
                var p = Provider.CreateDbParameter(value);
                if (p == null)
                {
                    SqlBuffer.Append("NULL");
                    return null;
                }
                p.ParameterName = pname;
                AddParameter(pname, p);
                if (_isIn)
                {
                    p.Direction = _isOut ? ParameterDirection.InputOutput : ParameterDirection.Input;
                }
                else
                {
                    p.Direction = ParameterDirection.Output;
                }
                return p;
            }
        }

        private string GetParameterName()
        {
            string pname;
            if (_name == null)
            {
                if (_enumerIndex == 0)
                    pname = string.Concat("p", _number);
                else
                    pname = string.Concat("p", _number, "_", _enumerIndex.ToString());
            }
            else
            {
                if (_enumerIndex == 0)
                    pname = string.Concat("p", _number, "_", _name);
                else
                    pname = string.Concat("p", _number, "_", _name, "_", _enumerIndex.ToString());
            }
            return pname;
        }
        /// <summary> 根据format中的name,从参数target中取出值,并追加到参数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="typeInfo">参数target的类型信息</param>
        private void AppendObjectByName(object target, TypeInfo typeInfo)
        {
            if (_name == null)
            {
                throw new FormatException(GetErrorMessage("必须设定参数名称"));
            }
            var lit = typeInfo.IgnoreCaseLiteracy;
            var prop = lit.Property[_name];
            if (prop == null)
            {
                lit.Load.PublicField();
                prop = lit.Field[_name];
                if (prop == null)
                {
                    throw new FormatException(GetErrorMessage("没有找到名为 " + _name + " 的公开属性或字段"));
                }
            }
            if (_isIn && prop.CanRead == false)
            {
                throw new FormatException(GetErrorMessage("属性 " + _name + " 不可读"));
            }

            //非out参数,必然是in参数
            if (_isOut == false)
            {
                var value = prop.GetValue(target);
                AppendValues(value);
                return;
            }

            if (target is ValueType)
            {
                throw new FormatException(GetErrorMessage("结构体 ,不能使用关键字out/ref"));
            }
            //非in参数,传入DbType
            DbParameter p;
            if (!_isIn && prop.TypeCodes != TypeCodes.DbParameter)
            {
                p = AppendValues(prop.MemberType);
            }
            else
            {
                p = AppendValues(prop.GetValue(target));
            }

            if (p != null && prop.CanWrite == false)
            {
                if (typeInfo.TypeCodes == TypeCodes.AnonymousType)
                {
                    lit.Load.NonPublicField();
                    var f = lit.Field["<" + _name + ">i__Field"];
                    if (f != null)
                    {
                        Callback += delegate { f.SetValue(target, p.Value); };
                        return;
                    }
                }
                throw new ReadOnlyException(GetErrorMessage("属性 " + _name + " 为只读,不能使用关键字out/ref"));
            }

            Callback += delegate { prop.SetValue(target, p.Value); };
        }
        /// <summary> 获取用于抛出的异常信息字符串
        /// </summary>
        /// <param name="message">错误说明</param>
        private string GetErrorMessage(string message)
        {
            return string.Concat("参数 {", _formatString, "} 中出现错误:", Environment.NewLine, message);
        }
        /// <summary> 解析format字符串,结果保存在 字段中
        /// </summary>
        /// <param name="format">格式字符串</param>
        private void ParseFormat(string format)
        {
            //初始化
            _isIn = true;
            _isOut = false;
            _name = null;
            if (format == null || format.Length == 0)
            {
                return;
            }
            var arr = format.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 2)
            {
                throw new FormatException(GetErrorMessage("参数格式为{index[:[ref|out] [name]]}"));
            }

            //split方法返回的必然是一个长度大于等于1的数组
            var verb = arr[0];  // 假设第一段是out或ref
            //当arr[0].Length == 0 的时候 只有可能是字符串全部是由空格组成
            if (verb.Length == 0)
            {
                return;
            }
            if (verb.Length == 3)
            {
                switch (verb[0])
                {
                    case 'r':
                    case 'R':
                        if ((verb[1] == 'e' || verb[1] == 'E') && (verb[2] == 'f' || verb[2] == 'F'))
                        {
                            _isIn = true;
                            _isOut = true;
                        }
                        break;
                    case 'o':
                    case 'O':
                        if ((verb[1] == 'u' || verb[1] == 'U') && (verb[2] == 't' || verb[2] == 'T'))
                        {
                            _isIn = false;
                            _isOut = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (_isOut) //无论是out还是ref _isOut必然是true
            {
                if (arr.Length == 1) //如果参数只有一段,无法获取name
                {
                    _name = null;
                    return;
                }
                _name = arr[1];
            }
            else if (arr.Length == 2) //第一段不是ref和out,但参数却有2段,抛异常
            {
                throw new FormatException(GetErrorMessage("关键词只能是 out/ref 不区分大小写"));
            }
            else
            {
                _name = verb;
            }

            var c = _name[0]; //这里name必然是有值 且长度不为0的
            if (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return;
            }
            throw new FormatException(GetErrorMessage("参数名首字母只能是下划线或字母[_a-zA-Z]"));
        }

    }
}
