using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ABC
{
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
}
