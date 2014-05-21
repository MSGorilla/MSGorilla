using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MSGorilla.Library
{
    public class Utils
    {
        public static string MD5Encoding(string rawPass)
        {
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
            return timestamp.ToUniversalTime().ToString("yyyyMMddHHmmss");
        }

        public static string ToAzureStorageDayBasedString(DateTime timestamp)
        {
            return timestamp.ToUniversalTime().ToString("yyyyMMdd");
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


        //todo ID checker
        public static bool IsValidID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            for(int i = 0; i < id.Length; i++){
                if (!char.IsLetterOrDigit(id, i))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
