using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;
using System.Data.SqlClient;
using blqw;

namespace UnitTestProject1
{
    static class CustomAssert
    {
        public static void Assert(this IFQLResult result, string sql)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(sql, result.CommandText);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(result.DbParameters);

            foreach (var p in result.DbParameters)
            {
                sql = sql.Replace("@" + p.ParameterName, "");
            }

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(sql.Contains("@"));
        }

    }

    #region MyRegion
    class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ArrayList IDs { get; set; }
    }

    struct User2
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public Array IDs { get; set; }
    }
    #endregion

    [TestClass]
    public class 单元测试
    {

        [TestMethod]
        public void 各种类型_输入参数()
        {
            int i = int.MaxValue;                               //系统类型
            long? l = null;                                     //可空值类型
            int[] arr = { 1, 2, 3, 4, 5 };                      //数组
            var list = new List<int> { 6, 7, 8, 9, 10 };        //集合
            string s = "blqw1";                                 //字符串
            var obj = new { id = 100, name = "blqw", IDs = new[] { 11, 12, 13, 14, 15 } };            //匿名类型
            var dict = new Dictionary<string, object>();        //键值对
            dict.Add("id", 101);
            dict.Add("name", "blqw2");
            dict.Add("ids", new[] { 16, 17, 18, 19, 20 });
            var user1 = new User { ID = 102, Name = "blqw3", IDs = new ArrayList { 21, 22, 23, 24, 25 } };     //自定义 引用类型
            var user2 = new User2 { ID = 103, Name = "blqw4", IDs = new[] { 31, 32, 33, 34, 35 } };    //自定义 值类型
            var p = new System.Data.SqlClient.SqlParameter("name", System.Data.SqlDbType.NVarChar);


            IFQLResult result;

            result = FQL.Format("select * from users where  id = {0} and name = {1} and 1=1", i, s);
            result.Assert("select * from users where  id = @p0 and name = @p1 and 1=1");
            Assert.AreEqual(i, result.DbParameters[0].Value);
            Assert.AreEqual(s, result.DbParameters[1].Value);

            result = FQL.Format("select * from users where  id = {0} and name = {1}", i, p);
            result.Assert("select * from users where  id = @p0 and name = @name");
            Assert.AreEqual(i, result.DbParameters[0].Value);
            Assert.AreEqual(p, result.DbParameters[1]);

            result = FQL.Format(FQL.CurrentFQLProvider, "select * from users where  id = {0} and name = {1}", l, s);
            result.Assert("select * from users where  id = NULL and name = @p1");
            Assert.AreEqual(s, result.DbParameters[0].Value);
            result = FQL.Format("select * from users where  id in ({0}) and name = {1}", arr, s);
            result.Assert("select * from users where  id in (@p0_1,@p0_2,@p0_3,@p0_4,@p0_5) and name = @p1");
            Assert.AreEqual(arr[0], result.DbParameters[0].Value);
            Assert.AreEqual(arr[1], result.DbParameters[1].Value);
            Assert.AreEqual(arr[2], result.DbParameters[2].Value);
            Assert.AreEqual(arr[3], result.DbParameters[3].Value);
            Assert.AreEqual(arr[4], result.DbParameters[4].Value);
            Assert.AreEqual(s, result.DbParameters[5].Value);
            result = FQL.Format("select * from users where  id in ({0},{1:id}) and name = {1:name}", list, obj);
            result.Assert("select * from users where  id in (@p0_1,@p0_2,@p0_3,@p0_4,@p0_5,@p1_id) and name = @p1_name");
            Assert.AreEqual(list[0], result.DbParameters[0].Value);
            Assert.AreEqual(list[1], result.DbParameters[1].Value);
            Assert.AreEqual(list[2], result.DbParameters[2].Value);
            Assert.AreEqual(list[3], result.DbParameters[3].Value);
            Assert.AreEqual(list[4], result.DbParameters[4].Value);
            Assert.AreEqual(obj.id, result.DbParameters[5].Value);
            Assert.AreEqual(obj.name, result.DbParameters[6].Value);

            result = FQL.Format("select * from users where  id = {0:id} and name = {0:name}", obj);
            result.Assert("select * from users where  id = @p0_id and name = @p0_name");
            Assert.AreEqual(obj.id, result.DbParameters[0].Value);
            Assert.AreEqual(obj.name, result.DbParameters[1].Value);
            result = FQL.Format("select * from users where  id = {0:id} and name = {0:name}", dict);
            result.Assert("select * from users where  id = @p0_id and name = @p0_name");
            Assert.AreEqual(dict["id"], result.DbParameters[0].Value);
            Assert.AreEqual(dict["name"], result.DbParameters[1].Value);
            result = FQL.Format("select * from users where  id = {0:id} and name = {0:name}", user1);
            result.Assert("select * from users where  id = @p0_id and name = @p0_name");
            Assert.AreEqual(user1.ID, result.DbParameters[0].Value);
            Assert.AreEqual(user1.Name, result.DbParameters[1].Value);
            result = FQL.Format("select * from users where  id = {0:id} and name = {0:name}", user2);
            result.Assert("select * from users where  id = @p0_id and name = @p0_name");
            Assert.AreEqual(user2.ID, result.DbParameters[0].Value);
            Assert.AreEqual(user2.Name, result.DbParameters[1].Value);


            result = FQL.Format("select * from users where id in ({0:ids},{0:id},{1}) and name = {0:name}", obj, 1);
            result.Assert("select * from users where id in (@p0_ids_1,@p0_ids_2,@p0_ids_3,@p0_ids_4,@p0_ids_5,@p0_id,@p1) and name = @p0_name");
            Assert.AreEqual(obj.IDs[0], result.DbParameters[0].Value);
            Assert.AreEqual(obj.IDs[1], result.DbParameters[1].Value);
            Assert.AreEqual(obj.IDs[2], result.DbParameters[2].Value);
            Assert.AreEqual(obj.IDs[3], result.DbParameters[3].Value);
            Assert.AreEqual(obj.IDs[4], result.DbParameters[4].Value);
            Assert.AreEqual(obj.id, result.DbParameters[5].Value);
            Assert.AreEqual(1, result.DbParameters[6].Value);
            Assert.AreEqual(obj.name, result.DbParameters[7].Value);


        }

        [TestMethod]
        public void 匿名类型_返回参数()
        {
            var obj = new { id = 1 };
            var result = FQL.Format("select * from users where id = {0:out id}", obj);
            result.Assert("select * from users where id = @p0_id");
            Assert.IsNull(result.DbParameters[0].Value);
            result.DbParameters[0].Value = 10;
            result.ImportOutParameter();
            Assert.AreEqual(10, obj.id);
            result = FQL.Format("select * from users where id = {0:ref id}", obj);
            result.Assert("select * from users where id = @p0_id");
            Assert.AreEqual(10, result.DbParameters[0].Value);
            result.DbParameters[0].Value = 100;
            result.ImportOutParameter();
            Assert.AreEqual(100, obj.id);
        }

        [TestMethod]
        public void 键值对_返回参数()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("id", 1);
            dict.Add("name", "");
            var result = FQL.Format("select {0:out name} = name from users where id = {0:id}", dict);
            result.Assert("select @p0_name = name from users where id = @p0_id");
            result.DbParameters[0].Value = "blqw";
            result.ImportOutParameter();
            Assert.AreEqual("blqw", dict["name"]);

            dict["name"] = typeof(string);
            result = FQL.Format("select {0:out name} = name from users where id = {0:id}", dict);
            result.Assert("select @p0_name = name from users where id = @p0_id");
            result.DbParameters[0].Value = "blqw";
            result.ImportOutParameter();
            Assert.AreEqual("blqw", dict["name"]);

            dict["name"] = new SqlParameter {
                ParameterName = "name",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Direction = System.Data.ParameterDirection.Output,
            };
            result = FQL.Format("select {0:name} = name from users where id = {0:id}", dict);
            result.Assert("select @name = name from users where id = @p0_id");
            result.DbParameters[0].Value = "blqw";
            Assert.AreEqual("blqw", ((SqlParameter)dict["name"]).Value);

            dict["name"] = new SqlParameter {
                ParameterName = "name",
                SqlDbType = System.Data.SqlDbType.NVarChar
            };
            result = FQL.Format("select {0:out name} = name from users where id = {0:id}", dict);
            result.Assert("select @name = name from users where id = @p0_id");
            result.DbParameters[0].Value = "blqw";
            Assert.AreEqual("blqw", ((SqlParameter)dict["name"]).Value);
        }

        [TestMethod]
        public void 多余的参数()
        {
            var p = new SqlParameter {
                ParameterName = "name",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Direction = System.Data.ParameterDirection.Output,
            };
            var result = FQL.Format("select name from users where id = {0}", 1, p);
            result.Assert("select name from users where id = @p0");
            Assert.AreEqual(1, result.DbParameters[0].Value);
            Assert.AreEqual(2, result.DbParameters.Length);

            //换位置
            result = FQL.Format("select name from users where id = {1}", p, 1);
            result.Assert("select name from users where id = @p1");
            Assert.AreEqual(1, result.DbParameters[0].Value);
            Assert.AreEqual(2, result.DbParameters.Length);


            result = FQL.Format("select name from users where id = {0} or id = {1}", p, 1);
            result.Assert("select name from users where id = @name or id = @p1");
            Assert.AreEqual(p, result.DbParameters[0]);
        }

        [TestMethod]
        public void 测试异常()
        {
            try
            {
                FQL.Format(null, 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("{1}", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("{x}", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("..{.", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("..{0:{}.", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("..{0::}.", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("...{x:...", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("...{0:name}:...", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            try
            {
                FQL.Format("...{0:name}:...", 1);
                Assert.Fail("测试失败");
            }
            catch (AssertFailedException) { throw; }
            catch { }

            FQL.Format("..{{.", 1);
            FQL.Format("..}}.", 1);
            FQL.Format("..{0:}.", 1);
            FQL.Format("..}.", 1);
            FQL.Format("..", 1);
            FQL.Format("....", null);
            FQL.Format(null, "{0}", 1);
        }

        [TestMethod]
        public void 一个参数使用多次()
        {
            IFQLResult result;

            result = FQL.Format("select {0} + {1} from users where  id = {0} and name = {1}", 1, "blqw");
            result.Assert("select @p0 + @p1 from users where  id = @p0 and name = @p1");
            Assert.AreEqual(1, result.DbParameters[0].Value);
            Assert.AreEqual("blqw", result.DbParameters[1].Value);


            result = FQL.Format(@"select * from users where id in ({0});
select * from roles where user_id in ({0});", new[] { 1, 2, 3, 4, 5 });
            result.Assert(@"select * from users where id in (@p0_1,@p0_2,@p0_3,@p0_4,@p0_5);
select * from roles where user_id in (@p0_1,@p0_2,@p0_3,@p0_4,@p0_5);");
            Assert.AreEqual(1, result.DbParameters[0].Value);
            Assert.AreEqual(2, result.DbParameters[1].Value);
            Assert.AreEqual(3, result.DbParameters[2].Value);
            Assert.AreEqual(4, result.DbParameters[3].Value);
            Assert.AreEqual(5, result.DbParameters[4].Value);
        }


        [TestMethod]
        public void 一般实体_返回参数()
        {
            var obj = new User { ID = 1 };
            var result = FQL.Format("select * from users where id = {0:out id}", obj);
            result.Assert("select * from users where id = @p0_id");
            Assert.IsNull(result.DbParameters[0].Value);
            result.DbParameters[0].Value = 10;
            result.ImportOutParameter();
            Assert.AreEqual(10, obj.ID);
            result = FQL.Format("select * from users where id = {0:ref id}", obj);
            result.Assert("select * from users where id = @p0_id");
            Assert.AreEqual(10, result.DbParameters[0].Value);
            result.DbParameters[0].Value = 100;
            result.ImportOutParameter();
            Assert.AreEqual(100, obj.ID);
        }

        [TestMethod]
        public void 一般结构_返回参数()
        {
            var obj = new User2 { ID = 1 };
            try
            {
                var result = FQL.Format("select * from users where id = {0:out id}", obj);
                Assert.Fail("测试失败");
            }
            catch (Exception)
            {

            }
        }
    }
}
