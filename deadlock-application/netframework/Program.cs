using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeadlockApplication
{
    class Program
    {
        private static object lockA = new object();
        private static object lockB = new object();

        static void Main(string[] args)
        {
            var taskA = Task.Run(() =>
            {
                MethodA();
            });

            var taskB = Task.Run(() =>
            {
                MethodB();
            });

            Task.WaitAll(taskA, taskB);
            Console.WriteLine("Finished...");
        }

        private static void MethodA()
        {
            lock(lockA)
            {
                Thread.Sleep(1000);
                lock (lockB)
                {
                    Console.WriteLine("Hello");
                }
            }            
        }

        private static void MethodB()
        {
            lock(lockB)
            {
                Thread.Sleep(1000);
                lock (lockA)
                {
                    Console.WriteLine("World");
                }
            }
        }
    }
}
