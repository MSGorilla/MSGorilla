using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using MSGorilla.Library;
using MSGorilla.Library.Models;
using MSGorilla.Utility;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.WebAPI
{

    public class CounterController : BaseController
    {
        CounterManager _counterManager = new CounterManager();

        [HttpGet]
        public List<DisplayCounterSet> GetCounterSetByGroup(string group = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            List<CounterSet> css = _counterManager.GetCounterSetByGroup(group);
            List<DisplayCounterSet> dcss = new List<DisplayCounterSet>();
            foreach(CounterSet cs in css)
            {
                DisplayCounterSet dcs = new DisplayCounterSet(cs);
                if (cs.RecordCount > 0)
                {
                    CounterRecord record = _counterManager.GetSingleCounterRecord(cs.Group, cs.Name, cs.RecordCount - 1);
                    foreach (var cv in record.Value.RelatedValues)
                    {
                        dcs.Entry.Add(new DisplayCounter(cv.Name, cv.Type));
                    }
                }
                
                dcss.Add(dcs);
            }
            return dcss;
        }

        [HttpGet, HttpPut, HttpPost]
        public CounterSet CreateCounterSet(string name, string group = null, string description = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            return _counterManager.CreateCounterSet(group, name, description);
        }

        [HttpPost]
        public ActionResult InsertRecord([FromUri]string name, [FromUri]string group = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            var task = this.Request.Content.ReadAsStreamAsync();
            task.Wait();
            Stream requestStream = task.Result;
            CounterRecord record = new BinaryFormatter().Deserialize(requestStream) as CounterRecord;

            _counterManager.InsertCounterRecord(record, name, group);
            return new ActionResult();
        }

        [HttpGet]
        public DisplayCounterRecords GetCounterRecord(string path, string name, string group = null, int start = 0, int end = -1)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            List<DisplayRecord> records = new List<DisplayRecord>();
            foreach (var record in _counterManager.GetCounterRecords(name, group, start, end))
            {
                DisplayRecord rec = new DisplayRecord(record.Key, CounterRecordHelper.GetValue(record, path));
                records.Add(rec);
            }

            return new DisplayCounterRecords(name, group, path, start, records);
        }

        [HttpGet]
        public DisplayCounterRecords GetLatestCounterRecord(string path, string name, string group = null, int count = 30)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            CounterSet counterSet = _counterManager.GetCounterSet(group, name);
            if (counterSet == null)
            {
                throw new CounterSetNotFoundException();
            }

            int start = counterSet.RecordCount - count > 0 ? counterSet.RecordCount - count : 0;
            List<DisplayRecord> records = new List<DisplayRecord>();
            foreach (var record in _counterManager.GetCounterRecords(name, group, start))
            {
                DisplayRecord rec = new DisplayRecord(record.Key, CounterRecordHelper.GetValue(record, path));
                records.Add(rec);
            }

            return new DisplayCounterRecords(name, group, path, start, records);
        }

        [HttpGet]
        public DisplayCounterChart GetCounterChart(string path, string name, string group = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            CounterSet cs = _counterManager.GetCounterSet(group, name);
            if (cs == null)
            {
                throw new CounterSetNotFoundException();
            }

            CounterRecord record = _counterManager.GetSingleCounterRecord(group, name, cs.RecordCount - 1);
            CounterRecord.ComplexValue cv = CounterRecordHelper.Get(record, path);
            if(cv == null)
            {
                return null;
            }

            DisplayCounterChart dcc = new DisplayCounterChart();
            dcc.Title = cv.Name;
            dcc.MainCounter = new DisplayCounter(path, cv.Type);
            dcc.RelatedCounter = new List<DisplayCounter>();
            foreach (var value in cv.RelatedValues)
            {
                dcc.RelatedCounter.Add(new DisplayCounter(string.Format("{0}.{1}", path, value.Name), value.Type));
            }

            return dcc;
        }
    }
}