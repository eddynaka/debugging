using System;

namespace MemoryLeakApplication
{
    public class Customer
    {
        public Guid Id { get; set; }

        public Customer(Guid guid)
        {
            this.Id = guid;
        }
    }
}
