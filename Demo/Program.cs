﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using blqw;
using System.Data.SqlClient;
using System.Data;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeTimer.Initialize();

            //CodeTimer.Time("a", 4, () => {
            //    using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            //    using (var cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "select count(1) from sys.objects";
            //        conn.Open();
            //    }
            //});

            //using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            //{
            //    conn.Open();
            //    CodeTimer.Time("b", 4, () => {
            //        using (var cmd = conn.CreateCommand())
            //        {
            //            cmd.CommandText = "select count(1) from sys.objects";
            //        }
            //    });
            //}
            //return;
            //// FQL.CurrentFQLProvider = FQL.SqlServer; //设定默认FQL格式化机制,因为系统默认就是SqlServer,所以可以省略

            //var keyword = "sys";
            ////var r = FQL.Format(FQL.SqlServer, sql, "sys"); //也可以在方法中设定格式化机制
            //var r = FQL.Format(SqlServerFQL.Instance,"select count(1) from sys.objects where name like '%' + {0} + '%'", keyword);

            //using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            //using (var cmd = conn.CreateCommand())
            //{
            //    cmd.CommandText = r.CommandText;        //设置CommandText
            //    cmd.Parameters.AddRange(r.DbParameters);//设定Parameters
            //    conn.Open();
            //    Console.WriteLine(cmd.ExecuteScalar());
            //}

            //OutDemo();
            //SearchCountDemo2("a", "S", null);
        }

        static void OutDemo()
        {
            string sql = "select {0:out count} = count(1) from sys.objects where name like '%' + {0:name} + '%'";
            var arg = new { name = "sys", count = 0 };
            var r = FQL.Format(SqlServerFQL.Instance, sql, arg);

            using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = r.CommandText;
                cmd.Parameters.AddRange(r.DbParameters);
                conn.Open();
                cmd.ExecuteNonQuery();
                r.ImportOutParameter();
                Console.WriteLine(arg.count);
            }
        }

        static void SearchCountDemo(string name, string type, string type_desc)
        {
            var sql = FQL.Format(SqlServerFQL.Instance, "select @totle = count(1) from sys.objects").AsBuilder();
            if (name != null) sql.And("name like '%' + {0} + '%'", name);
            if (type != null) sql.And("type = {0}", type); ;
            if (type_desc != null) sql.And("type_desc = {0}", type_desc);

            using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql.CommandText;                  //设置CommandText
                cmd.Parameters.AddRange(sql.DbParameters);          //设定Parameters
                var p = new SqlParameter("totle", 0) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(p);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine(p.Value);
            }
        }

        static void SearchCountDemo2(string name, string type, string type_desc)
        {
            var p = new { totle = 0 };
            var sql = FQL.Format(SqlServerFQL.Instance, "select {0:out totle} = count(1) from sys.objects", p).AsBuilder();
            if (name != null) sql.And("name like '%' + {0} + '%'", name);
            if (type != null) sql.And("type = {0}", type); ;
            if (type_desc != null) sql.And("type_desc = {0}", type_desc);

            using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql.CommandText;                  //设置CommandText
                cmd.Parameters.AddRange(sql.DbParameters);          //设定Parameters
                conn.Open();
                cmd.ExecuteNonQuery();
                sql.ImportOutParameter();
                Console.WriteLine(p.totle);
            }
        }

        static int ExecuteNonQuery(string sql, object[] args)
        {
            var fql = FQL.Format(SqlServerFQL.Instance, sql, args);
            using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = fql.CommandText;                  //设置CommandText
                cmd.Parameters.AddRange(fql.DbParameters);          //设定Parameters
                var p = new SqlParameter("totle", 0) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(p);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }

}
