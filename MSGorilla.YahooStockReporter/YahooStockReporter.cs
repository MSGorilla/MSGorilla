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
    class YahooStockReporter
    {
        private Timer _dataCollectTimer;
        private Timer _dataPostTimer;
        private string _stocks = "YHOO+GOOG+MSFT";
        Dictionary<string, Dictionary<DateTime, StockData>> _stockDataSet = new Dictionary<string, Dictionary<DateTime, StockData>>();
        GorillaWebAPI _client;
        Object _lock = new object();

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
            _dataCollectTimer.Interval = interval.Milliseconds;
            _dataCollectTimer.Start();

            firstRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);
            interval = firstRunTime - now;
            _dataPostTimer.Interval = interval.Milliseconds;
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

        private bool DoDataPost()
        {
            // Check paremeters
            if (_stocks == "")
            {
                return false;
            }

            try
            {
                foreach (var dataSet in _stockDataSet)
                {

                    // Create figure
                    Figure figure = new Figure(dataSet.Key, DateTime.UtcNow.ToString(), dataSet.Value);

                    // Post 
                    _client.PostMessage(figure.ToString(), null, "chart-axis-singleaxis");

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


    public class Figure
    {
        public class FigureYAxis
        {
            public string name { get; set; }
            public string type { get; set; }
            public List<double> data { get; set; }
            public FigureYAxis(string n, string t)
            {
                name = n;
                type = t;
            }
        }

        public string title { get; set; }
        public string subtitle { get; set; }
        public List<string> legend { get; set; }
        public List<string> xAxis { get; set; }
        public List<FigureYAxis> yAxis { get; set; }

        public Figure(string title, string subtitle, Dictionary<DateTime, StockData> data)
        {
            this.title = title;
            this.subtitle = subtitle;
            this.legend = new List<string>();
            this.xAxis = new List<string>();
            this.yAxis = new List<FigureYAxis>();

            foreach (var d in data)
            {
                xAxis.Add(string.Format("{0:yyyy-MM-dd}", d.Key));
            }
        }

        protected void Addline(string name, string type, List<double> count)
        {
            FigureYAxis yaxis = new FigureYAxis(name, type);
            yaxis.data = count;

            this.yAxis.Add(yaxis);
            this.legend.Add(name);
        }

        public string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
