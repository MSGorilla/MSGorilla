using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library
{
    public class Utils
    {
        public static string MD5Encoding(string rawPass)
        {
            if (string.IsNullOrEmpty(rawPass))
            {
                return "d41d8cd98f00b204e9800998ecf8427e";
            }

            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hs)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string EncodeBase64(string code)
        {
            string encode = "";
            byte[] bytes = Encoding.UTF8.GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }

        public static string ToAzureStorageSecondBasedString(DateTime timestamp)
        {
            return ((long)DateTime.MaxValue.Subtract(timestamp.ToUniversalTime()).TotalMilliseconds).ToString();
            //return timestamp.ToUniversalTime().ToString("yyyyMMddHHmmss");
        }

        public static string ToAzureStorageDayBasedString(DateTime timestamp)
        {
            return ((long)DateTime.MaxValue.Subtract(timestamp.ToUniversalTime()).TotalDays).ToString();
            //return timestamp.ToUniversalTime().ToString("yyyyMMdd");
        }

        public static string NextKeyString(string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                return "";
            }

            char[] array = current.ToCharArray();
            array[array.Length - 1]++;
            return new string(array);
        }

        public static bool IsValidID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (id.Contains('\\') || 
                id.Contains(' ') || 
                id.Contains('?') || 
                id.Contains('/') || 
                id.Contains('#') ||
                id.Contains('\t') ||
                id.Contains('\n') ||
                id.Contains('\r'))
            {
                return false;
            }

            return true;
        }

        public static string Token2String(TableContinuationToken token)
        {
            if (token == null)
            {
                return null;
            }
            string ret = (token.NextPartitionKey == null ? "" : token.NextPartitionKey) + ";";
            ret += (token.NextRowKey == null ? "" : token.NextRowKey) + ";";
            ret += (token.NextTableName == null ? "" : token.NextTableName) + ";";
            ret += (token.TargetLocation == StorageLocation.Primary ? "Primary" : "Secondary") + ";";
            return ret;
        }

        public static TableContinuationToken String2Token(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            string[] parts = token.Split(';');
            if (parts.Length < 4)
            {
                return null;
            }
            TableContinuationToken ret = new TableContinuationToken();
            ret.NextPartitionKey = parts[0];
            ret.NextRowKey = parts[1];
            ret.NextTableName = string.IsNullOrEmpty(parts[2]) ? null : parts[2];
            ret.TargetLocation = "Primary".Equals(parts[3]) ? StorageLocation.Primary : StorageLocation.Secondary;
            return ret;
        }
    }
}
