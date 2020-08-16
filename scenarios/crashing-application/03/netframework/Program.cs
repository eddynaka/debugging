using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrashingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            Process();
        }

        private static void Process()
        {
            try
            {
                throw new Exception("some random exception");
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        private static void WriteLog(Exception ex)
        {
            try
            {
                throw new IOException("Not Authorized");
            }
            catch (Exception nex)
            {
                WriteLog(nex);
                throw;
            }
        }
    }
}
