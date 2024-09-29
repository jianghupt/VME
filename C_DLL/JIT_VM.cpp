#include <cstdint>
#include<Windows.h>
#include<stdio.h>

#include <string>
#include <iostream>
#include <vector>

#pragma region MyRegion   CORINFO_METHOD_INFO结构
typedef struct CORINFO_METHOD_STRUCT_* CORINFO_METHOD_HANDLE;
typedef struct CORINFO_MODULE_STRUCT_* CORINFO_MODULE_HANDLE;

enum CorInfoOptions
{
    CORINFO_OPT_INIT_LOCALS = 0x00000010, // zero initialize all variables

    CORINFO_GENERICS_CTXT_FROM_THIS = 0x00000020, // is this shared generic code that access the generic context from the this pointer?  If so, then if the method has SEH then the 'this' pointer must always be reported and kept alive.
    CORINFO_GENERICS_CTXT_FROM_METHODDESC = 0x00000040, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodDesc)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE
    CORINFO_GENERICS_CTXT_FROM_METHODTABLE = 0x00000080, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodTable)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE
    CORINFO_GENERICS_CTXT_MASK = (CORINFO_GENERICS_CTXT_FROM_THIS |
    CORINFO_GENERICS_CTXT_FROM_METHODDESC |
        CORINFO_GENERICS_CTXT_FROM_METHODTABLE),
    CORINFO_GENERICS_CTXT_KEEP_ALIVE = 0x00000100, // Keep the generics context alive throughout the method even if there is no explicit use, and report its location to the CLR

};


enum CorInfoRegionKind
{
    CORINFO_REGION_NONE,
    CORINFO_REGION_HOT,
    CORINFO_REGION_COLD,
    CORINFO_REGION_JIT,
};


enum CorInfoCallConv
{
    // These correspond to CorCallingConvention

    CORINFO_CALLCONV_DEFAULT = 0x0,
    // Instead of using the below values, use the CorInfoCallConvExtension enum for unmanaged calling conventions.
    // CORINFO_CALLCONV_C          = 0x1,
    // CORINFO_CALLCONV_STDCALL    = 0x2,
    // CORINFO_CALLCONV_THISCALL   = 0x3,
    // CORINFO_CALLCONV_FASTCALL   = 0x4,
    CORINFO_CALLCONV_VARARG = 0x5,
    CORINFO_CALLCONV_FIELD = 0x6,
    CORINFO_CALLCONV_LOCAL_SIG = 0x7,
    CORINFO_CALLCONV_PROPERTY = 0x8,
    CORINFO_CALLCONV_UNMANAGED = 0x9,
    CORINFO_CALLCONV_NATIVEVARARG = 0xb,    // used ONLY for IL stub PInvoke vararg calls

    CORINFO_CALLCONV_MASK = 0x0f,     // Calling convention is bottom 4 bits
    CORINFO_CALLCONV_GENERIC = 0x10,
    CORINFO_CALLCONV_HASTHIS = 0x20,
    CORINFO_CALLCONV_EXPLICITTHIS = 0x40,
    CORINFO_CALLCONV_PARAMTYPE = 0x80,     // Passed last. Same as CORINFO_GENERICS_CTXT_FROM_PARAMTYPEARG
};


typedef uint8_t COR_SIGNATURE;
typedef struct CORINFO_CLASS_STRUCT_* CORINFO_CLASS_HANDLE;
typedef struct CORINFO_ARG_LIST_STRUCT_* CORINFO_ARG_LIST_HANDLE;
typedef const COR_SIGNATURE* PCCOR_SIGNATURE;
struct MethodSignatureInfo;
typedef uint32_t mdToken;


struct CORINFO_SIG_INST
{
    unsigned                classInstCount;
    CORINFO_CLASS_HANDLE* classInst; // (representative, not exact) instantiation for class type variables in signature
    unsigned                methInstCount;
    CORINFO_CLASS_HANDLE* methInst; // (representative, not exact) instantiation for method type variables in signature
};

struct CORINFO_SIG_INFO
{
    CorInfoCallConv         callConv;
    CORINFO_CLASS_HANDLE    retTypeClass;   // if the return type is a value class, this is its handle (enums are normalized)
    CORINFO_CLASS_HANDLE    retTypeSigClass;// returns the value class as it is in the sig (enums are not converted to primitives)
    int             retType : 8;
    unsigned                flags : 8;    // used by IL stubs code
    unsigned                numArgs : 16;
    struct CORINFO_SIG_INST sigInst;        // information about how type variables are being instantiated in generic code
    CORINFO_ARG_LIST_HANDLE args;
    PCCOR_SIGNATURE         pSig;
    unsigned                cbSig;
    MethodSignatureInfo* methodSignature;// used in place of pSig and cbSig to reference a method signature object handle
    CORINFO_MODULE_HANDLE   scope;          // passed to getArgClass
    mdToken                 token;

