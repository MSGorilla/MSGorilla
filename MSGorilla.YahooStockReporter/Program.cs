using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.YahooStockReporter
{
    public class Program
    {
        static void Main(string[] args)
        {

            YahooStockReporter reporter = new YahooStockReporter();
            reporter.Start();
            //reporter.DoDataCollect();
            //reporter.DoDataPost();

            Console.WriteLine("Type 'exit' to exit...");
            string input = Console.ReadLine().Trim();
            while (!input.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                input = Console.ReadLine().Trim();
            }

            reporter.Stop();
        }
    }
}
