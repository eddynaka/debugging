using System;
using System.Collections.Generic;
using System.Threading;

namespace MemoryLeakApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 1;

            while (true)
            {
                Helper.AddRange(GetCustomers(i));
                Console.WriteLine(Helper.Count());
                i++;
            }
        }

        private static IEnumerable<Customer> GetCustomers(int i)
        {
            for (int k = 0; k < i; k++)
            {
                yield return new Customer(Guid.NewGuid());
            }

            Thread.Sleep(1_000);
        }
    }
}
