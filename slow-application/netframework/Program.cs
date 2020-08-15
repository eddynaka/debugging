using System;
using System.Threading;

namespace SlowApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            while (true)
            {
                Console.WriteLine(i);
                Thread.Sleep(i * 500);
                i++;
            }
        }
    }
}
