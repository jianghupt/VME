using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Assemblies;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        #region 全局变量 函数
        //全局字符串，以便于获取#us的索引
        string jinus = null;
        //全局变量，键值分别为函数名称和函数体的MSIl十六进制
        Dictionary<string, byte[]> keyValuePairs = new Dictionary<string, byte[]>();

        //注意这些非托管DLLImport引入库，是高版本C++生成，.NET低版本会提示格式错误，比如当前的.NET4.8.1调用就会提示
        //[DllImport("E:\\Visual Studio Project\\Test_\\x64\\Debug\\ConsoleApplication5.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void FUnctioNameAndBytes(byte[] intptr);

        #endregion

        #region 窗体初始化
        public Form1()
        {
            InitializeComponent();
        }
        #endregion

        #region 读取PE #us的范围以及查找出字符串在#us的索引
        public string ReadPE(string assembly)
        {
            int GlobalPosion = 0;
            byte[] bytes;
            using (FileStream fileStream = new FileStream(assembly, FileMode.Open))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                for (int i = 0; i < bytes.Length; i++)
                {
                    if ((bytes[i] == 0x42) && (bytes[i + 1] == 0x53) && (bytes[i + 2] == 0x4A) && (bytes[i + 3] == 0x42))//查找PE的BSJB
                    {
                        GlobalPosion = i;
                        break;
                    }
                }
            }
            int yuanshujutou = GlobalPosion;//.NET目录下的元数据头地址
            int usxiang= GlobalPosion += 0x40;//0x40是#us相对于.NET目录下面元数据头的偏移，也即GlobalPostion指向#us项
            byte[] bytess= new byte[4];//读取#us前四个字节，也即是其真正偏移地址
            bytess[0] = bytes[usxiang];
            bytess[1] = bytes[usxiang+1];
            bytess[2] = bytes[usxiang+2];
            bytess[3] = bytes[usxiang+3];
            int number = BitConverter.ToInt32(bytess, 0);//把四个字节转换成数字
            int usvalue = yuanshujutou + number;//把元数据头+偏移，等于#us真正地址。
            byte[] bytesss = new byte[4];//读取#us下面的四个字节，接着上面四字节后面读取
            bytesss[0] = bytes[usxiang + 4];
            bytesss[1] = bytes[usxiang + 5];
            bytesss[2] = bytes[usxiang + 6];
            bytesss[3] = bytes[usxiang + 7];
            int number1=BitConverter.ToInt32(bytesss, 0);//读取#us的长度，以便在此范围内，查找字符串的索引

            int length = usvalue + number1;
            //byte[] b1 = { 0x43, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x6C, 0x00, 0x20, 0x00, 0x41, 0x00, 0x42, 0x00, 0x43, 0x00 };
          
            string usstring = Encoding.ASCII.GetString(bytes, usvalue, number1);
            //int lt = usstring.Length;
            //int index = usstring.IndexOf(hexString, 0, number1);
            return usstring;
        }

        public string suoyin(string str)
        {
            //string str = "Call Main";
            byte[] byteArray = Encoding.ASCII.GetBytes(str);
            byte[] temp = new byte[byteArray.Length * 2];
            int j = 0;
            for (int i = 0; i < temp.Length; i++)//遍历循环，每个字节后都加一个0x00
            {
                if (i % 2 == 0)//奇偶赋值
                {
                    temp[i] = byteArray[i / 2];//temp的4索引，是byteArray的两倍索引，所以除2
                }
                else
                {
                    temp[i] = 0;
                }
            }
            string hexString = Encoding.ASCII.GetString(temp);
            return hexString;
        }

        #endregion
        
        #region  OpCode的 Opreand
        private byte[] GetOperandBytes(Instruction instruction,string assemblypath)
        {
            if (instruction.Operand == null)
            {
                return Array.Empty<byte>();
            }

            switch (instruction.Operand)
            {
                case sbyte sb:
                    return new[] { (byte)sb };
                case byte b:
                    return new[] { b };
                case int i:
                    return BitConverter.GetBytes(i);
                case long l:
                    return BitConverter.GetBytes(l);
                case float f:
                    return BitConverter.GetBytes(f);
                case double d:
                    return BitConverter.GetBytes(d);
                case Instruction target:
                    return BitConverter.GetBytes(target.Offset);
                case Instruction[] targets:
                    return targets.SelectMany(t => BitConverter.GetBytes(t.Offset)).ToArray();
                case VariableDefinition variable:
                    return BitConverter.GetBytes(variable.Index);
                case ParameterDefinition parameter:
                    return BitConverter.GetBytes(parameter.Index);
                case MethodReference method:
                    return BitConverter.GetBytes(method.MetadataToken.ToInt32());
                case FieldReference field:
                    return BitConverter.GetBytes(field.MetadataToken.ToInt32());
                case string str:
                    //var token = BitConverter.GetBytes((uint)instruction.Operand.GetHashCode());
                    var strBytes = Encoding.UTF8.GetBytes(str);
                    int index= jinus.IndexOf(suoyin(Encoding.ASCII.GetString(strBytes)),0,jinus.Length);
                    byte[] ss = { (byte)(index-1), 0x00, 0x00, 0x70 };
                    return  ss;//.Concat(strBytes).ToArray();
                //case System.SByte b:
                //    return return BitConverter.GetBytes(b);
                default:
                    throw new NotSupportedException($"Unsupported operand type: {instruction.Operand.GetType()}");
            }
        }
        #endregion

        #region EnCry DEcry
        public string JiaMiCString(char[] str)
        {
            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                chars[i] = (char)(str[i] - 1);
            }
            return new string(chars);
            //return ;
        }

        public string JieMiCString(char[] str)
        {
            char[] chars = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                chars[i] = (char)(str[i] + 1);
            }
            return new string(chars);
        }
        #endregion

        #region 解析MSIl十六进制的函数名和函数体，以便后续存储存储

        //public (string[], byte[][]) GetFunctionNameAndFunctionBody ()
        //{
        //    string[] str = keyValuePairs.Keys.ToArray();//函数名称
        //    byte[][] bytes = keyValuePairs.Values.ToArray();//函数体MSIL十六进制
        //    return (str, bytes);
        //}

        byte[] DumpMSIL()
        {

            string[] str = keyValuePairs.Keys.ToArray();//函数名称
            byte[][] bytes = keyValuePairs.Values.ToArray();//函数体MSIL十六进制

            byte[] test = bytes[0];
            byte[] test1 = bytes[1];

            StringBuilder sr = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                string str1 = str[i];
                sr.Append(str1+"@@");
            }
            byte[] bb = Encoding.ASCII.GetBytes(sr.ToString());

            byte[] be = new byte[bytes[0].Length + bytes[1].Length];
            for (int i = 0; i < bytes[0].Length; i++)
            {
                be[i] = bytes[0][i];
            }
            for (int i = 0; i < bytes[1].Length; i++)
            {
                be[i + bytes[0].Length] = bytes[1][i];
            }
            byte[] bbb1 = new byte[bb.Length + be.Length];
            Array.Copy(bb, 0, bbb1, 0, bb.Length);
            Array.Copy(be, 0, bbb1, bb.Length, be.Length);
            //IntPtr intPtr = Marshal.AllocHGlobal(bbb1.Length);//申请指针内存空间
            //Marshal.Copy(bbb1, 0, intPtr, bbb1.Length);//把byte转成指针 
            return bbb1;
        }
        #endregion

        //添加DllImport方法
        void AddImportAttribute(string AssemblyPath)
        {
            #region 读取PE文件，获取字符串#us里面的索引
            jinus=ReadPE(AssemblyPath);

            #endregion

            #region 杂项全局

            //int bolbindex = ReadPE(AssemblyPath," ");

            string assemblyPath = AssemblyPath;
            string dir = Path.GetDirectoryName(assemblyPath);
            string outputPath = dir + JieMiCString("[Oqnsdbsdc".ToCharArray());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            
            var module = assembly.MainModule;
            List<string> NameSpaceAndClass = new List<string>();

            var type = module.Types.Single(t => t.Name ==JieMiCString(";Lnctkd=".ToCharArray()));
            if(type == null)
            {
                MessageBox.Show("请选择Program.Main所在托管DLL");
                return ;
            }
            // 遍历每个类型
            
            #endregion

            #region <Module>类添加DllImport的hook方法
            // 创建新的方法并添加 DllImport 特性
            var newMethod = new MethodDefinition(JieMiCString("gnnj".ToCharArray()),
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PInvokeImpl,
                module.ImportReference(typeof(void)));

            var parameter = new ParameterDefinition("intptr", ParameterAttributes.None, module.ImportReference(typeof(IntPtr)));
            newMethod.Parameters.Add(parameter);

            //var parameterr = new ParameterDefinition("intptr", ParameterAttributes.None, module.ImportReference(typeof(IntPtr)));
            //newMethod.Parameters.Add(parameterr);

            //var moduleRef = new ModuleReference("E:\\Visual Studio Project\\Test_\\x64\\Debug\\ConsoleApplication5.dll");

            var moduleRef = new ModuleReference("JIT_VM.dll");
            module.ModuleReferences.Add(moduleRef);

            // 设置 P/Invoke 信息
            var pInvokeInfo = new PInvokeInfo(PInvokeAttributes.CallConvCdecl, JieMiCString("gnnj".ToCharArray()), moduleRef);
            newMethod.ImplAttributes |= MethodImplAttributes.PreserveSig;//注意设置这个属性
            newMethod.PInvokeInfo = pInvokeInfo;

            // 添加新方法到类型
            type.Methods.Add(newMethod);
            #endregion

            #region <Module>类添加DllImport的FUnctioNameAndBytes方法
            // 创建新的方法并添加 DllImport 特性
            var newMethodd = new MethodDefinition("FUnctioNameAndBytes",
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PInvokeImpl,
                module.ImportReference(typeof(void)));

            var parameterr = new ParameterDefinition("intptr", ParameterAttributes.None, module.ImportReference(typeof(byte[])));
            newMethodd.Parameters.Add(parameterr);

            //var parameterr = new ParameterDefinition("intptr", ParameterAttributes.None, module.ImportReference(typeof(IntPtr)));
            //newMethod.Parameters.Add(parameterr);

            //var moduleReff = new ModuleReference("E:\\Visual Studio Project\\Test_\\x64\\Debug\\ConsoleApplication5.dll");
            module.ModuleReferences.Add(moduleRef);

            // 设置 P/Invoke 信息
            var pInvokeInfoo = new PInvokeInfo(PInvokeAttributes.CallConvCdecl, "FUnctioNameAndBytes", moduleRef);
            newMethodd.ImplAttributes |= MethodImplAttributes.PreserveSig;
            newMethodd.PInvokeInfo = pInvokeInfoo;

            // 添加新方法到类型
            type.Methods.Add(newMethodd);
            #endregion

            #region  <Module>类添加添加DllImport的getJit方法
            var newMethod1 = new MethodDefinition(JieMiCString("fdsIhs".ToCharArray()),
        MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PInvokeImpl,
        module.ImportReference(typeof(IntPtr)));

            //var parameter1 = new ParameterDefinition("intptr", ParameterAttributes.None, module.ImportReference(typeof(IntPtr)));
            //newMethod1.Parameters.Add(parameter1);
            //string vr= Environment.Version.ToString();

            var moduleRef1 = new ModuleReference(JieMiCString("B9[Oqnfq`l\u001fEhkdr[cnsmds[rg`qdc[Lhbqnrnes-MDSBnqd-@oo[7-/-3[bkqihs-ckk".ToCharArray()));

            //var moduleRef1 = new ModuleReference("G:\\runtime9_pre3\\runtime\\artifacts\\bin\\coreclr\\windows.x64.Debug\\clrjit.dll");

            module.ModuleReferences.Add(moduleRef1);

            // 设置 P/Invoke 信息
            var pInvokeInfo1 = new PInvokeInfo(PInvokeAttributes.CallConvStdCall, JieMiCString("fdsIhs".ToCharArray()), moduleRef1);
            newMethod1.PInvokeInfo = pInvokeInfo1;
            newMethod1.ImplAttributes |= MethodImplAttributes.PreserveSig;
            // 添加新方法到类型
            type.Methods.Add(newMethod1);
            #endregion

            #region 注释
            //这行注释的代码，如果是<Module>模块则不需要，因为全局只有一个，如果是Main函数所在模块
            //或者其它托管函数所在模块，则需要遍历循环找出。
            //foreach (var ml in assembly.Modules)
            //{
            //    foreach (var ty in ml.Types)
            //    {
            //        if (ty.Name == "<Module>")//类名为<Module>
            //        {
            //            IEnumerable<MethodDefinition> IM = ty.GetMethods();
            //            foreach (var m in IM)
            //            {
            //                if (m.Name == ".cctor")
            //                {
            //                    NameSpaceAndClass.Add(ty.Name);
            //                    NameSpaceAndClass.Add(ty.Namespace);
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
            //string SpaceAndClass = "<Module>";
            //string SpaceAndClass = NameSpaceAndClass[1].ToString() + "." + NameSpaceAndClass[0].ToString();
            //var type = module.GetType(SpaceAndClass);

            #endregion

            #region 添加<Module>..cctor静态构造
            var cctor = type.Methods.FirstOrDefault(m => m.Name == JieMiCString("-bbsnq".ToCharArray()));
            if (cctor == null)
            {
                cctor = new MethodDefinition(JieMiCString("-bbsnq".ToCharArray()),
                    MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    module.ImportReference(typeof(void)));
                type.Methods.Add(cctor);
            }
            #endregion

            //下面的代码没用到

            #region  Program类添加添加DllImport VirtualProtect方法
            //        var newMethod2 = new MethodDefinition("VirtualProtect",
            //MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PInvokeImpl | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot,
            //module.ImportReference(typeof(bool)));

            //        var uintType = module.ImportReference(typeof(uint));

            //        var parameter2 = new ParameterDefinition("lpAddress", ParameterAttributes.None, module.ImportReference(typeof(IntPtr)));
            //        var parameter22 = new ParameterDefinition("dwSize", ParameterAttributes.None, module.ImportReference(typeof(UIntPtr)));
            //        var parameter222 = new ParameterDefinition("newproj", ParameterAttributes.None, module.ImportReference(typeof(uint)));
            //        var parameter2222 = new ParameterDefinition("oldpro", ParameterAttributes.Out, uintType.MakeByReferenceType());//注意这里的，参数带out

            //        newMethod2.Parameters.Add(parameter2);
            //        newMethod2.Parameters.Add(parameter22);
            //        newMethod2.Parameters.Add(parameter222);
            //        newMethod2.Parameters.Add(parameter2222);

            //        var moduleRef2 = new ModuleReference("kernel32.dll");
            //        module.ModuleReferences.Add(moduleRef2);

            //        // 设置 P/Invoke 信息
            //        var pInvokeInfo2 = new PInvokeInfo(PInvokeAttributes.CallConvWinapi, "VirtualProtect", moduleRef2);
            //        newMethod2.PInvokeInfo = pInvokeInfo2;
            //        newMethod2.ImplAttributes |= MethodImplAttributes.PreserveSig;
            //        // 添加新方法到类型
            //        targetType.Methods.Add(newMethod2);

            #endregion

            #region ConsoleApp5.Program里面添加hookVP方法

            //// 创建新方法
            //var hookVPMethod = new MethodDefinition("hookVP",
            //    MethodAttributes.Public | MethodAttributes.Static|MethodAttributes.HideBySig,
            //    module.TypeSystem.Void);

            //// 添加参数
            //var intPtrType = module.ImportReference(typeof(IntPtr));
            //hookVPMethod.Parameters.Add(new ParameterDefinition("address", ParameterAttributes.None, intPtrType));
            //hookVPMethod.Parameters.Add(new ParameterDefinition("addressvalue", ParameterAttributes.None, intPtrType));

            //// 创建方法体  var val = targetType.GetMethods().FirstOrDefault(t => t.Name == "VirtualProtect");
            //var il = hookVPMethod.Body.GetILProcessor();

            //il.Append(il.Create(OpCodes.Ldstr, "Module static constructor is called."));
            ////il.Append(il.Create(OpCodes.Ldarg_0));
            //il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
            //il.Append(il.Create(OpCodes.Ret));

            //var oldProVar = new VariableDefinition(module.TypeSystem.UInt32);
            //hookVPMethod.Body.Variables.Add(oldProVar);
            //hookVPMethod.Body.InitLocals = true;


            ////var assembly111 = AssemblyDefinition.ReadAssembly("kernel32.dll");
            ////var module111 = assembly111.MainModule;
            ////module111.ImportReference(typeof);

            //var virtualProtectMethod = module.ImportReference(newMethod2);
            ////var virtualProtectMethod = module.ImportReference(targetType.Methods.First(m => m.Name == "VirtualProtect"));
            //////var virtualProtectMethod = targetType.GetMethods().FirstOrDefault(t => t.Name == "VirtualProtect");
            //var marshalWriteIntPtrMethod = module.ImportReference(typeof(Marshal).GetMethod("WriteIntPtr", new[] { typeof(IntPtr), typeof(IntPtr) }));

            //// VirtualProtect(address, 8, 0x40, out oldpro)
            //il.Append(il.Create(OpCodes.Nop));

            //il.Append(il.Create(OpCodes.Ldarg_0));
            //il.Append(il.Create(OpCodes.Ldc_I4_8));
            //il.Append(il.Create(OpCodes.Conv_I));
            //il.Append(il.Create(OpCodes.Ldc_I4_S, (sbyte)0x40));
            //il.Append(il.Create(OpCodes.Ldloca_S, oldProVar));
            //il.Append(il.Create(OpCodes.Call, virtualProtectMethod));

            //// Marshal.WriteIntPtr(address, addressvalue)
            ////il.Append(il.Create(OpCodes.Nop));
            //il.Append(il.Create(OpCodes.Pop));
            //il.Append(il.Create(OpCodes.Ldarg_0));
            //il.Append(il.Create(OpCodes.Ldarg_1));
            //il.Append(il.Create(OpCodes.Call, marshalWriteIntPtrMethod));

            //// VirtualProtect(address, 8, oldpro, out oldpro)
            //il.Append(il.Create(OpCodes.Nop));
            //il.Append(il.Create(OpCodes.Ldarg_0));
            //il.Append(il.Create(OpCodes.Ldc_I4_8));
            //il.Append(il.Create(OpCodes.Conv_I));
            //il.Append(il.Create(OpCodes.Ldloc_0));
            //il.Append(il.Create(OpCodes.Ldloca_S, oldProVar));
            //il.Append(il.Create(OpCodes.Call, virtualProtectMethod));

            //il.Append(il.Create(OpCodes.Nop));
            //il.Append(il.Create(OpCodes.Ret));

            //// 将方法添加到目标类型
            //targetType.Methods.Add(hookVPMethod);
            #endregion

            assembly.Write(outputPath + JieMiCString("[Ih`mfgtos-ckk".ToCharArray()));
        }

        void AddModuleCCtor(string AssemblyPath)
        {
            #region 杂项
            string assemblyPath = AssemblyPath;
            string dir = Path.GetDirectoryName(assemblyPath);
            string outputPath = dir + JieMiCString("[Oqnsdbsdc".ToCharArray());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var module = assembly.MainModule;
            List<string> NameSpaceAndClass = new List<string>();

            var type = module.Types.Single(t => t.Name == JieMiCString(";Lnctkd=".ToCharArray()));

            var cctor = type.Methods.FirstOrDefault(m => m.Name == JieMiCString("-bbsnq".ToCharArray()));
            var getjit = type.Methods.FirstOrDefault(m => m.Name == JieMiCString("fdsIhs".ToCharArray()));
            var hook = type.Methods.FirstOrDefault(m => m.Name == JieMiCString("gnnj".ToCharArray()));
            var FUnctioNameAndBytes = type.Methods.FirstOrDefault(m => m.Name == "FUnctioNameAndBytes");
            //var VirtualProtect = type.Methods.FirstOrDefault(m => m.Name == "VirtualProtect");

            if (cctor == null)
            {
                cctor = new MethodDefinition(JieMiCString("-bbsnq".ToCharArray()),
                    MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    module.ImportReference(typeof(void)));
                type.Methods.Add(cctor);
            }

            #endregion

            //考虑这个地的代码，如果把它放到AddImportAttribute函数，获取的是ConsoleApp5.dll原始。原始的托管DLL跟Jianghupt.dll函数体不同
            //比如：280D00000A IL_0006: call void [System.Console]System.Console::WriteLine(string)，ConsoleApp5.dll机器码是280D00000A
            //而Jianghupt.dll里面则是280100000A IL_0006: call void [System.Console]System.Console::WriteLine(string),机器码是:280100000A
            //函数体的MSIL十六进制，把它放到这里，这里加载的是Proteced/Jianghupt.dll托管DLL，它的函数体
            //跟当前函数下面保存的程序集函数体是相同的，所以加密Main这段代码必须放在这里。
            #region 加密Main,获取函数名和函数体MSIL十六进制码，且替换其body
            var targetType = module.Types.FirstOrDefault(t => t.Name == "Program");
            if (targetType == null)
            {
                Console.WriteLine("Target class Program not found.");
                return;
            }
            //var MainMethod = targetType.GetMethods().FirstOrDefault(t => t.Name == "Main");
            var countMethods = targetType.GetMethods();
            foreach (var itemMethod in countMethods)
            {
                ILProcessor ill = itemMethod.Body.GetILProcessor();
                int codesize = itemMethod.Body.CodeSize;
                if (codesize == 0) return;
                //int ilcount = itemMethod.Body.Instructions.Count();
                var instructions = itemMethod.Body.Instructions;
                byte[] opcodeBytes = new byte[codesize];
                //byte[] operandBytes= new byte[codesize];
                int flag = 0;
                for (int k = 0; k < instructions.Count; k++)
                {
                    var opcode = instructions[k].OpCode;

                    if (opcode.Size == 1)
                    {
                        opcodeBytes[flag] = opcode.Op2;
                        flag = flag + 1;
                    }
                    else
                    {
                        opcodeBytes[flag] = opcode.Op1;
                        opcodeBytes[flag + 1] = opcode.Op2;
                        flag = flag + 2;
                    }

                    //Console.Write($"IL_{instruction.Offset:X4}: {BitConverter.ToString(opcodeBytes)}");
                    //int rv=itemMethod.RVA;
                    var operandBytes = GetOperandBytes(instructions[k], assemblyPath);//如果操作码，有操作数，这里还需要对操作数进行处理，获取MSIl十六进制码
                    if (operandBytes != null && operandBytes.Length > 0)
                    {
                        //int flags = flag + 1;
                        for (int m = 0; m < operandBytes.Length; m++)
                            opcodeBytes[flag + m] = operandBytes[m];
                        //Console.Write($" {BitConverter.ToString(operandBytes)}");
                        flag += (operandBytes.Length);
                    }
                }
                flag = 0;//索引重置为0

                keyValuePairs.TryGetValue(itemMethod.Name, out var bl);
                if (bl == null) keyValuePairs.Add(itemMethod.Name, opcodeBytes);//把函数名和解析的函数体MSIl十六进制字节码写入键值变量。

                for (int i = 0; i < countMethods.Count(); i++)
                {

                }

                //替换函数体，把它变成throw形式,注意这段代码，如果把注释取消掉，则因为CompileMethod不能正确识别函数，导致任何修改都会错误
                //所以这里需要注释掉，以便进行调试。等所有完成，再把注释取消。
                //if (codesize < 0xD)
                //{
                //    ill.Clear();
                //    for (int i = 0; i < codesize; i++)
                //    {
                //        ill.Append(ill.Create(OpCodes.Nop));
                //    }
                //}
                //else
                //{
                //    int shengyuILcount = codesize - 0xD;
                //    ill.Clear();
                //    ill.Append(ill.Create(OpCodes.Nop));
                //    ill.Append(ill.Create(OpCodes.Ldstr, "ABCDEFGHIJKL"));
                //    ill.Append(ill.Create(OpCodes.Stloc_0));
                //    ill.Append(ill.Create(OpCodes.Newobj, module.ImportReference(typeof(NotImplementedException).GetConstructor(Type.EmptyTypes))));
                //    ill.Append(ill.Create(OpCodes.Throw));
                //    for (int i = 0; i < shengyuILcount; i++)
                //    {
                //        ill.Append(ill.Create(OpCodes.Nop));
                //    }
                //}
            }


            //module.ImportReference(typeof(Marshal).GetMethod("ReadIntPtr", new Type[] { typeof(IntPtr) }))
            #endregion

            #region <Module>..cctor注入代码

            var fieldType = module.ImportReference(typeof(byte[]));
            var newField = new FieldDefinition("be", FieldAttributes.Static  | FieldAttributes.Assembly, fieldType);
            //newField.InitialValue =new byte[]{ 0x1,0x2,0x3};//当你不知道如何给newField赋值，可以看看newField属性，比如InitialValue。
            type.Fields.Add(newField);

            byte[] bt = DumpMSIL();

            ILProcessor il = cctor.Body.GetILProcessor();
            //Console.WriteLine代码注入
            //il.Append(il.Create(OpCodes.Ldstr, "Module static constructor is called."));
            //il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
            //il.Append(il.Create(OpCodes.Ret));
            //var retInstruction = il.Create(OpCodes.Ret);

            //注入一个全局byte数组，存储函数名和函数体的MSIL十六进制
            il.Emit(OpCodes.Ldc_I4, bt.Length); // 数组长度
            il.Emit(OpCodes.Newarr, module.TypeSystem.Byte); // 创建byte数组         
            for (int i = 0; i < bt.Length; i++)
            {
                il.Emit(OpCodes.Dup); // 复制数组引用（为了接下来的元素赋值）
                il.Emit(OpCodes.Ldc_I4, i); // 索引0
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)bt[i]); //注意这的Ldc_I4_S有个_S，(sbyte)bt[i]有个(sbyte)转换，否则错误
                il.Emit(OpCodes.Stelem_I1); // 存储到数组      
            }
            il.Emit(OpCodes.Stsfld, newField);

            //上面注入了byte[]数组，下面是函数调用
            var uintType = module.ImportReference(typeof(uint));
            var intPtrType = module.ImportReference(typeof(IntPtr));
            var oldProtectVariable = new VariableDefinition(uintType);
            var jitVariable = new VariableDefinition(intPtrType);
            var ptr1Variable = new VariableDefinition(intPtrType);
            var ptr2Variable = new VariableDefinition(intPtrType);
            var bytes = new VariableDefinition(module.ImportReference(typeof(byte[])));

            cctor.Body.Variables.Add(oldProtectVariable);
            cctor.Body.Variables.Add(jitVariable);
            cctor.Body.Variables.Add(ptr1Variable);
            cctor.Body.Variables.Add(ptr2Variable);
            cctor.Body.Variables.Add(bytes);
            cctor.Body.InitLocals = true;

            //var typetarget = module.Types.FirstOrDefault(t => t.Name == "Program");
            //var hookVPMethod = module.ImportReference(type.Methods.First(m => m.Name == ".cctor"));
            //var hook=module.ImportReference(cctor);
            //var consoleReadLineMethod = module.ImportReference(typeof(Console).GetMethod("ReadLine", Type.EmptyTypes));
            //il.Append(il.Create(OpCodes.Call, consoleReadLineMethod));//这两句是调用Console.ReadLine

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ldsfld,newField));//调用FUnctioNameAndBytes函数，先加载字段newField
            il.Append(il.Create(OpCodes.Call, FUnctioNameAndBytes));
            il.Append(il.Create(OpCodes.Call, getjit));
            il.Append(il.Create(OpCodes.Stloc_1));
            il.Append(il.Create(OpCodes.Ldloc_1));
            //il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(Marshal).GetMethod("ReadIntPtr", new Type[] { typeof(IntPtr) }))));
            //il.Append(il.Create(OpCodes.Stloc, ptr1Variable));
            ////il.Append(il.Create(OpCodes.Stloc_2));
            //il.Append(il.Create(OpCodes.Ldloc_2));
            //il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(Marshal).GetMethod("ReadIntPtr", new Type[] { typeof(IntPtr) }))));
            //il.Append(il.Create(OpCodes.Stloc_3));
            //il.Append(il.Create(OpCodes.Ldloc_3));
            //il.Append(il.Create(OpCodes.Ldloc_2));
            il.Append(il.Create(OpCodes.Call, hook));
            //il.Append(il.Create(OpCodes.Stloc_S, hookptrVariable));
            //il.Append(il.Create(OpCodes.Ldloc, ptr1Variable));
            //il.Append(il.Create(OpCodes.Ldloc, hookptrVariable));
            //il.Append(il.Create(OpCodes.Call, hookVPMethod));

            #region <Module>..cctor里面调用VirtualProtect函数，因为出错，放到非托管里面去了，这里用不到，此处注释
            /*
            VirtualProtect(num, 8u, 4u, out var oldpro);
            Marshal.WriteIntPtr(num, val);
            VirtualProtect(num, 8u, oldpro, out oldpro);
            */
            //以下代码是以上代码的OpCodes
            //il.Append(il.Create(OpCodes.Ldloc_2));
            //il.Append(il.Create(OpCodes.Ldc_I4_8));
            //il.Append(il.Create(OpCodes.Conv_I));
            ////il.Append(il.Create(OpCodes.Ldc_I4_S, (sbyte)0x40));//注意这里的sbyte转换，否则这条语句报错,或者下面这条语句替代也可以
            //il.Append(il.Create(OpCodes.Ldc_I4, 0x04));
            //il.Append(il.Create(OpCodes.Ldloca_S, oldProtectVariable));
            //il.Append(il.Create(OpCodes.Call, VirtualProtect));
            //il.Append(il.Create(OpCodes.Nop));
            //il.Append(il.Create(OpCodes.Ldloc_2));
            //il.Append(il.Create(OpCodes.Ldloc_S, hookptrVariable));
            //il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(Marshal).GetMethod("WriteIntPtr", new Type[] { typeof(IntPtr), typeof(IntPtr) }))));
            //il.Append(il.Create(OpCodes.Nop));
            //il.Append(il.Create(OpCodes.Ldloc_2));
            //il.Append(il.Create(OpCodes.Ldc_I4_8));
            //il.Append(il.Create(OpCodes.Conv_I));
            //il.Append(il.Create(OpCodes.Ldloc_0));
            //il.Append(il.Create(OpCodes.Ldloca_S, oldProtectVariable));
            //il.Append(il.Create(OpCodes.Call, VirtualProtect));
            #endregion

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));
            #endregion

            assembly.Write(outputPath + JieMiCString("[Ih`mfgtos-ckk".ToCharArray()));
            MessageBox.Show("加密成功，请到路径：" + outputPath + JieMiCString("[Ih`mfgtos-ckk".ToCharArray()) + "下查看");
        }

        void FileCaoZuo(string outputPath)
        {
            //DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);
            //DirectoryInfo parentDirectory = directoryInfo.Parent;
            //if (parentDirectory != null)
            //{
            //    File.Delete(parentDirectory.FullName + "\\Jianghupt.dll");
            //}
            //File.Move(outputPath + "\\Jianghupt.dll", parentDirectory.FullName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "*.dll|*.*";
            if (file.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = file.FileName;
            }
            else
            {
                MessageBox.Show("请选择托管DLL文件");
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string dir = Path.GetDirectoryName(textBox1.Text.Trim());

            AddImportAttribute(textBox1.Text.Trim());
            AddModuleCCtor(dir + "\\Protected\\Jianghupt.dll");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Assembly executingAssembly = Assembly.GetExecutingAssembly();
            //if (false)
            //{
            //}
            //Module module = executingAssembly.GetModules()[0];
        }
    }
}
