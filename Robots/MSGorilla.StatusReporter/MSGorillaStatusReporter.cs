using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MSGorilla.WebAPI.Client;

namespace MSGorilla.StatusReporter
{
    public class MSGorillaStatusReporter : StatusReporter
    {
        GorillaWebAPI _client;
        List<HistoryData> datas;

        public MSGorillaStatusReporter()
            : base()
        {
            _client = new GorillaWebAPI("MSGorillaStatusReporter", "User@123");
        }

        public override void Report()
        {
            Logger.Info("Report latest status to MSGorilla.");

            List<HistoryData> datas = _collector.LoadDataByDateUtc(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

            Figure figure = new UserCountTrendFigure(datas);
            _client.PostMessage(figure.ToString(), "chart-axis-singleaxis");
            figure = new TopicTrendFigure(datas);
            _client.PostMessage(figure.ToString(), "chart-axis-singleaxis");


            if (datas[datas.Count - 1].CountOfMsgPostedByRobot == 0)
            {
                datas.RemoveAt(datas.Count - 1);
            }
            figure = new RobotMsgTrendFigure(datas);
            _client.PostMessage(figure.ToString(), "chart-axis-singleaxis");

            Logger.Info("Draw topic, user and message trend on MSGorilla.");
        }

        public class Figure
        {
            public class FigureYAxis
            {
                public string name { get; set; }
                public string type { get; set; }
                public List<int> data { get; set; }
                public FigureYAxis(string n, string t){
                    name = n;
                    type = t;
                }
            }

            public string title { get; set; }
            public string subtitle { get; set; }
            public List<string> legend { get; set; }
            public List<string> xAxis { get; set; }
            public List<FigureYAxis> yAxis { get; set; }

            public Figure(List<HistoryData> data, string title, string subtitle)
            {
                this.title = title;
                this.subtitle = subtitle;
                this.legend = new List<string>();
                this.xAxis = new List<string>();
                this.yAxis = new List<FigureYAxis>();

                foreach (HistoryData d in data)
                {
                    xAxis.Add(string.Format("{0:yyyy-MM-dd}", d.Date));
                }
            }

            protected void Addline(string name, string type, List<int> count)
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

        public class UserCountTrendFigure : Figure
        {
            public UserCountTrendFigure(List<HistoryData> data):
                base(data, "User Trend", "users and robots")
            {
                List<int> usercount = new List<int>();
                List<int> robotcount = new List<int>();
                foreach (HistoryData d in data)
                {
                    usercount.Add(d.UserCount);
                    robotcount.Add(d.RobotCount);
                }

                this.Addline("Users", "line", usercount);
                this.Addline("Robots", "line", robotcount);
            }
        }

        public class TopicTrendFigure : Figure
        {
            public TopicTrendFigure(List<HistoryData> data) :
                base(data, "Topic Trend", "")
            {
                List<int> topicCount = new List<int>();
                foreach (HistoryData d in data)
                {
                    topicCount.Add(d.TopicCount);
                }

                this.Addline("Topics", "line", topicCount);
            }
        }

        public class RobotMsgTrendFigure : Figure
        {
            public RobotMsgTrendFigure(List<HistoryData> data) :
                base(data, "Message Trend", "")
            {
                List<int> msgCount = new List<int>();
                foreach (HistoryData d in data)
                {
                    msgCount.Add(d.CountOfMsgPostedByRobot);
                }

                this.Addline("Messages", "line", msgCount);
            }
        }
    }
}
