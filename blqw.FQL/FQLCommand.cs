using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace blqw.Data
{
    /// <summary> 格式化Sql字符串时的辅助对象
    /// </summary>
    struct FQLCommand
    {
        public FQLCommand(string flag)
            : this()
        {
            _concats = new string[5];
            _concats[0] = "p";
            if (flag == null)
            {
                _concats[2] = "_";
            }
            else
            {
                _concats[2] = "_" + flag + "_";
            }
        }
        /// <summary> 空格分隔符
        /// </summary>
        private readonly static char[] Separator = { ' ' };

        public IFQLProvider Provider;
        public Dictionary<string, DbParameter> Values;
        public ThreadStart Callback;
        public StringBuilder SqlBuffer;
        public object[] Arguments;

        private string _name;
        private bool _isOut;
        private bool _isIn;
        private string _formatString;
        private int _suffix;
        private string[] _concats;

        private DbParameter AppendValues(object value)
        {
            if (value == null && _isOut == false)
            {
                SqlBuffer.Append("NULL");
                return null;
            }
            string pname;
            if (_suffix > 0)
            {
                _concats[3] = _name;
                _concats[4] = _suffix.ToString();
                pname = string.Concat(_concats);
            }
            else if (_name == null)
            {
                pname = _concats[0] + _concats[1];
            }
            else
            {
                pname = string.Concat(_concats[0], _concats[1], _concats[2], _name);
            }

            if (Values.ContainsKey(pname))
            {
                SqlBuffer.Append(Provider.ParameterPrefix);
                SqlBuffer.Append(Values[pname].ParameterName);
                return null;
            }

            var op = value as DbParameter;
            #region DbParameter
            if (op != null)
            {
                var p = Provider.CreateDbParameter(op.DbType);
                p.Size = op.Size;
                p.Value = op.Value;
                p.DbType = op.DbType;
                p.Direction = op.Direction;
                p.ParameterName = _name ?? op.ParameterName;
                if (_isOut)
                {
                    p.Direction = _isIn ? ParameterDirection.InputOutput : ParameterDirection.Output;
                    Callback += delegate { op.Value = p.Value; };
                }
                else if ((op.Direction & ParameterDirection.Output) != 0)
                {
                    p.Direction = op.Direction;
                    Callback += delegate { op.Value = p.Value; };
                }
                SqlBuffer.Append(Provider.ParameterPrefix);
                SqlBuffer.Append(p.ParameterName);
                Values.Add(pname, p);
                return null;
            }
            #endregion

            var t = value as Type;
            #region Type
            if (t != null)
            {
                if (!_isOut)
                {
                    throw new FormatException(ErrMsg("当参数是 Type 类型时,必须使用关键字out或ref"));
                }
                if (t == typeof(object))
                {
                    throw new FormatException(ErrMsg("无法从 typeof(object) 推断返回值的类型"));
                }
                var p = Provider.CreateDbParameter(Convert2.TypeToDbType(t));
                p.ParameterName = pname;
                SqlBuffer.Append(Provider.ParameterPrefix);
                SqlBuffer.Append(p.ParameterName);
                Values.Add(pname, p);
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
                    throw new FormatException(ErrMsg("当参数是 IEnumerable 类型时,不能使用关键字out/ref"));
                }
                var ee = em.GetEnumerator();
                if (ee.MoveNext())
                {
                    _suffix++;
                    AppendValues(ee.Current);
                    while (ee.MoveNext())
                    {
                        _suffix++;
                        SqlBuffer.Append(',');
                        AppendValues(ee.Current);
                    }
                }
                else
                {
                    throw new FormatException(ErrMsg("当参数是 IEnumerable 类型时,元素个数不能为0"));
                }
                return null;
            }
            #endregion

            {
                var p = Provider.CreateDbParameter(value);
                p.ParameterName = pname;
                SqlBuffer.Append(Provider.ParameterPrefix);
                SqlBuffer.Append(p.ParameterName);
                Values.Add(pname, p);
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
        public void AppendFormat(string number, string format)
        {
            _concats[1] = number;
            _concats[4] = null;

            _suffix = 0;
            _formatString = number + ":" + format;
            //将number转为整型,且不能大于Arguments的个数
            int index;
            if (int.TryParse(number, out index) && index >= 0)
            {
                if (index >= Arguments.Length)
                {
                    throw new FormatException(ErrMsg("参数索引{" + number + "}过大"));
                }
            }
            else
            {
                throw new FormatException(ErrMsg("参数索引{" + number + "}错误"));
            }

            //使用number得到参数
            var value = Arguments[index];
            ParseFormat(format);
            #region FormatNull
            //如果参数为null,则不能不是返回参数,也不能有name
            if (value == null || value is DBNull)
            {
                if (_name != null)
                {
                    throw new FormatException(ErrMsg("无法从<NULL>中获取" + _name + "属性"));
                }
                if (_isOut)
                {
                    throw new FormatException(ErrMsg("无法从<NULL>中推断返回值的类型"));
                }
                SqlBuffer.Append("NULL");
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
                    throw new FormatException(ErrMsg(typeInfo.DisplayName + "不支持参数名称"));
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
                        throw new FormatException(ErrMsg("当参数是 IEnumerable 类型时,不能使用参数名"));
                    }
                    AppendValues(value);
                    return;
                case TypeCodes.IDictionary:
                case TypeCodes.IDictionaryT:
                    var dict = value as IDictionary;
                    if (dict == null)
                    {
                        throw new FormatException(ErrMsg("当参数实现 IDictionary<,> 同时也必须实现 IDictionary"));
                    }
                    if (_name == null)
                    {
                        throw new FormatException(ErrMsg("当参数是 IDictionary 类型时,必须设定键名称"));
                    }
                    if (dict.Contains(_name) == false)
                    {
                        throw new FormatException(ErrMsg("没有找到元素 " + _name));
                    }
                    value = dict[_name];
                    if (_isOut)
                    {
                        if (dict.IsReadOnly)
                        {
                            throw new ReadOnlyException(ErrMsg("参数为只读,不能使用关键字out/ref"));
                        }
                        if (value == null)
                        {
                            throw new FormatException(ErrMsg("无法从<NULL>中推断返回值的类型"));
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
                    AppendValues(value);
                    return;
                case TypeCodes.AnonymousType:
                    FormatObject(value, typeInfo);
                    return;
                default:
                    if (value is IEnumerable)
                    {
                        if (_name != null)
                        {
                            throw new FormatException(ErrMsg("当参数是 IEnumerable 类型时,不能使用参数名"));
                        }
                        AppendValues(value);
                    }
                    else
                    {
                        FormatObject(value, typeInfo);
                    }
                    return;
            }
        }


        private void FormatObject(object target, TypeInfo typeInfo)
        {
            if (_name == null)
            {
                throw new FormatException(ErrMsg("必须设定参数名称"));
            }
            var lit = typeInfo.IgnoreCaseLiteracy;
            var prop = lit.Property[_name];
            if (prop == null)
            {
                throw new FormatException(ErrMsg("没有找到名为 " + _name + " 的公开属性"));
            }
            if (_isIn && prop.CanRead == false)
            {
                throw new FormatException(ErrMsg("属性 " + _name + " 不可读"));
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
                throw new FormatException(ErrMsg("结构体 ,不能使用关键字out/ref"));
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
                throw new ReadOnlyException(ErrMsg("属性 " + _name + " 为只读,不能使用关键字out/ref"));
            }

            Callback += delegate { prop.SetValue(target, p.Value); };
        }
        private string ErrMsg(string message)
        {
            return string.Concat("参数 {", _formatString, "} 中出现错误:", Environment.NewLine, message);
        }

        /// <summary> 解析format字符串
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
                throw new FormatException(ErrMsg("参数格式为{index[:[ref|out] [name]]}"));
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
                throw new FormatException(ErrMsg("关键词只能是 out/ref 不区分大小写"));
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
            throw new FormatException(ErrMsg("参数名首字母只能是下划线或字母[_a-zA-Z]"));
        }

        public DbParameter[] GetParameters()
        {
            var ps = new DbParameter[Values.Count];
            Values.Values.CopyTo(ps, 0);
            return ps;
        }
    }
}
