using MSGorilla.WebAPI.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MSGorilla.YahooStockReporter
{
    public class YahooStockReporter
    {
        private Timer _dataCollectTimer;
        private Timer _dataPostTimer;
        private string _stocks = "YHOO+GOOG+MSFT";
        private Dictionary<string, Dictionary<DateTime, StockData>> _stockDataSet = new Dictionary<string, Dictionary<DateTime, StockData>>();
        private GorillaWebAPI _client;
        private Object _lock = new object();

        public YahooStockReporter()
        {
            _client = new GorillaWebAPI("stockrobot", "User@123");

            // Create data collecting timer
            _dataCollectTimer = new Timer();
            _dataCollectTimer.Elapsed += new ElapsedEventHandler(OnDataCollectTimedEvent);
            _dataCollectTimer.AutoReset = true;

            // Create data posting timer
            _dataPostTimer = new Timer();
            _dataPostTimer.Elapsed += new ElapsedEventHandler(OnDataPostTimedEvent);
            _dataPostTimer.AutoReset = true;
        }

        public void Start()
        {
            DateTime now = DateTime.UtcNow;
            DateTime firstRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, 0);
            TimeSpan interval = firstRunTime - now;
            _dataCollectTimer.Interval = interval.TotalMilliseconds;
            _dataCollectTimer.Start();

            firstRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 30);
            interval = firstRunTime - now;
            _dataPostTimer.Interval = interval.TotalMilliseconds;
            _dataPostTimer.Start();
        }

        private void OnDataPostTimedEvent(object sender, ElapsedEventArgs e)
        {
            _dataPostTimer.Interval = 3600 * 1000;   //run every hour
            int i = 0; // retry 3 times
            while (DoDataPost() == false && i < 3)
            {
                i++;
            }
        }

        private void OnDataCollectTimedEvent(object sender, ElapsedEventArgs e)
        {
            _dataCollectTimer.Interval = 60 * 1000;   //run every minute

            int i = 0; // retry 3 times
            while (DoDataCollect() == false && i < 3)
            {
                i++;
            }
        }

        public void Stop()
        {
            _dataCollectTimer.Enabled = false;
            _dataPostTimer.Enabled = false;
        }

        /// <summary>
        /// This function handles and parses multiple stock symbols as input parameters
        /// and builds a stockdata.
        /// </summary>
        /// <param name="_stocks">A bunch of stock symbols
        ///    seperated by "+"</param>
        /// <returns>Return true of false</returns>
        public bool DoDataCollect()
        {
            // Check paremeters
            if (_stocks == "")
            {
                return false;
            }

            try
            {
                // Use Yahoo finance service to download stock data from Yahoo
                string yahooURL = @"http://download.finance.yahoo.com/d/quotes.csv?s=" +
                                  _stocks + "&f=sl1d1t1c1hgvbap2";
                string[] symbols = _stocks.Replace("+", " ").Split(' ');

                // Initialize a new WebRequest.
                HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(yahooURL);
                // Get the response from the Internet resource.
                HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
                // Read the body of the response from the server.
                StreamReader strm = new StreamReader(webresp.GetResponseStream(), Encoding.ASCII);

                // Get data
                string content = "";
                for (int i = 0; i < symbols.Length; i++)
                {
                    // Loop through each line from the stream,
                    // building the return XML Document string
                    if (symbols[i].Trim() == "")
                        continue;

                    content = strm.ReadLine().Replace("\"", "");
                    string[] contents = content.ToString().Split(',');
                    Console.WriteLine("Get Data : " + content);

                    // If contents[2] = "N/A". the stock symbol is invalid.
                    if (contents[2] == "N/A")
                    {
                        continue;
                    }
                    else
                    {
                        StockData data = new StockData();

                        try { data.Symbol = contents[0].Trim(); }
                        catch { continue; }
                        try { data.Last = Convert.ToDouble(contents[1].Trim()); }
                        catch { continue; }
                        try { data.Date = Convert.ToDateTime(contents[2].Trim() + " " + contents[3].Trim()); }
                        catch { continue; }
                        try { data.Change = Convert.ToDouble(contents[4].Trim()); }
                        catch { continue; }
                        try { data.High = Convert.ToDouble(contents[5].Trim()); }
                        catch { data.High = data.Last; }
                        try { data.Low = Convert.ToDouble(contents[6].Trim()); }
                        catch { data.Low = data.Last; }
                        try { data.Volumn = Convert.ToInt64(contents[7].Trim()); }
                        catch { continue; }
                        try { data.Bid = Convert.ToDouble(contents[8].Trim()); }
                        catch { data.Bid = 0; }
                        try { data.Ask = Convert.ToDouble(contents[9].Trim()); }
                        catch { data.Ask = 0; }
                        try { data.ChangeInPercent = Convert.ToDouble(contents[10].Trim('%')); }
                        catch { continue; }

                        // Store data
                        lock (_lock)
                        {
                            Dictionary<DateTime, StockData> dataList = null;
                            if (_stockDataSet.ContainsKey(data.Symbol))
                            {
                                dataList = _stockDataSet[data.Symbol];
                            }
                            else
                            {
                                dataList = new Dictionary<DateTime, StockData>();
                                _stockDataSet.Add(data.Symbol, dataList);
                            }
                            if (!dataList.ContainsKey(data.Date))
                            {
                                dataList.Add(data.Date, data);
                            }
                        }

                        // Post alert
                        if (Math.Abs(data.ChangeInPercent) >= 1)
                        {
                            string message = string.Format(
@"Stock Price Alert:
The price change of {0} reached {1:c} ({2}%) at {3}",
                                           data.Symbol, data.Change, data.ChangeInPercent, data.Date.ToShortTimeString());
                            _client.PostMessage(message, null, "none", "none", new string[] { "Stock", "Stock " + data.Symbol });
                            Console.WriteLine(message);
                        }
                    }
                }

                // Close the StreamReader object.
                strm.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        public bool DoDataPost()
        {
            // Check paremeters
            if (_stocks == "")
            {
                return false;
            }

            try
            {
                lock (_lock)
                {
                    foreach (var dataSet in _stockDataSet)
                    {
                        string symbol = dataSet.Key;
                        Dictionary<DateTime, StockData> data = dataSet.Value;

                        if (data.Count > 1)
                        {
                            // Create figure
                            StockFigure figure = new StockFigure(symbol, DateTime.UtcNow.ToString(), data);
                            List<double> price = new List<double>();
                            List<double> volumn = new List<double>();
                            foreach (var d in data)
                            {
                                price.Add(d.Value.Last);
                                volumn.Add(d.Value.Volumn);
                            }

                            figure.Addline("Volumn", "bar", volumn);
                            figure.Addline2("Price", "line", price);

                            // Post 
                            string message = figure.ToString();
                            _client.PostMessage(message, null, "chart-axis-doubleaxes", "none", new string[] { "Stock", "Stock " + symbol });
                            Console.WriteLine(message);
                        }
                    }
                    _stockDataSet.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

    }


}