    CorInfoCallConv     getCallConv() { return CorInfoCallConv((callConv & CORINFO_CALLCONV_MASK)); }
    bool                hasThis() { return ((callConv & CORINFO_CALLCONV_HASTHIS) != 0); }
    bool                hasExplicitThis() { return ((callConv & CORINFO_CALLCONV_EXPLICITTHIS) != 0); }
    bool                hasImplicitThis() { return ((callConv & (CORINFO_CALLCONV_HASTHIS | CORINFO_CALLCONV_EXPLICITTHIS)) == CORINFO_CALLCONV_HASTHIS); }
    unsigned            totalILArgs() { return (numArgs + (hasImplicitThis() ? 1 : 0)); }
    bool                isVarArg() { return ((getCallConv() == CORINFO_CALLCONV_VARARG) || (getCallConv() == CORINFO_CALLCONV_NATIVEVARARG)); }
    bool                hasTypeArg() { return ((callConv & CORINFO_CALLCONV_PARAMTYPE) != 0); }
};

struct CORINFO_METHOD_INFO
{
    CORINFO_METHOD_HANDLE       ftn;
    CORINFO_MODULE_HANDLE       scope;
    uint8_t* ILCode;
    unsigned                    ILCodeSize;
    unsigned                    maxStack;
    unsigned                    EHcount;
    CorInfoOptions              options;
    CorInfoRegionKind           regionKind;
    CORINFO_SIG_INFO            args;
    CORINFO_SIG_INFO            locals;
};


#pragma endregion

#pragma region MyRegion  函数指针和全局变量

typedef int(*compileMethod_def)(long long* intptr,
    long long* compHnd,
    CORINFO_METHOD_INFO* methodInfo,
    unsigned             flags,
    uint8_t** entryAddress,
    uint32_t* nativeSizeOfCode
    );


typedef int (*compileMethod_zidingyi)(void* comp,            /* IN */
    void* methodInfo,      /* IN */
    unsigned             flags,           /* IN */
    uint8_t** nativeEntry,     /* OUT */
    uint32_t* nativeSizeOfCode /* OUT */
);

typedef size_t(*printMethodName)(
    long long* compHnd,
    CORINFO_METHOD_HANDLE ftn,
    char* buffer,
    size_t                bufferSize,
    size_t* pRequiredBufferSize
    );


std::string strFunctionName;
byte FunctionBoyd[];
compileMethod_def GlobalPtr;
//LONG64 GlobalPtr;
DWORD OldProtect;

#pragma endregion

#pragma region MyRegion  测试的MSIl十六进制码
 

BYTE MainCode[13] =
{
    //1.注意这里的MSIL机器码，不能写错了，否则出错
    //2.字节码跟原有字节码ILCodeSize长度一样，否则出错
    0x00,
    0x72,0x13,0x00,0x00,0x70,
    0x28,0x01,0x00,0x00,0x0A,
    0x00,0x02A
};

BYTE ABCCode[13] =
{
    //1.注意这里的MSIL机器码，不能写错了，否则出错
    //2.字节码跟原有字节码ILCodeSize长度一样，否则出错
    0x00,0x28,0x06,0x00,0x00,0x06,0x00,0x2A
};


#pragma endregion

#pragma region 自定义的CompileMethod


int my_compileMethod(long long* intptr,
    long long* compHnd,
    CORINFO_METHOD_INFO* methodInfo,
    unsigned             flags,
    uint8_t** entryAddress,
    uint32_t* nativeSizeOfCode
)
{
    size_t requiredBufferSize;
    char   buffer[100];
    //获取当前编译的函数名称
    ((printMethodName)*((long long*)*compHnd + 0x72))(compHnd, methodInfo->ftn, buffer, sizeof(buffer), &requiredBufferSize);

    if (strcmp(buffer, "Main") == 0) //如果是Main 则进行ILCode动态替换
    {
        DWORD oldPro;
        VirtualProtect((LONG64*)*((LONG64*)((byte*)methodInfo + 0x10)), *((int*)((byte*)methodInfo + 0x18)), 0x40, &oldPro);
        for (int i = 0; i < *((int*)((byte*)methodInfo + 0x18)); i++)
        {
            *((byte*)((LONG64*)*((LONG64*)((byte*)methodInfo + 0x10)))+i) = ABCCode[i];
        }
        VirtualProtect((LONG64*)*((LONG64*)((byte*)methodInfo + 0x10)), *((int*)((byte*)methodInfo + 0x18)), oldPro, &oldPro);
    }

    int nRet = GlobalPtr(intptr,compHnd, methodInfo, flags, entryAddress, nativeSizeOfCode);//继续运行
    return nRet;
}


#pragma endregion


#pragma region byte[]和string[]操作


#pragma endregion


#pragma region
extern "C" __declspec(dllexport) void hook(LONG64 **ptr)
{
    GlobalPtr = (compileMethod_def)**ptr;
    DWORD oldProtect;
    VirtualProtect(*ptr, 8, 0x40,  &OldProtect);
    **ptr = (LONG64) & my_compileMethod;
    VirtualProtect(*ptr, 8, OldProtect, &OldProtect);
}

extern "C" __declspec(dllexport) void FUnctioNameAndBytes(byte* ptr)
{

    //std::vector<unsigned char> byteArray = { ptr[0], ptr[1], ptr[2]};
    //std::string str=std::string(byteArray.begin(), byteArray.end());
    //printf("%s\r\n", str);

    //std::vector<std::string> strVector;
    //strVector.push_back("Dynamic");
    //strVector.push_back("Size");
    //strVector.push_back("Strings");

}


extern "C" __declspec(dllexport) int Add(int x,int y)
{
    return x + y;
}
#pragma endregion


