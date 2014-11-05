#blqw.FQL 像 string.Format 那样去写sql~   
  
##特色
#### 轻量
核心对象 `FQL`,`FQLResult`,`IFQLProvider`  
#### 简单
核心方法 `FQL.Format` 返回 `FQLResult` 对象  
`FQLResult` 对象有两个属性 `CommandText`,`DbParameters`;一个方法`ImportOutParameter`   
#### 灵活
可以方便于各种ORM或原生ADO.NET结合使用,简单封装后,使用更灵活  
对于拓展现有项目功能非常方便  

*ps:项目依赖[blqw.Literacy](https://code.csdn.net/jy02305022/blqw.Literacy)*  

##更新日志
#### 2014.11.05
* 直接引入Literacy源码,不用在额外引用项目
* IFQLBuilder修改方法名 Comma 为 Concat(bate阶段会发生对象命名修改的情况,请见谅)
* IFQLBuilder增加IsEmpty()方法判断是否从未执行过Append,And,Or或Concat方法
* 小幅度优化代码

#### 2014.10.10
* 优化格式化机制:现在"{0:Name}"除了属性,还会匹配公共的字段

#### 2014.09.16
* 优化格式化过程

#### 2014.09.14
* 同步更新 blqw.Literacy  

#### 2014.09.13
第一版完成,单元测试覆盖率80%  
使用.Net2.0编译,

##Demo 演示  
```csharp
static int ExecuteNonQuery(string sql, object[] args)
{
    using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
    using (var cmd = conn.CreateCommand())
    {
        var fql = FQL.Format(sql, args);
        cmd.CommandText = fql.CommandText;                  //设置CommandText
        cmd.Parameters.AddRange(fql.DbParameters);          //设定Parameters
        conn.Open();
        var r = cmd.ExecuteNonQuery();
        fql.ImportOutParameter();                           //将out类型的值导入到args参数
        return r;
    }
}
```
```csharp
var count = ExecuteNonQuery("insert into users(name, address) values({0},{1})","blqw","杭州");
```
或
```csharp
var user = new User { ID = 0, Name = "blqw", Address = "杭州" };
var count = ExecuteNonQuery(
        @"insert into users(name, address) values({0:name},{0:address});
          set {0:out id} = @@identity;"
        ,user);
return count == 1 ? user.ID : -1;
```