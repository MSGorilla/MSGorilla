using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.WebApi
{
    public class MessageController : BaseController
    {
        MessageManager _messageManager = new MessageManager();
        NotifManager _notifManager = new NotifManager();
        TopicManager _topicManager = new TopicManager();
        RichMsgManager _richMsgManager = new RichMsgManager();

        /// <summary>
        /// Return the messages in the current user's userline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY3Ng--;1!72!MjUyMDAwODg2ODUxNjg4XzhjN2Y5ZmVjLTgyMTEtNGIzYi1iZTI5LTAyNjRiMTNmYjUxOQ--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user2",
        ///                 "DisplayName": "User22",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user22"
        ///             },
        ///             "ID": "251998713203119_c6db3598-d234-45b8-9bc9-c29f805f6be7",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": null,
        ///             "TopicName": null,
        ///             "MessageContent": "Is every thing good?",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-24T05:26:36.8802633Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user2",
        ///                 "DisplayName": "User22",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user22"
        ///             },
        ///             "ID": "251999741397833_43f40cf4-3509-46a5-8f31-8f96ef1305d4",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": null,
        ///             "TopicName": null,
        ///             "MessageContent": "@user10 welcome",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-12T07:50:02.1662774Z"
        ///         },
        ///			......
        ///     ]
        /// }
        /// </summary>
        /// <param name="count">count of message in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(int count = 25, string token = null)
        {
            return UserLine(whoami(), count, token);
        }

        /// <summary>
        /// Return the messages in a user's userline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY3Ng--;1!72!MjUyMDAwODg2ODUxNjg4XzhjN2Y5ZmVjLTgyMTEtNGIzYi1iZTI5LTAyNjRiMTNmYjUxOQ--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user2",
        ///                 "DisplayName": "User22",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user22"
        ///             },
        ///             "ID": "251998713203119_c6db3598-d234-45b8-9bc9-c29f805f6be7",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": null,
        ///             "TopicName": null,
        ///             "MessageContent": "Is every thing good?",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-24T05:26:36.8802633Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user2",
        ///                 "DisplayName": "User22",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user22"
        ///             },
        ///             "ID": "251999741397833_43f40cf4-3509-46a5-8f31-8f96ef1305d4",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": null,
        ///             "TopicName": null,
        ///             "MessageContent": "@user10 welcome",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-12T07:50:02.1662774Z"
        ///         },
        ///			......
        ///     ]
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.UserLine(userid, count, tok));
        }

        /// <summary>
        /// Deprecated. Return the messages in a user's userline in a list
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998797027974_478a39bc-6671-4381-8648-b70143eba83d",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": null,
        ///         "TopicName": null,
        ///         "MessageContent": "smoke test",
        ///         "PostTime": "2014-06-23T06:09:32.0256428Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998806386312_4a37b9f0-a123-4c38-af0f-e6e69fae4e19",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": [
        ///             "user1"
        ///         ],
        ///         "TopicName": [],
        ///         "MessageContent": "@user1",
        ///         "PostTime": "2014-06-23T03:33:33.6874261Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///		......
        /// ]
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="start">start timestamp</param>
        /// <param name="end">end timestamp</param>
        /// <returns></returns>
        [HttpGet]
        public List<Message> UserLine(string userid, DateTime start, DateTime end)
        {
            string me = whoami();
            return _messageManager.UserLine(userid, start, end);
        }

        /// <summary>
        /// Return the messages in the current user's homeline in a list
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998797027974_478a39bc-6671-4381-8648-b70143eba83d",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": null,
        ///         "TopicName": null,
        ///         "MessageContent": "smoke test",
        ///         "PostTime": "2014-06-23T06:09:32.0256428Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998806386312_4a37b9f0-a123-4c38-af0f-e6e69fae4e19",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": [
        ///             "user1"
        ///         ],
        ///         "TopicName": [],
        ///         "MessageContent": "@user1",
        ///         "PostTime": "2014-06-23T03:33:33.6874261Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///		......
        /// ]
        /// </summary>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(int count = 25, string token = null, string filter = "")
        {
            DateTime start, end;
            end = DateTime.UtcNow;

            switch (filter)
            {
                case "latest24hours":
                    start = end.AddDays(-1);
                    break;
                case "latest7days":
                    start = end.AddDays(-7);
                    break;
                case "latest1month":
                    start = end.AddMonths(-1);
                    break;
                case "":
                case "all":
                default:
                    return HomeLine(whoami(), count, token);
            }

            return HomeLine(whoami(), start, end, count, token);
        }

        /// <summary>
        /// Return the messages in a user's userline in a list
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998797027974_478a39bc-6671-4381-8648-b70143eba83d",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": null,
        ///         "TopicName": null,
        ///         "MessageContent": "smoke test",
        ///         "PostTime": "2014-06-23T06:09:32.0256428Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998806386312_4a37b9f0-a123-4c38-af0f-e6e69fae4e19",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": [
        ///             "user1"
        ///         ],
        ///         "TopicName": [],
        ///         "MessageContent": "@user1",
        ///         "PostTime": "2014-06-23T03:33:33.6874261Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///		......
        /// ]
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);

            if (me.Equals(userid) && token == null)
            {
                _notifManager.clearHomelineNotifCount(me);
            }
            return new DisplayMessagePagination(_messageManager.HomeLine(userid, count, tok));
        }

        /// <summary>
        /// Deprecated. Return the messages in a user's homeline in a list
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998797027974_478a39bc-6671-4381-8648-b70143eba83d",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": null,
        ///         "TopicName": null,
        ///         "MessageContent": "smoke test",
        ///         "PostTime": "2014-06-23T06:09:32.0256428Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///     {
        ///         "User": "user1",
        ///         "ID": "251998806386312_4a37b9f0-a123-4c38-af0f-e6e69fae4e19",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": [
        ///             "user1"
        ///         ],
        ///         "TopicName": [],
        ///         "MessageContent": "@user1",
        ///         "PostTime": "2014-06-23T03:33:33.6874261Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///		......
        /// ]
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="end">end timestamp</param>
        /// <param name="start">start timestamp</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(string userid, DateTime start, DateTime end, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);

            if (me.Equals(userid))
            {
                _notifManager.clearHomelineNotifCount(me);
            }
            return new DisplayMessagePagination(_messageManager.HomeLine(userid, start, end, count, tok));
        }

        /// <summary>
        /// Return the messages in the current user's ownerline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY3MA--;1!72!MjUyMDAwMzY2OTk3NjQzXzViNGVkNzNjLTJiMTMtNDE1Ni04ODBhLTgwZmNhZTk0MzEzMA--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999678181833_c8864f48-5238-4cd6-bc46-82d20c6da044",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user2"
        ///             ],
        ///             "AtUser": [
        ///                 "user3"
        ///             ],
        ///             "TopicName": [
        ///                 "world"
        ///             ],
        ///             "MessageContent": "#worldcup# Brazil won the first match @user3",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-13T01:23:38.1661805Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "252000366997643_5b4ed73c-2b13-4156-880a-80fcae943130",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user2"
        ///             ],
        ///             "AtUser": [],
        ///             "TopicName": null,
        ///             "MessageContent": "something owned by user2",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-05T02:03:22.3562092Z"
        ///         },
        ///         ......
        ///     ]
        /// }
        /// </summary>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(int count = 25, string token = null)
        {
            return OwnerLine(whoami(), count, token);
        }

        /// <summary>
        /// Return the messages in a user's ownerline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY3MA--;1!72!MjUyMDAwMzY2OTk3NjQzXzViNGVkNzNjLTJiMTMtNDE1Ni04ODBhLTgwZmNhZTk0MzEzMA--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999678181833_c8864f48-5238-4cd6-bc46-82d20c6da044",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user2"
        ///             ],
        ///             "AtUser": [
        ///                 "user3"
        ///             ],
        ///             "TopicName": [
        ///                 "world"
        ///             ],
        ///             "MessageContent": "#worldcup# Brazil won the first match @user3",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-13T01:23:38.1661805Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "252000366997643_5b4ed73c-2b13-4156-880a-80fcae943130",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user2"
        ///             ],
        ///             "AtUser": [],
        ///             "TopicName": null,
        ///             "MessageContent": "something owned by user2",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-05T02:03:22.3562092Z"
        ///         },
        ///         ......
        ///     ]
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);

            if (me.Equals(userid) && token == null)
            {
                _notifManager.clearOwnerlineNotifCount(me);
            }
            return new DisplayMessagePagination(_messageManager.OwnerLine(userid, count, tok));
        }

        /// <summary>
        /// Deprecated. Return the messages in a user's owner in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY3MA--;1!72!MjUyMDAwMzY2OTk3NjQzXzViNGVkNzNjLTJiMTMtNDE1Ni04ODBhLTgwZmNhZTk0MzEzMA--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999678181833_c8864f48-5238-4cd6-bc46-82d20c6da044",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user2"
        ///             ],
        ///             "AtUser": [
        ///                 "user3"
        ///             ],
        ///             "TopicName": [
        ///                 "world"
        ///             ],
        ///             "MessageContent": "#worldcup# Brazil won the first match @user3",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-13T01:23:38.1661805Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "252000366997643_5b4ed73c-2b13-4156-880a-80fcae943130",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user2"
        ///             ],
        ///             "AtUser": [],
        ///             "TopicName": null,
        ///             "MessageContent": "something owned by user2",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-05T02:03:22.3562092Z"
        ///         },
        ///         ......
        ///     ]
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="end">end time</param>
        /// <param name="start">start time</param>
        /// <returns></returns>
        [HttpGet]
        public List<Message> OwnerLine(string userid, DateTime end, DateTime start)
        {
            whoami();
            return _messageManager.OwnerLine(whoami(), start, end);
        }

        /// <summary>
        /// Return the messages in the current user's atline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY2Mg--;1!72!MjUxOTk5Njc3ODE5NTk0XzEyMTI3ODViLWEyZWUtNDMyMi05ZDA1LWYyNWQ1MDIyZmI4Zg--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999326132390_8b076705-aa69-4522-b389-3d184cfdcfdd",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": [
        ///                 "user2"
        ///             ],
        ///             "TopicName": [],
        ///             "MessageContent": "test@user2",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-17T03:11:07.6098089Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999677173707_aac7d956-076e-461a-bb0e-54c61e0e3876",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user1"
        ///             ],
        ///             "AtUser": [
        ///                 "user2"
        ///             ],
        ///             "TopicName": [
        ///                 "111",
        ///                 "test2"
        ///             ],
        ///             "MessageContent": "Multi topic test",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-13T01:40:26.2923506Z"
        ///         }
        ///     ]
        /// }
        /// </summary>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(int count = 25, string token = null)
        {
            return AtLine(whoami(), count, token);
        }

        /// <summary>
        /// Return the messages in a user's atline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY2Mg--;1!72!MjUxOTk5Njc3ODE5NTk0XzEyMTI3ODViLWEyZWUtNDMyMi05ZDA1LWYyNWQ1MDIyZmI4Zg--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999326132390_8b076705-aa69-4522-b389-3d184cfdcfdd",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": [
        ///                 "user2"
        ///             ],
        ///             "TopicName": [],
        ///             "MessageContent": "test@user2",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-17T03:11:07.6098089Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999677173707_aac7d956-076e-461a-bb0e-54c61e0e3876",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "user1"
        ///             ],
        ///             "AtUser": [
        ///                 "user2"
        ///             ],
        ///             "TopicName": [
        ///                 "111",
        ///                 "test2"
        ///             ],
        ///             "MessageContent": "Multi topic test",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-13T01:40:26.2923506Z"
        ///         }
        ///     ]
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = whoami();
            }

            TableContinuationToken tok = Utils.String2Token(token);

            if (me.Equals(userid) && token == null)
            {
                _notifManager.clearAtlineNotifCount(me);
            }

            return new DisplayMessagePagination(_messageManager.AtLine(userid, count, tok));
        }

        /// <summary>
        /// Deprecated. Return all messages do not have a eventid in a list.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMessage> EventLine()
        {
            whoami();
            return EventLine("none");
        }

        /// <summary>
        /// Deprecated. Return all messages have the same eventid in a list
        /// </summary>
        /// <param name="eventID">event id</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMessage> EventLine(string eventID)
        {
            whoami();
            var msglist = _messageManager.EventLine(eventID);
            var msg = new List<DisplayMessage>();
            AccountManager accManager = new AccountManager();
            AttachmentManager attManage = new AttachmentManager();
            foreach (var m in msglist)
            {
                msg.Add(new DisplayMessage(m, accManager, attManage));
            }

            return msg;
        }

        /// <summary>
        /// Deprecated. Return all messages posted in a certain time
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "User": "user2",
        ///         "ID": "251998713203119_c6db3598-d234-45b8-9bc9-c29f805f6be7",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": null,
        ///         "TopicName": null,
        ///         "MessageContent": "Is every thing good?",
        ///         "PostTime": "2014-06-24T05:26:36.8802633Z",
        ///         "RichMessageID": null,
        ///         "AttachmentID": null
        ///     },
        ///     {
        ///         "User": "WossWAESMonitor",
        ///         "ID": "251998713939135_355a132e-0594-4fb2-ab27-b09e6f1a09f7",
        ///         "EventID": "1-955a4ba5-56db-40f8-9863-3b17896aed42-jusjin",
        ///         "SchemaID": "none",
        ///         "Owner": [
        ///             "jusjin"
        ///         ],
        ///         "AtUser": [
        ///             "jusjin"
        ///         ],
        ///         "TopicName": [
        ///             "WAES Job 1-955a4ba5-56db-40f8-9863-3b17896aed42-jusjin",
        ///             "WOSS Change 1170604",
        ///             "WOSS WAES Job"
        ///         ],
        ///         "MessageContent": "PCV build complete \nsubmited by @jusjin \nbuild: Success \nCode Analysis: Not Run \nWAES /// Validation: 100.00% #WAES Job 1-955a4ba5-56db-40f8-9863-3b17896aed42-jusjin# \nchangeList #WOSS Change 1170604# \nchangeList Associated with: none \n",
        ///         "PostTime": "2014-06-24T05:14:20.8641009Z",
        ///         "RichMessageID": "WossWAESMonitor_2916651;081a7930-bb0d-479b-b958-2ee652294eb0",
        ///         "AttachmentID": null
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <returns></returns>
        [HttpGet]
        public List<Message> PublicSquareLine(DateTime start, DateTime end)
        {
            whoami();
            return _messageManager.PublicSquareLine(start, end);
        }

        /// <summary>
        /// Return a messages list order by post time desc
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!12!MjkxNjY1MQ--;1!72!MjUxOTk4NzA4Njc4Mjg1XzdiMzFhYTM3LWFhMDUtNGU4OS05ZTdmLTI5MDA3Y2M3MDczMQ--;;Primary;",
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "WossWAESMonitor",
        ///                 "DisplayName": "WossWAESMonitor",
        ///                 "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///                 "Description": "WossWAESMonitor"
        ///             },
        ///             "ID": "251998707929673_79d10a16-e68d-495f-8980-4a718721ec44",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": [
        ///                 "lazhang"
        ///             ],
        ///             "AtUser": null,
        ///             "TopicName": [
        ///                 "WOSS Change 1170731",
        ///                 "WOSS WAES Job"
        ///             ],
        ///             "MessageContent": "PCV build complete \nsubmited by @lazhang \nbuild: Fail \nCode Analysis: PASS \nWAES Validation: Build Failed \nchangeList #WOSS Change 1170731# \nchangeList Associated with: none \n",
        ///             "RichMessageID": "WossWAESMonitor_2916651;2486a037-df4d-43e3-ba29-2097bc6df45f",
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-24T06:54:30.3262394Z"
        ///         },
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": null,
        ///             "TopicName": null,
        ///             "MessageContent": "a new cloud message",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-24T06:47:51.0325756Z"
        ///         },
        /// 		......
        ///     ]
        /// }
        /// </summary>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(int count = 25, string token = null)
        {
            whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.PublicSquareLine(count, tok));
        }

        /// <summary>
        /// Return a list of messages having the same topic
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": null,
        ///     "message": [
        ///         {
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251999057446274_93dc9472-7321-48d0-901d-7ec7443009d9",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": [],
        ///             "TopicName": [
        ///                 "WAES Job 1-bbcf06e2-adb8-4b36-af9b-7f0d54e0467a-bvt"
        ///             ],
        ///             "MessageContent": "#WAES Job 1-bbcf06e2-adb8-4b36-af9b-7f0d54e0467a-bvt#",
        ///             "RichMessageID": "user1_2916655;75383ec9-1fb3-4b7f-bc8f-4a947ce26aec",
        ///             "Attachment": null,
        ///             "PostTime": "2014-06-20T05:49:13.7251136Z"
        ///         },
        ///         ......
        ///     ]
        /// }
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination TopicLine(string topic, int count = 25, string token = null)
        {
            string me = whoami();
            var t = _topicManager.FindTopicByName(topic);
            if (t == null)
            {
                return null;
            }

            TableContinuationToken tok = Utils.String2Token(token);
            if (tok == null)
            {
                _topicManager.clearUnreadMsgCountOfFavouriteTopic(me, t.Id);
            }
            return new DisplayMessagePagination(_messageManager.TopicLine(t.Id.ToString(), count, tok));
        }

        /// <summary>
        /// Deprecated. Return the detail of a Message
        /// 
        /// Example output:
        /// {
        ///     "ReplyCount": 3,
        ///     "Replies": [
        ///         {
        ///             "FromUser": "user2",
        ///             "ToUser": "user1",
        ///             "Message": "test cloud reply",
        ///             "PostTime": "2014-06-24T06:51:02.9789122Z",
        ///             "MessageUser": "user1",
        ///             "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///             "ReplyID": "251998708137021_431cab73-f6d7-4484-8872-797ec183ec68"
        ///         },
        ///         {
        ///             "FromUser": "user2",
        ///             "ToUser": "user1",
        ///             "Message": "greate",
        ///             "PostTime": "2014-06-24T06:48:26.5283644Z",
        ///             "MessageUser": "user1",
        ///             "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///             "ReplyID": "251998708293471_0e283889-18e2-44d3-a681-c07c35824c19"
        ///         },
        ///         {
        ///             "FromUser": "user2",
        ///             "ToUser": "user1",
        ///             "Message": "hello",
        ///             "PostTime": "2014-06-24T06:48:08.9586062Z",
        ///             "MessageUser": "user1",
        ///             "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///             "ReplyID": "251998708311041_51bfb009-53a2-435f-8559-5bf8e222887b"
        ///         }
        ///     ],
        ///     "User": "user1",
        ///     "ID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///     "EventID": "none",
        ///     "SchemaID": "none",
        ///     "Owner": null,
        ///     "AtUser": null,
        ///     "TopicName": null,
        ///     "MessageContent": "a new cloud message",
        ///     "PostTime": "2014-06-24T06:47:51.0325756Z",
        ///     "RichMessageID" : null,
        ///     "AttachmentID" : null,
        /// }
        /// </summary>
        /// <param name="userid">user id of whom posted the message</param>
        /// <param name="messageID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public MessageDetail GetMessage(string userid, string messageID)
        {
            whoami();
            return _messageManager.GetMessageDetail(userid, messageID);
        }

        /// <summary>
        /// Return the details of a message
        /// 
        /// Example output:
        /// {
        ///     "User": {
        ///         "Userid": "user1",
        ///         "DisplayName": "User1",
        ///         "PortraitUrl": null,
        ///         "Description": "user for test"
        ///     },
        ///     "ID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///     "EventID": "none",
        ///     "SchemaID": "none",
        ///     "Owner": null,
        ///     "AtUser": null,
        ///     "TopicName": null,
        ///     "MessageContent": "a new cloud message",
        ///     "RichMessageID": null,
        ///     "Attachment": null,
        ///     "PostTime": "2014-06-24T06:47:51.0325756Z"
        /// }
        /// </summary>
        /// <param name="msgUser">user id of whom posted the message</param>
        /// <param name="msgID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessage GetDisplayMessage(string msgUser, string msgID)
        {
            whoami();
            return _messageManager.GetDisplayMessage(msgUser, msgID);
        }

        /// <summary>
        /// Return all replies within the message in a list
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "FromUser": {
        ///             "Userid": "user2",
        ///             "DisplayName": "User22",
        ///             "PortraitUrl": null,
        ///             "Description": "user22"
        ///         },
        ///         "ToUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "Message": "test cloud reply",
        ///         "PostTime": "2014-06-24T06:51:02.9789122Z",
        ///         "MessageUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///         "ReplyID": "251998708137021_431cab73-f6d7-4484-8872-797ec183ec68"
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <param name="msgID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayReply> GetMessageReply(string msgID)
        {
            whoami();
            var replylist = _messageManager.GetAllReplies(msgID);
            var reply = new List<DisplayReply>();
            AccountManager accManager = new AccountManager();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager));
            }

            return reply;
        }

        /// <summary>
        /// Return the rich messsage.
        /// 
        /// Example output:
        /// "A rich message"
        /// </summary>
        /// <param name="richMsgID">rich message id</param>
        /// <returns></returns>
        [HttpGet]
        public string GetRichMessage(string richMsgID)
        {
            return _richMsgManager.GetRichMessage(richMsgID);
        }

        /// <summary>
        /// Post a new message
        /// 
        /// Example output:
        /// {
        ///     "User": {
        ///         "Userid": "user2",
        ///         "DisplayName": "User22",
        ///         "PortraitUrl": null,
        ///         "Description": "user22"
        ///     },
        ///     "ID": "251998703315809_64a6332e-7809-46cc-89c1-5d0624db7111",
        ///     "EventID": "none",
        ///     "SchemaID": "none",
        ///     "Owner": [
        ///         "user1"
        ///     ],
        ///     "AtUser": [
        ///         "user2",
        ///         "user4",
        ///         "user3"
        ///     ],
        ///     "TopicName": [
        ///         "test topic"
        ///     ],
        ///     "MessageContent": "@user2 a new posted message",
        ///     "RichMessageID": "user2_2916651;e18437bd-2fe7-427f-94be-fcd4c1c69fd8",
        ///     "Attachment": [
        ///         {
        ///             "AttachmentID": "2916651;251998720140928_7b7b2ad2-dd71-424d-a918-279c832b0440.xml",
        ///             "FileID": "7b7b2ad2-dd71-424d-a918-279c832b0440.xml",
        ///             "Uploader": "user1",
        ///             "UploadTimestamp": "2014-06-24T03:30:59.0715892Z",
        ///             "Filename": "FederationMetadata.xml",
        ///             "Filetype": "text/xml",
        ///             "Filesize": 46403
        ///         }
        ///     ],
        ///     "PostTime": "2014-06-24T08:11:24.1907127Z"
        /// }
        /// </summary>
        /// <param name="message">message content. If a single word of content starts with @ and the suffix is a valid
        /// userid, such as @user1, the userid will be added into atUser list. If a single word starts with and ends with #,
        /// such as #world cup#, it will be recognized as a topic name and be added into topicName list.
        /// </param>
        /// <param name="schemaID">schema id</param>
        /// <param name="eventID">event id</param>
        /// <param name="owner">user id of the owner. Can be a list.</param>
        /// <param name="atUser">user id of related users. Can be a list</param>
        /// <param name="topicName">topic name of related topic. Can be a list</param>
        /// <param name="richMessage">rich message. Up to 992 kb</param>
        /// <param name="attachmentID">attachment id related. Can be a list</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayMessage PostMessage(string message,
                                    string schemaID = "none",
                                    string eventID = "none",
                                    [FromUri]string[] owner = null,
                                    [FromUri]string[] atUser = null,
                                    [FromUri]string[] topicName = null,
                                    string richMessage = null,
                                    [FromUri]string[] attachmentID = null)
        {
            return new DisplayMessage(_messageManager.PostMessage(whoami(), eventID, schemaID, owner, atUser, topicName, message, richMessage, attachmentID, DateTime.UtcNow), new AccountManager(), new AttachmentManager());
            //return new ActionResult();
        }

        /// <summary>
        /// Post a new message. Same as the Get Api.
        /// </summary>
        public class MessageModel
        {
            public string Message { get; set; }
            public string SchemaID { get; set; }
            public string EventID { get; set; }
            public string[] TopicName { get; set; }
            public string[] Owner { get; set; }
            public string[] AtUser { get; set; }
            public string RichMessage { get; set; }
            public string[] AttachmentID { get; set; }
        };

        [HttpPost]
        public DisplayMessage PostMessage(MessageModel msg)
        {
            if (string.IsNullOrEmpty(msg.Message))
            {
                throw new MessageNullException();
            }
            if (string.IsNullOrEmpty(msg.SchemaID))
            {
                msg.SchemaID = "none";
            }
            if (string.IsNullOrEmpty(msg.EventID))
            {
                msg.EventID = "none";
            }
            return PostMessage(msg.Message, msg.SchemaID, msg.EventID, msg.Owner, msg.AtUser, msg.TopicName, msg.RichMessage, msg.AttachmentID);
            //return new ActionResult();
        }
    }
}
