using MSGorilla.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Client
{
    public class GorillaWebAPI
    {
        private string _userName;
        private string _password;
        private string _authHeader;
        public GorillaWebAPI(string username, string password)
        {
            _userName = username;
            _password = password;
            _generateAuthHeader();
        }
        static string _rootUri = Constant.UriRoot;

        private void _generateAuthHeader()
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(_password));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                string authInfo = _userName + ":" + sBuilder.ToString();
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                _authHeader = "Basic " + authInfo;
            }
        }
        private string _readResponseContent(HttpWebResponse response)
        {
            Stream receiveStream = response.GetResponseStream();
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream);
            Char[] read = new Char[1024];
            // Reads 256 characters at a time.     
            int count = readStream.Read(read, 0, 1024);
            StringBuilder sb = new StringBuilder();
            while (count > 0)
            {
                // Dumps the 256 characters on a string and displays the string to the console.
                String str = new String(read, 0, count);
                sb.Append(str);
                count = readStream.Read(read, 0, 1024);
            }
            // Releases the resources of the response.
            response.Close();
            // Releases the resources of the Stream.
            readStream.Close();
            return sb.ToString();
        }

        public string PostMessage(string message, string schemaID = "none", string eventID = "none", string[] topicName = null, string[] owner = null, string[] atUser = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + Constant.UriPostMessage);
            request.Method = "POST";
            request.Headers["Authorization"] = _authHeader;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                var ownerStr = "";
                var atUserStr = "";
                var topicNameStr = "";
                if (owner != null)
                {
                    ownerStr = "&owner=" + string.Join("&owner=", owner); 
                }
                if (atUser != null)
                {
                    atUserStr = "&atUser=" + string.Join("&atUser=", atUser) ;
                }
                if (topicName != null)
                {
                    topicNameStr = "&topicName=" + string.Join("&topicName=", topicName);
                }

                writer.Write(string.Format("Message={0}&SchemaID={1}&EventID={2}{3}{4}{5}", Uri.EscapeDataString(message), schemaID, eventID, topicNameStr, ownerStr, atUserStr));
            }
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return _readResponseContent(response);
        }

        public List<Message> HomeLine(DateTime start, DateTime end)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriHomeLine, start, end));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return JsonConvert.DeserializeObject<List<Message>>(_readResponseContent(response));            
        }

        public List<Message> EventLine(string eventID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriEventLine, eventID));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return JsonConvert.DeserializeObject<List<Message>>(_readResponseContent(response));
        }

        public MessageDetail GetMessage(string userid, string messageID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriGetMessage, userid, messageID));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return JsonConvert.DeserializeObject<MessageDetail>(_readResponseContent(response));
        }

        public string PostReply(string toUserId, string message, string originMessageUserId, string originMessageID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + Constant.UriPostReply);
            request.Method = "POST";
            request.Headers["Authorization"] = _authHeader;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(string.Format("To={0}&Message={1}&MessageID={2}&MessageUser", toUserId, Uri.EscapeDataString(message), originMessageID, originMessageID));
            }
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return _readResponseContent(response);
        }

    }
}
