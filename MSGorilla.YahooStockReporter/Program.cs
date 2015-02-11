using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.YahooStockReporter
{
    class Program
    {
        static void Main(string[] args)
        {

            YahooStockReporter reporter = new YahooStockReporter();
            //reporter.Start();
            reporter.DoDataCollect("YHOO+GOOG+MSFT");

            Console.WriteLine("Type 'exit' to exit...");
            string input = Console.ReadLine();
            while (!input.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                input = Console.ReadLine();
            }

            reporter.Stop();
        }
    }
}
