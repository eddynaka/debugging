using System.Collections.Generic;

namespace MemoryLeakApplication
{
    public static class Helper
    {
        private static readonly List<Customer> Customers = new List<Customer>();

        public static void Add(Customer customer)
        {
            Customers.Add(customer);
        }

        public static void AddRange(IEnumerable<Customer> customers)
        {
            Customers.AddRange(customers);
        }

        public static int Count()
        {
            return Customers.Count;
        }
    }
}
