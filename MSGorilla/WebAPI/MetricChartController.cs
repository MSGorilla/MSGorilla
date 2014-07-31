using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Utility;
using MSGorilla.Library.Models;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.WebAPI
{
    public class MetricChartController : BaseController
    {
        MetricManager _metricManager = new MetricManager();

        [HttpGet]
        public DisplayMetricDataSet GetDataSet(int id)
        {
            string me = whoami();
            return _metricManager.GetDateSet(id);
        }

        [HttpGet, HttpPost]
        public DisplayMetricDataSet AddDataSet(string name, string group, string description = null)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);

            return _metricManager.CreateDataSet(me, name, group, description);
        }

        [HttpGet, HttpPost, HttpDelete]
        public ActionResult RemoveDataSet(int id)
        {
            MetricDataSet dataset = _metricManager.GetDateSet(id);
            string me = whoami();

            if (!me.Equals(dataset.Creater))
            {
                MembershipHelper.CheckAdmin(dataset.GroupID, me);
            }

            _metricManager.DeleteDataSet(id);
            return new ActionResult();
        }

        [HttpGet, HttpPost]
        public ActionResult AddRecord(int id, string key, double value)
        {
            MetricDataSet dataset = _metricManager.GetDateSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            _metricManager.AppendDataRecord(id, key, value.ToString());
            return new ActionResult();
        }

        [HttpGet]
        public List<MetricRecord> RetriveLastestDataRecord(int id, int count = 30)
        {
            MetricDataSet dataset = _metricManager.GetDateSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            return _metricManager.RetriveLastestDataRecord(dataset, count);
        }

        [HttpGet]
        public List<MetricRecord> RetriveLastestDataRecord(int id, int startIndex, int endIndex)
        {
            MetricDataSet dataset = _metricManager.GetDateSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            return _metricManager.RetriveDataRecord(dataset.Id, startIndex, endIndex);
        }

        [HttpGet, HttpPost, HttpPut]
        public DisplayMetricChart CreateChart(string chartName, string group, string title, string subtitle = null)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);
            return _metricManager.CreateChart(me, group, title, subtitle);
        }

        [HttpGet]
        public DisplayMetricChart GetChart(string chartName)
        {
            string me = whoami();
            DisplayMetricChart chart = _metricManager.GetChart(chartName);
            MembershipHelper.CheckMembership(chart.GroupID, me);
            return chart;
        }

        [HttpGet, HttpPost, HttpPut]
        public DisplayMetricChart AddDataSet(string chartName, int dataSetID, string legend, string type)
        {
            string me = whoami();
            DisplayMetricDataSet dataset = GetDataSet(dataSetID);
            if (dataset == null)
            {
                throw new MetricDataSetNotFoundException();
            }

            MembershipHelper.CheckMembership(dataset.GroupID, me);
            return _metricManager.AddDataSet(chartName, dataSetID, legend, type);
        }

        [HttpGet, HttpDelete]
        public DisplayMetricChart RemoveDataSet(string chartName, int dataSetID)
        {
            string me = whoami();
            DisplayMetricDataSet dataset = GetDataSet(dataSetID);
            if (dataset == null)
            {
                throw new MetricDataSetNotFoundException();
            }

            MembershipHelper.CheckMembership(dataset.GroupID, me);

            return _metricManager.RemoveDataSet(chartName, dataSetID);
        }

        [HttpGet]
        public List<DisplayMetricChart> GetChartsByGroup(string group)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);
            return _metricManager.GetAllChartByGroup(group);
        }
    }
}