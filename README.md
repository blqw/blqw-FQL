�� string.Format ����ȥдsql~   
  
##��ɫ
#### ����
���Ķ��� `FQL`,`FQLResult`,`IFQLProvider`  
#### ��
���ķ��� `FQL.Format` ���� `FQLResult` ����  
`FQLResult` �������������� `CommandText`,`DbParameters`;һ������`ImportOutParameter`   
#### ���
���Է����ڸ���ORM��ԭ��ADO.NET���ʹ��,�򵥷�װ��,ʹ�ø����  
������չ������Ŀ���ܷǳ�����  

*ps:��Ŀ����[blqw.Literacy](https://code.csdn.net/jy02305022/blqw.Literacy)*  

##������־

#### 2014.09.16
* �Ż���ʽ������

#### 2014.09.14
* ͬ������ blqw.Literacy  

#### 2014.09.13
��һ�����,��Ԫ���Ը�����80%  
ʹ��.Net2.0����,

##Demo ��ʾ  
```csharp
static int ExecuteNonQuery(string sql, object[] args)
{
    using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
    using (var cmd = conn.CreateCommand())
    {
        var fql = FQL.Format(sql, args);
        cmd.CommandText = fql.CommandText;                  //����CommandText
        cmd.Parameters.AddRange(fql.DbParameters);          //�趨Parameters
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
}
```
```csharp
var count = ExecuteNonQuery("insert into users(name, address) values({0},{1})","blqw","����");
```