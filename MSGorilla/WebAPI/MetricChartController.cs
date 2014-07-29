using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Utility;
using MSGorilla.Library.Models;

namespace MSGorilla.WebAPI
{
    public class MetricChartController : BaseController
    {
        MetricManager _metricManager = new MetricManager();

        [HttpGet, HttpPost]
        public MetricDataSet AddDataSet(string name, string group, string description = null)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);

            return _metricManager.AddDataSet(me, name, group, description);
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

            _metricManager.RemoveDataSet(id);
            return new ActionResult();
        }

        [HttpGet, HttpPost]
        public ActionResult AddRecord(int id, string key, string value)
        {
            MetricDataSet dataset = _metricManager.GetDateSet(id);
            MembershipHelper.CheckMembership(dataset.GroupID, whoami());

            _metricManager.AppendDataRecord(id, key, value);
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
    }
}