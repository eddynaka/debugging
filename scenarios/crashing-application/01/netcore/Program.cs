using System;
using System.Threading;

namespace CrashingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Thread.Sleep(60_000);

            throw new Exception("crashing my application");
        }
    }
}
