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
```
static int ExecuteNonQuery(string sql, object[] args)
{
    var fql = FQL.Format(sql, args);
    using (var conn = new SqlConnection("Data Source=.;Initial Catalog=Test;Integrated Security=True"))
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = fql.CommandText;                  //����CommandText
        cmd.Parameters.AddRange(fql.DbParameters);          //�趨Parameters
        var p = new SqlParameter("totle", 0) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(p);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
}
```