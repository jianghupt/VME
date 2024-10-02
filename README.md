# 项目简介
项目测试研究用途，主要是用.NET8 JIT的虚拟机加密，源代码完全开放。    
C#_DLL是一个C#示例，被Encryption SoftWare加密测试之用，版本.NET8.0.4。    
C_DLL项目是C++ 虚拟机加密托管DLL运行时的项目，也就是JIT加密的关键部分。  
加密软件项目Encryption SoftWare文件下面，它是用.NET4.8.1写的Winform窗体应用程序。    

由于测试用例，代码命名等并不太规范。  
测试用例，对托管Main函数进行了动态拦截(虚拟机加密),改变其函数调用。  

##### 有问题可以关注公众号:jianghupt了解最新动向。    

## 虚拟机加密原理
原理：通过hook jit的编译函数compileMethod，对其参数的ILCode进行动态更改。比如当CLR运行调用JIT的时候，判断当前JIT编译的函数是否是托管Main  
如果是则把托管Main的ILCode更改为需要的ILcode，也即是十六进制更改。  
关于JIT加密参考下图：
![image](https://github.com/user-attachments/assets/631cd060-7070-44e7-99a8-adbcc4692100)


## .注意事项
注意事项写在前面  

1.目前加密测试用例是控制台.NET8.0.4版本，其它版本没有测试，所以以下测试建议在.NET8.0.4版本上运行。否则会出错。  
也就是你必须具有以下目录的.NET控制台版本：  
```
C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.4
```

2.如果还是出错，查看下C_DLL项目的C++代码JIT_VM.cpp里面的MainCode变量：
 ```
BYTE MainCode[13] =
{
    //1.注意这里的MSIL机器码，不能写错了，否则出错
    //2.字节码跟原有字节码ILCodeSize长度一样，否则出错
    0x00,
    0x72,0x13,0x00,0x00,0x70,
    0x28,0x01,0x00,0x00,0x0A,
    0x00,0x02A
};
 ```
把里面的十六进制更改成C#_DLL生成的托管DLL的DEF函数的十六进制函数体。  

3.当前示例，匹配当前项目，专测试之用。如果要加密其它托管DLL，需要自行扩展。也就是说当前只能加密C#_DLL项目的ABC.DLL，其它出错

4.因为代码涉及到通过mono.cecil注入，hook JIT等操作。一些低级杀毒软件可能报毒，这点需注意。  

## 介绍
### 1.C#_DLL
这个文件夹里面包含了需要被加密的.NET 托管DLL的例子，托管DLL名：ABC.dll

### 2.C_DLL
这里面包含了通过C++操控JIT对ILCode进行运行时的更改，以便动态的利用虚拟机加密托管DLL。C++ dll：JIT_VM.dll

### 3.Encryption SoftWare
加密软件，通过选择示例托管的DLL，用这个软件进行加密。加密之后的托管DLL，存放路径在托管DLL当前目录的/Proteced/Proteced里面

### 4.Demo
演示的例子，演示步骤如下：

打开Demo文件夹  
1.双击加密软件.exe  

2.路径选择当前目录下(Demo文件夹目录下)的ABC.dll，这个dll是上面介绍的C#_DLL项目里面生成的。

3.点击加密  

4.在当前目录(Demo文件夹下)的/Proteced/Proteced里面生成了一个托管Jianghupt.dll， 
把Demo文件夹下的ABC.runtimeconfig.json放到/Proteced/Proteced目录改名为：Jianghupt.runtimeconfig.json    
把JIT_VM.dll(它是支撑JIT加密的C++ DLL，也即是上面介绍的C_DLL项目)也放入到/Proteced/Proteced文件夹。  

5.到此时，如果一切没有出错。命令：dotnet Jianghupt.dll 即可运行看到效果。本来调用的ABC()函数被JIT动态Hook成了DEF()函数。  

6.在Jianghupt.dll被CLR加载调用JIT的时候，关联了JIT_VM.dll，会被其hook更改了托管Main函数里面的IL代码。以此运行真正的代码。  


