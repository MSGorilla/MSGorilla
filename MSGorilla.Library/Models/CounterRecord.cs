using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Exceptions;

namespace MSGorilla.Library.Models
{
    public static class CounterRecordHelper
    {
        /// <summary>
        /// get the value from the counter and path.
        /// path should be a string splited by dot, like BugQuality.TableQuality.TableBugFixRate
        /// return null if such path doesn't exist
        /// </summary>
        /// <param name="record"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CounterRecord.ComplexValue Get(CounterRecord record, string path)
        {
            if (record == null || record.Value == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] names = path.Split('.');
            CounterRecord.ComplexValue cur = record.Value;
            foreach (string name in names)
            {
                if (cur.RelatedValues == null || cur.RelatedValues.Count == 0)
                {
                    return null;
                }

                foreach (var value in cur.RelatedValues)
                {
                    if (value != null && name.Equals(value.Name))
                    {
                        cur = value;
                        break;
                    }
                }
                if (cur.Name != name)
                {
                    return null;
                }
            }

            return cur == null ? null : cur;
        }


        public static object GetValue(CounterRecord record, string path)
        {
            CounterRecord.ComplexValue cur = CounterRecordHelper.Get(record, path);
            return cur == null ? null : cur.Value;
        }

        public static CounterRecord ParseEntity(DynamicTableEntity entity)
        {
            CounterRecord record = new CounterRecord(entity.Properties["Key"].StringValue);

            byte[] bin = Utils.GetBinFromEntity(entity);
            if (bin == null || bin.Length == 0)
            {
                return record;
            }

            using (Stream stream = new MemoryStream(bin))
            {
                record.Value = new BinaryFormatter().Deserialize(stream) as CounterRecord.ComplexValue;
            }

            return record;
        }

        public static DynamicTableEntity ToTableEntity(CounterRecord record, string pk, string rk)
        {
            DynamicTableEntity entity = new DynamicTableEntity(pk, rk);
            entity.Properties["Key"] = new EntityProperty(record.Key);
            entity.Properties["Timestamp"] = new EntityProperty(DateTimeOffset.UtcNow);

            //Serialize ComplexValue Value
            using (MemoryStream mstream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(mstream, record.Value);
                byte[] bin = mstream.ToArray();

                if (!Utils.AttachBin2Entity(entity, bin))
                {
                    throw new CounterTooLargeException();
                }
            }

            return entity;
        }
    }
}
