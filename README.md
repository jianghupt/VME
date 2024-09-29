# 项目简介
项目为个人平时测试研究用途，主要是用.NET8 JIT的虚拟机加密，源代码完全开放。    
加密软件项目Encryption SoftWare文件下面，它是用.NET4.8.1写的Winform窗体应用程序。  
C_DLL项目是C++ 虚拟机加密托管DLL运行时的项目，也就是JIT加密的关键部分。  
C#_DLL是一个C#示例，被Encryption SoftWare加密测试之用，版本.NET8.0.4。  
由于测试用例，代码命名等并不太规范。

## 有问题可以关注公众号:jianghupt了解最新动向。  

## .注意事项
注意事项写在前面，
1.目前加密测试用例是控制台.NET8.0.4版本，其它版本没有测试，所以以下测试建议在.NET8.0.4版本上运行。否则会出错。  
也就是你必须具有以下目录的.NET控制台版本：  
```
C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.4
```
2.如果出错，查看下C_DLL项目的C++代码JIT_VM.cpp里面的MainCode变量：
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


### 1.C#_DLL
这个文件夹里面包含了需要被加密的.NET 托管DLL的例子，托管DLL名：ABC.dll

### 2.C_DLL
这里面包含了通过C++操控JIT对ILCode进行运行时的更改，以便动态的利用虚拟机加密托管DLL。C++ dll：JIT_VM.dll

### 3.Encryption SoftWare
加密软件，通过选择托管的DLL，然后用这个软件进行加密。加密之后的托管DLL，存放路径在托管DLL当前目录的/Proteced/Proteced里面

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

### 5.实际效果
上面第4步实例演示的C#_DLL项目里的C#代码如下：
 ```
    internal class Program  
    {  
        static void GHI()  
        {  
            Console.WriteLine("Call GHI");  
        }  
        static void DEF()  
        {  
            Console.WriteLine("Call DEF");  
        }  
        static void ABC()  
        {  
            Console.WriteLine("Call ABC");  
        }  
        static void Main(string[] args)  
        {  
            ABC();  
        }  
    }  
```
加密之后本来的ABC.dll被放置到了/Proteced/Proteced文件夹里面的Jianghupt.dll，需要依赖JIT_VM.dll进行运行。
所以需要把JIT_VM.dll放置到Jianghupt.dll同一目录，同时需要runtimeconfig.json文件。所以需要把原来的ABC.runtimeconfig.json改成  
Jianghupt.runtimeconfig.json，也放到/Proteced/Proteced文件夹里面。加密之后，代码实际上是变成了如下：  
 ```
    internal class Program  
    {  
        static void GHI()  
        {  
            Console.WriteLine("Call GHI");  
        }  
        static void DEF()  
        {  
            Console.WriteLine("Call DEF");  
        }  
        static void ABC()  
        {  
            Console.WriteLine("Call ABC");  
        }  
        static void Main(string[] args)  
        {  
            DEF();  
        }  
    }  
```

如果到这一步成功了，最终可以看到打印出【Call DEF】。



