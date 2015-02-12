using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.YahooStockReporter
{
    public class StockData
    {
        //tmp += "<Symbol>&lt;span style='color:red'&gt;" +
        //symbols[i].ToUpper() +
        //" is invalid.&lt;/span&gt;</Symbol>";
        //tmp += "<Last></Last>";
        //tmp += "<Date></Date>";
        //tmp += "<Time></Time>";
        //tmp += "<Change></Change>";
        //tmp += "<High></High>";
        //tmp += "<Low></Low>";
        //tmp += "<Volume></Volume>";
        //tmp += "<Bid></Bid>";
        //tmp += "<Ask></Ask>";
        //tmp += "<Ask></Ask>";

        public string Symbol { get; set; }
        public double Last { get; set; }
        public DateTime Date { get; set; }
        public double Change { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public long Volumn { get; set; }

        public double Bid { get; set; }
        public double Ask { get; set; }
        public double ChangeInPercent { get; set; }

    }
}
