用 string.Format 的方式去写sql~   
  
##特色
#### 轻量
核心对象 `FQL`,`FQLResult`,`IFQLProvider`  
#### 简单
核心方法 `FQL.Format` 返回 `FQLResult` 对象  
`FQLResult` 对象有两个属性 `CommandText`,`DbParameters`;一个方法`ImportOutParameter`   
#### 灵活
可以方便于各种ORM或原生ADO.NET结合使用,简单封装后,使用更灵活  
对于拓展现有项目功能非常方便  

##更新日志
#### 2014.09.14
* 同步更新 [blqw.Literacy](https://code.csdn.net/jy02305022/blqw.Literacy)

* 2014.09.13
第一版完成,单元测试覆盖率80%  
使用.Net2.0编译,项目依赖[blqw.Literacy](https://code.csdn.net/jy02305022/blqw.Literacy)  

