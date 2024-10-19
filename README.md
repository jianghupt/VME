## Language

- [English](https://github.com/jianghupt/VME/blob/main/README.md)
- [中文](https://github.com/jianghupt/VME/blob/main/README_zh.md)



# Project Introduction
The purpose of the project test research is mainly to use .NET8 JIT virtual machine encryption, and the source code is completely open.
C#_DLL is a C# example used for encryption testing by Encryption SoftWare, version .NET8.0.4.
The C_DLL project is a C++ virtual machine encryption managed DLL runtime project, which is the key part of JIT encryption.
Under the Encryption SoftWare file of the encryption software project, it is a Winform form application written in .NET4.8.1.

Due to the test cases, code naming, etc. are not very standardized.
The test case dynamically intercepts the managed Main function (virtual machine encryption) and changes its function call.

##### If you have any questions, you can follow the official account: jianghupt to learn about the latest trends.
## The principle of virtual machine encryption
Principle: By hooking the compiled function of jit, the ILCode of its parameters is dynamically changed. For example, when the CLR runs and calls JIT, it determines whether the current JIT-compiled function is a managed Main
If it is, change the ILCode of the managed Main to the required ILcode, that is, a hexadecimal change.
For JIT encryption, please refer to the following article:
.Net Virtual Machine (CLR/JIT) Encryption Principle (Copyright Protection)
image

## .Notes
Notes are written in the front

1. The current encryption test case is the console .NET8.0.4 version, and other versions have not been tested, so the following tests are recommended to be run on the .NET8.0.4 version. Otherwise, an error will occur.
That is, you must have the .NET console version in the following directory:

C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.4

2. If the error still occurs, check the MainCode variable in the C++ code JIT_VM.cpp of the C_DLL project:

BYTE MainCode[13] =
{
//1. Pay attention to the MSIL machine code here. Do not write it wrong, otherwise an error will occur
//2. The bytecode is the same length as the original bytecode ILCodeSize, otherwise an error will occur
0x00,
0x72,0x13,0x00,0x00,0x70,
0x28,0x01,0x00,0x00,0x0A,
0x00,0x02A
};
Change the hexadecimal inside to the hexadecimal function body of the DEF function of the managed DLL generated by C#_DLL.

3. The current example matches the current project and is specifically used for testing. If you want to encrypt other managed DLLs, you need to expand them yourself. That is to say, currently only ABC.DLL of C#_DLL project can be encrypted, and other errors will occur.

4. Because the code involves injection through mono.cecil, hook JIT and other operations. Some low-level antivirus software may report viruses, so please pay attention to this.

## Introduction
### 1.C#_DLL
This folder contains examples of .NET managed DLLs that need to be encrypted. Managed DLL name: ABC.dll

### 2.C_DLL
This contains runtime changes to ILCode through C++ manipulation of JIT, so as to dynamically use the virtual machine to encrypt managed DLLs. C++ dll: JIT_VM.dll

### 3.Encryption SoftWare
Encryption software, by selecting the sample managed DLL, use this software to encrypt. The encrypted managed DLL is stored in the /Proteced/Proteced directory of the managed DLL.

### 4. Demo
Demonstration example, the demonstration steps are as follows:

Open the Demo folder

1. Double-click the encryption software.exe

2. Select the ABC.dll in the current directory (Demo folder directory). This dll is generated in the C#_DLL project introduced above.

3. Click Encrypt

4. A managed Jianghupt.dll is generated in /Proteced/Proteced in the current directory (Demo folder). Put the ABC.runtimeconfig.json in the Demo folder into the /Proteced/Proteced directory and rename it to: Jianghupt.runtimeconfig.json
Put JIT_VM.dll (it is the C++ DLL that supports JIT encryption, that is, the C_DLL project introduced above) into the /Proteced/Proteced folder.

5. At this point, if everything is correct. Command: dotnet Jianghupt.dll to run and see the effect. The ABC() function that was originally called was dynamically hooked by JIT into the DEF() function.

6. When Jianghupt.dll is loaded by CLR and calls JIT, it is associated with JIT_VM.dll, and its hook changes the IL code in the managed Main function. In this way, the real code is run.
