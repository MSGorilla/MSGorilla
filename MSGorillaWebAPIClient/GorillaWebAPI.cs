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

using MSGorilla.Library.Models.ViewModels;

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

        HttpWebResponse GetResponseFromMSGorilla(HttpWebRequest request)
        {
            try
            {
                return request.GetResponse() as HttpWebResponse;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    try
                    {
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            MSGorillaException gorillaException = new MSGorillaException(text, e);
                            throw gorillaException;
                        }
                    }
                    catch(Exception exp)
                    {
                        //If the exception can be recognized as gorilla exception,
                        //throw the MSGorillaException.
                        //Else throw the origin exception.
                        if (exp is MSGorillaException)
                        {
                            throw exp;
                        }
                        else
                        {
                            throw e;
                        }
                    }

                }
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

        public string PostMessage(string message, string groupID = null, string schemaID = "none", string eventID = "none", string[] topicName = null, string[] owner = null, string[] atUser = null, string richMessage = null, string[] attachmentID = null, int importance = 2)
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
                var richMessageStr = "";
                var attachmentStr = "";
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
                if (richMessage != null)
                {
                    richMessageStr = "&richMessage=" + EscapeString(richMessage);
                }
                if (attachmentID != null)
                {
                    attachmentStr = "&attachmentID=" + string.Join("&attachmentID=", attachmentID);
                }

                string msg = string.Format("Message={0}&group={1}&SchemaID={2}&EventID={3}{4}{5}{6}{7}{8}&importance={9}", 
                                            Uri.EscapeDataString(message),
                                            groupID,
                                            Uri.EscapeDataString(schemaID), 
                                            Uri.EscapeDataString(eventID), 
                                            topicNameStr, 
                                            ownerStr, 
                                            atUserStr, 
                                            richMessageStr, 
                                            attachmentStr, 
                                            importance);
                writer.Write(msg);
            }
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return _readResponseContent(response);
        }

        public string UpdateCategoryMessage(string[] eventIDs, string notifyTo, string categoryName, string groupID = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + Constant.UriUpdateCategoryMessage);
            request.Method = "POST";
            request.Headers["Authorization"] = _authHeader;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                string idString = "eventid=" + string.Join("&eventid=", eventIDs); 
                string msg = string.Format("{0}&to={1}&categoryName={2}&group={3}",
                                                idString,
                                                notifyTo,
                                                categoryName,
                                                groupID);
                writer.Write(msg);
            }

            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return _readResponseContent(response);
        }

        private string EscapeString(string longStr)
        {
            int limit = 2000;
            if (!string.IsNullOrEmpty(longStr))
            {
                if (longStr.Length > limit)
                {
                    StringBuilder sb = new StringBuilder();
                    int loops = longStr.Length / limit;

                    for (int i = 0; i <= loops; i++)
                    {
                        if (i < loops)
                        {
                            sb.Append(Uri.EscapeDataString(longStr.Substring(limit * i, limit)));
                        }
                        else
                        {
                            sb.Append(Uri.EscapeDataString(longStr.Substring(limit * i)));
                        }
                    }

                    return sb.ToString();
                }
            }
            return Uri.EscapeDataString(longStr);
        }

        public DisplayMessagePagination HomeLine(string userid, string group = null, int count = 25, string token = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriHomeLine, userid, group, count, token));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMessagePagination>(_readResponseContent(response));            
        }

        public DisplayMessagePagination UserLine(string userid, string group = null, int count = 25, string token = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriUserLine, userid, group, count, token));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMessagePagination>(_readResponseContent(response));
        }

        public DisplayMessagePagination OwnerLine(string userid, int count = 25, string token = "", bool keepUnread = false)
        {
            string reqUri = _rootUri + string.Format(Constant.UriOwnerLine, userid, count, token, keepUnread ? "true" : "false");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqUri);
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMessagePagination>(_readResponseContent(response));
        }

        public DisplayMessagePagination AtLine(string userid, int count = 25, string token = "", bool keepUnread = false)
        {
            string reqUri = _rootUri + string.Format(Constant.UriAtLine, userid, count, token, keepUnread ? "true" : "false");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqUri);
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMessagePagination>(_readResponseContent(response));
        }

        public DisplayMessagePagination TopicLine(string topic, int count = 25, string group = "", string token = "")
        {
            string reqUri = _rootUri + string.Format(Constant.UriTopicLine, topic, count, group, token);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqUri);
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMessagePagination>(_readResponseContent(response));
        }

        public List<Message> EventLine(string eventID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriEventLine, eventID));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<List<Message>>(_readResponseContent(response));
        }

        public string GetRichMessage(string richMessageID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                _rootUri + string.Format(Constant.UriRichMessage, richMessageID)
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return _readResponseContent(response);
        }

        public Message GetRawMessage(string messageID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriGetRawMessage, messageID));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<Message>(_readResponseContent(response));
        }

        public DisplayReplyPagination GetReply(int count = 25, string token = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriGetMyReply, count, token));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayReplyPagination>(_readResponseContent(response));
        }

        public string PostReply(string[] toUser, string message, string originMessageID, string richMessage = null, string[] attachmentID = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + Constant.UriPostReply);
            request.Method = "POST";
            request.Headers["Authorization"] = _authHeader;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                string toUserStr = "";
                string attachmentIdStr = "";

                if (toUser != null && toUser.Count() > 0)
                {
                    toUserStr = string.Join("&To=", toUser); 
                }
                if (attachmentID != null && attachmentID.Count() > 0)
                {
                    attachmentIdStr = "&attachmentID=" + string.Join("&attachmentID=", attachmentID);
                }

                string post = string.Format("To={0}&Message={1}&MessageID={2}&richMessage={4}{5}{6}",
                    toUserStr,
                    Uri.EscapeDataString(message),
                    originMessageID,
                    Uri.EscapeDataString(richMessage),
                    toUserStr,
                    attachmentIdStr);
                writer.Write(post);
                //writer.Write(string.Format("To={0}&Message={1}&MessageID={2}&MessageUser", toUserId, Uri.EscapeDataString(message), originMessageID, originMessageID));
            }
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return _readResponseContent(response);
        }

        public Attachment UploadAttachment(string filePath)
        {
            string url = _rootUri + Constant.UriUploadAttachment;

            WebClient myWebClient = new WebClient();
            myWebClient.Headers.Add("Authorization:"+_authHeader);
            Console.WriteLine("Uploading {0} to {1} ...", filePath, url);

            // Upload the file to the URI. 
            // The 'UploadFile(uriString,fileName)' method implicitly uses HTTP POST method. 
            byte[] responseArray = myWebClient.UploadFile(url, filePath);
            string ret = System.Text.Encoding.ASCII.GetString(responseArray);

            return JsonConvert.DeserializeObject<List<Attachment>>(ret)[0];
        }

        public void DownloadAttachment(string attachmentID, string filePath)
        {
            string url = string.Format(_rootUri + Constant.UriDownloadAttachment, attachmentID);
            WebClient myWebClient = new WebClient();
            myWebClient.Headers.Add("Authorization:" + _authHeader);

            using (var stream = myWebClient.OpenRead(url))
            {
                using (var fileStream = File.Create(filePath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        public NotificationCount GetNotificationCount(string userid = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + string.Format(Constant.UriNotificationCount, userid));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<NotificationCount>(_readResponseContent(response));
        }

        public DisplayMetricDataSet CreateMetricDataset(string instance, string counter, string category, string group = null, string description = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri + 
                string.Format(Constant.UriCreateMetricDataset, instance, counter, category, group, description)
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMetricDataSet>(_readResponseContent(response));
        }

        public string InsertMetricRecord(int datasetID, string key, double value)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri +
                string.Format(Constant.UriInserRecord, datasetID, key, value)
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return _readResponseContent(response);
        }

        public string InsertMetricRecord(DisplayMetricDataSet dataset, string key, double value)
        {
            return InsertMetricRecord(dataset.Id, key, value);
        }

        public DisplayMetricDataSet QueryMetricDataSet(string instance, string counter, string category, string group = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri +
                string.Format(Constant.UriQueryMetricDataset, 
                    Uri.EscapeDataString(instance), 
                    Uri.EscapeDataString(counter), 
                    Uri.EscapeDataString(category), 
                    group)
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<DisplayMetricDataSet>(_readResponseContent(response));
        }

        public List<DisplayMembership> GetJoinedGroup()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri +
                Constant.UrlGetJoinedGroup
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<List<DisplayMembership>>(_readResponseContent(response));
        }

        public List<DisplayUserProfile> GetFollowings()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri +
                Constant.UriFollowings
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<List<DisplayUserProfile>>(_readResponseContent(response));
        }

        public List<DisplayFavouriteTopic> GetMyFavouriteTopic()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri +
                Constant.UriGetMyFavouriteTopic
                );
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            return JsonConvert.DeserializeObject<List<DisplayFavouriteTopic>>(_readResponseContent(response));
        }

        public bool IsFavouriteTopic(string topicName, string groupID = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_rootUri +
                string.Format(Constant.UriIsFavouriteTopic, topicName, groupID));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            string ret = _readResponseContent(response);
            return "true".Equals(ret);
        }

        public void AddFavouriteTopic(string topicName, string groupID = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                _rootUri + string.Format(Constant.UriAddFavouriteTopic, topicName, groupID));
            request.Headers["Authorization"] = _authHeader;
            HttpWebResponse response = GetResponseFromMSGorilla(request);
            string ret = _readResponseContent(response);
        }
    }
}
