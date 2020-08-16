using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrashingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(60_000);
            try
            {
                HttpRequest();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(@"C:/Windows/private.txt", ex.ToString());
            }
        }

        private static bool HttpRequest()
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync("https://www.wronggithuburl.com").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
    }
}
