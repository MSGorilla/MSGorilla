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

        /// <summary>
        /// Get dataset info by ID
        /// </summary>
        /// <param name="id">Dataset ID</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMetricDataSet GetDataSet(int id)
        {
            string me = whoami();
            return _metricManager.GetDataSet(id);
        }

        /// <summary>
        /// Get all dataset infos in a group 
        /// </summary>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMetricDataSet> GetDataSetByGroup(string group)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);
            List<DisplayMetricDataSet> ans = new List<DisplayMetricDataSet>();
            foreach (var dataset in _metricManager.GetAllDataSetByGroup(group))
            {
                ans.Add(dataset);
            }
            return ans;
        }

        /// <summary>
        /// Create a new dataset
        /// </summary>
        /// <param name="name">dataset name</param>
        /// <param name="group">group id</param>
        /// <param name="description">description</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayMetricDataSet CreateDataSet(string name, string group, string description = null)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);

            return _metricManager.CreateDataSet(me, name, group, description);
        }

        /// <summary>
        /// Delete a dataset by id. You must be either group admin or the dataset creater
        /// </summary>
        /// <param name="id">dataset id</param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpDelete]
        public ActionResult DeleteDataSet(int id)
        {
            MetricDataSet dataset = _metricManager.GetDataSet(id);
            string me = whoami();

            if (!me.Equals(dataset.Creater))
            {
                MembershipHelper.CheckAdmin(dataset.GroupID, me);
            }

            _metricManager.DeleteDataSet(id);
            return new ActionResult();
        }

        /// <summary>
        /// Add a record in a dataset.
        /// Record is a key value pair and the value should be a double type
        /// </summary>
        /// <param name="id">dataset id</param>
        /// <param name="key">key is string</param>
        /// <param name="value">value is double</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public ActionResult InsertRecord(int id, string key, double value)
        {
            MetricDataSet dataset = _metricManager.GetDataSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            _metricManager.AppendDataRecord(id, key, value.ToString());
            return new ActionResult();
        }

        /// <summary>
        /// Retrive the lastest inserted records in a data set.
        /// </summary>
        /// <param name="id">dataset id</param>
        /// <param name="count">count</param>
        /// <returns></returns>
        [HttpGet]
        public List<MetricRecord> RetriveLastestDataRecord(int id, int count = 30)
        {
            MetricDataSet dataset = _metricManager.GetDataSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            return _metricManager.RetriveLastestDataRecord(dataset, count);
        }

        /// <summary>
        /// Retrive records from a dataset. 
        /// </summary>
        /// <param name="id">dataset id</param>
        /// <param name="startIndex">start index</param>
        /// <param name="endIndex">end index</param>
        /// <returns></returns>
        [HttpGet]
        public List<MetricRecord> RetriveDataRecord(int id, int startIndex, int endIndex)
        {
            MetricDataSet dataset = _metricManager.GetDataSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            return _metricManager.RetriveDataRecord(dataset.Id, startIndex, endIndex);
        }

        /// <summary>
        /// create a new chart
        /// </summary>
        /// <param name="chartName">chart name</param>
        /// <param name="group">group id</param>
        /// <param name="title">title</param>
        /// <param name="subtitle">subtitle</param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut]
        public DisplayMetricChart CreateChart(string chartName, string group, string title, string subtitle = null)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);
            return _metricManager.CreateChart(chartName, group, title, subtitle);
        }

        /// <summary>
        /// get chart by name. You should belong to the group of the chart.
        /// </summary>
        /// <param name="chartName">chart name.</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMetricChart GetChart(string chartName)
        {
            string me = whoami();
            DisplayMetricChart chart = _metricManager.GetChart(chartName);
            MembershipHelper.CheckMembership(chart.GroupID, me);
            return chart;
        }

        /// <summary>
        /// Add a dataset into a chart. Chart and dataset should belong to the same group
        /// </summary>
        /// <param name="chartName">chart name</param>
        /// <param name="dataSetID">dataset id</param>
        /// <param name="legend">legend</param>
        /// <param name="type">type, can be "line" or "bar"</param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove a dataset from a chart
        /// </summary>
        /// <param name="chartName">chart name</param>
        /// <param name="dataSetID">dataset id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get all charts in a group
        /// </summary>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMetricChart> GetChartsByGroup(string group)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);
            return _metricManager.GetAllChartByGroup(group);
        }
    }
}