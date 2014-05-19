using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MSGorilla.Library.Models.AzureModels
{
    public class QueueMessage
    {
        public const string TypeMessage = "tweet";
        public const string TypeReply = "reply";

        public QueueMessage(string type, string content)
        {
            Content = content;
            Type = type;
        }

        public string Type {get; set;}

        /*
        * Case(Type) tweet
        * Content shoule be a jsonObject looks like:
        *  {
        *      "Type" : "text",
        *      "Content" : "something user want to say"
        *  }
        *  or 
        *  {
        *      "Type" : "picture",
        *      "Url" : "http://*******",
        *      "Comment" : "something to say"
        *  }
        * 
         * Case(Type) retweet
         * {
         *      "Userid": "",
         *      "MessageID": ""
         * }
        * 
         * Case(Type) reply
         * {
         *      "From" : "some user id"
         *      "To" : "some userid",
         *      "Content" : "something to say",
         *      "Tweet":{
         *          "Userid": "",
         *          "MessageID": ""
         *      }
         * }
        *
        */
        public string Content { get; set; } //should be 

        public CloudQueueMessage toAzureCloudQueueMessage()
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(this));
        }
    }
}
