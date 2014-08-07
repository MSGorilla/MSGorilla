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
using MSGorilla.Utility;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;


namespace MSGorilla.WebAPI
{
    public class MessageController : BaseController
    {
        MessageManager _messageManager = new MessageManager();
        NotifManager _notifManager = new NotifManager();
        TopicManager _topicManager = new TopicManager();
        RichMsgManager _richMsgManager = new RichMsgManager();
        AccountManager _accManager = new AccountManager();
        AttachmentManager _attManager = new AttachmentManager();
        SearchManager _searchManager = new SearchManager();

        private DisplayMessagePagination CreateDisplayMsgPag(MessagePagination msgPag)
        {
            DisplayMessagePagination dmp = new DisplayMessagePagination();
            dmp.continuationToken = msgPag.continuationToken;
            List<DisplayMessage> msgs = new List<DisplayMessage>();
            dmp.message = msgs;

            foreach (Message msg in msgPag.message)
            {
                DisplayMessage dmsg = new DisplayMessage(msg, _attManager, null);
                dmsg.User = AccountController.GetSimpleUserProfile(msg.User);
                msgs.Add(dmsg);
            }
            return dmp;
        }


        /// <summary>
        /// Return the messages in the current user's userline in a list
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!20!dXNlcjJfMjkxNjY3Ng--;1!72!MjUyMDAwODg2ODUxNjg4XzhjN2Y5ZmVjLTgyMTEtNGIzYi1iZTI5LTAyNjRiMTNmYjUxOQ--;;Primary;",
        ///     "message": [
        ///         {
        /// 		    "User": {
        /// 		        "Userid": "user2",
        /// 		        "DisplayName": "User22",
        /// 		        "PortraitUrl": null,
        /// 		        "Description": "user22"
        /// 		    },
        /// 		    "ID": "251998703315809_microsoft_user2_64a6332e-7809-46cc-89c1-5d0624db7111",
        /// 			"Group": "microsoft",
        /// 		    "EventID": "none",
        /// 		    "SchemaID": "none",
        /// 		    "Owner": [
        /// 		        "user1"
        /// 		    ],
        /// 		    "AtUser": [
        /// 		        "user2",
        /// 		        "user4",
        /// 		        "user3"
        /// 		    ],
        /// 		    "TopicName": [
        /// 		        "test topic"
        /// 		    ],
        /// 		    "MessageContent": "@user2 a new posted message",
        /// 		    "RichMessageID": "user2_2916651;e18437bd-2fe7-427f-94be-fcd4c1c69fd8",
        /// 		    "Attachment": [
        /// 		        {
        /// 		            "AttachmentID": "2916651;251998720140928_7b7b2ad2-dd71-424d-a918-279c832b0440.xml",
        /// 		            "FileID": "7b7b2ad2-dd71-424d-a918-279c832b0440.xml",
        /// 		            "Uploader": "user1",
        /// 		            "UploadTimestamp": "2014-06-24T03:30:59.0715892Z",
        /// 		            "Filename": "FederationMetadata.xml",
        /// 		            "Filetype": "text/xml",
        /// 		            "Filesize": 46403
        /// 		        }
        /// 		    ],
        /// 		    "PostTime": "2014-06-24T08:11:24.1907127Z",
        /// 		    "Importance": 2
        /// 		},
        ///			......
        ///     ]
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="group">group id</param>
        /// <param name="count">count of message in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(string filter, string userid, string group = null, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return UserLine(userid, group, start, end, count, token);
            }
            else
            {
                return UserLine(userid, group, count, token);
            }
        }

        /// <summary>
        /// Return the messages in a user's userline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="group">group id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(string userid, string group = null, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(userid);
            }
            MembershipHelper.CheckMembership(group, me);

            TableContinuationToken tok = Utils.String2Token(token);
            return CreateDisplayMsgPag(_messageManager.UserLine(userid, group, count, tok));
        }

        /// <summary>
        /// Deprecated. Return the messages in a user's userline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="group">group id</param>
        /// <param name="start">start timestamp</param>
        /// <param name="start">end timestamp</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(string userid, string group, DateTime start, DateTime end, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(userid);
            }
            MembershipHelper.CheckMembership(group, me);

            TableContinuationToken tok = Utils.String2Token(token);
            return CreateDisplayMsgPag(_messageManager.UserLine(userid, group, start, end, count, tok));
        }

        /// <summary>
        /// Return the messages in the current user's homeline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="group">group id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(string filter, string userid, string group = null, int count = 25, string token = null, bool keepUnread = false)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return HomeLine(userid, group, start, end, count, token, keepUnread);
            }
            else
            {
                return HomeLine(userid, group, count, token, keepUnread);
            }
        }

        /// <summary>
        /// Return the messages in a user's userline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="group">group id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(string userid, string group = null, int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(userid);
            }
            TableContinuationToken tok = Utils.String2Token(token);
            MembershipHelper.CheckMembership(group, me);

            if (!keepUnread && me.Equals(userid) && token == null)
            {
                _notifManager.clearHomelineNotifCount(me);
                _notifManager.clearImportantMsgCount(me);
            }
            return CreateDisplayMsgPag(_messageManager.HomeLine(userid, group, count, tok));
        }

        /// <summary>
        ///  Return the messages in a user's homeline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="group">group id</param>
        /// <param name="start">start timestamp</param>
        /// <param name="end">end timestamp</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(string userid, string group, DateTime start, DateTime end, int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(userid);
            }
            MembershipHelper.CheckMembership(group, me);

            TableContinuationToken tok = Utils.String2Token(token);

            if (!keepUnread && me.Equals(userid) && token == null)
            {
                _notifManager.clearHomelineNotifCount(me);
                _notifManager.clearImportantMsgCount(me);
            }
            return CreateDisplayMsgPag(_messageManager.HomeLine(userid, group, start, end, count, tok));
        }

        /// <summary>
        /// Return the messages in the current user's ownerline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string filter, string userid, int count = 25, string token = null, bool keepUnread = false)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return OwnerLine(userid, start, end, count, token, keepUnread);
            }
            else
            {
                return OwnerLine(userid, count, token, keepUnread);
            }
        }

        /// <summary>
        /// Return the messages in a user's ownerline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string userid, int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);

            if (!keepUnread && me.Equals(userid) && token == null)
            {
                _notifManager.clearOwnerlineNotifCount(me);
            }
            return CreateDisplayMsgPag(_messageManager.OwnerLine(userid, count, tok));
        }

        /// <summary>
        /// Deprecated. Return the messages in a user's owner in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string userid, DateTime start, DateTime end, int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);

            if (!keepUnread && me.Equals(userid) && token == null)
            {
                _notifManager.clearOwnerlineNotifCount(me);
            }
            return CreateDisplayMsgPag(_messageManager.OwnerLine(userid, start, end, count, tok));
        }

        /// <summary>
        /// Return the messages in the current user's atline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(string filter, string userid, int count = 25, string token = null, bool keepUnread = false)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return AtLine(userid, start, end, count, token, keepUnread);
            }
            else
            {
                return AtLine(userid, count, token, keepUnread);
            }
        }

        /// <summary>
        /// Return the messages in a user's atline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="keepUnread">if false, gorilla will clear unreadhomelinemessage count</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(string userid, int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = whoami();
            }

            TableContinuationToken tok = Utils.String2Token(token);

            if (!keepUnread && me.Equals(userid) && token == null)
            {
                _notifManager.clearAtlineNotifCount(me);
            }

            return CreateDisplayMsgPag(_messageManager.AtLine(userid, count, tok));
        }

        /// <summary>
        /// Return the messages in a user's atline in a list
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(string userid, DateTime start, DateTime end, int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = whoami();
            }

            TableContinuationToken tok = Utils.String2Token(token);

            if (!keepUnread && me.Equals(userid) && token == null)
            {
                _notifManager.clearAtlineNotifCount(me);
            }

            return CreateDisplayMsgPag(_messageManager.AtLine(userid, start, end, count, tok));
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
                msg.Add(new DisplayMessage(m, attManage, accManager));
            }

            return msg;
        }

        /// <summary>
        /// Return a messages list order by post time desc
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="count">count of messages in the list</param>
        /// <param name="group">group id</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(string group, string filter, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return PublicSquareLine(group, start, end, count, token);
            }
            else
            {
                return PublicSquareLine(group, count, token);
            }
        }

        /// <summary>
        /// Deprecated. Return all messages posted in a certain time
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="group">group id</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(string group, DateTime start, DateTime end, int count = 25, string token = null)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(group, me);

            TableContinuationToken tok = Utils.String2Token(token);
            return CreateDisplayMsgPag(_messageManager.PublicSquareLine(group, start, end, count, tok));
        }

        /// <summary>
        /// Return a messages list order by post time desc
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="group">group id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(string group = null, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            MembershipHelper.CheckMembership(group, me);
            TableContinuationToken tok = Utils.String2Token(token);
            return CreateDisplayMsgPag(_messageManager.PublicSquareLine(group, count, tok));
        }

        /// <summary>
        /// Return a list of messages having the same topic
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="group">group id, since topic with the same name can be exist in different groups</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination TopicLine(string filter, string topic, int count = 25, [FromUri]string[] group = null, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return TopicLine(topic, start, end, count, group, token);
            }
            else
            {
                return TopicLine(topic, count, group, token);
            }
        }

        /// <summary>
        /// Return a list of messages having the same topic
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="group">group id, since topic with the same name can be exist in different groups</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination TopicLine(string topic, int count = 25, [FromUri]string[] group = null, string token = null)
        {
            string me = whoami();
            var t = _topicManager.FindTopicByName(topic, MembershipHelper.CheckJoinedGroup(whoami(), group));
            if (t == null)
            {
                return null;
            }

            TableContinuationToken tok = Utils.String2Token(token);
            if (tok == null)
            {
                _topicManager.clearUnreadMsgCountOfFavouriteTopic(me, t.Id);
            }
            return CreateDisplayMsgPag(_messageManager.TopicLine(t.Id.ToString(), count, tok));
        }

        /// <summary>
        /// Return a list of messages having the same topic
        /// 
        /// Example output is just like Userline above
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="group">group id, since topic with the same name can be exist in different groups</param>
        /// <param name="start">start timestamp</param>
        /// <param name="end">end timestamp</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination TopicLine(string topic, DateTime start, DateTime end, int count = 25, [FromUri]string[] group = null, string token = null)
        {
            string me = whoami();
            var t = _topicManager.FindTopicByName(topic, MembershipHelper.CheckJoinedGroup(whoami(), group));
            if (t == null)
            {
                return null;
            }

            TableContinuationToken tok = Utils.String2Token(token);
            if (tok == null)
            {
                _topicManager.clearUnreadMsgCountOfFavouriteTopic(me, t.Id);
            }
            return CreateDisplayMsgPag(_messageManager.TopicLine(t.Id.ToString(), start, end, count, tok));
        }

        /// <summary>
        /// Return a raw message
        /// 
        /// Example output:
        /// {
        ///     "User": "user2",
        ///     "ID": "251996365812114_microsoft_user2_b5627431-2d8e-4a11-b2a2-a4a05affa04a",
        ///     "Group": "mcrosoft",
        ///     "EventID": "AAQkAGU0N2MzZjk5LTZiZDQtNDgzMy1hODE3LThiMjUxYWU1NjIzMQAQAKciM9jy2slMoqdIuthUExI=",
        ///     "SchemaID": "none",
        ///     "Owner": null,
        ///     "AtUser": null,
        ///     "TopicName": null,
        ///     "MessageContent": "",
        ///     "PostTime": "2014-07-21T09:29:47.8851473Z",
        ///     "RichMessageID": "user2_2916624;1fee9aca-766c-4925-bfd3-a2e195d48f98",
        ///     "AttachmentID": null,
        ///     "Importance": 2
        /// }
        /// </summary>
        /// <param name="messageID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public Message GetRawMessage(string messageID)
        {
            whoami();
            return _messageManager.GetRawMessage(messageID);
        }

        ///// <summary>
        ///// Deprecated. Return the detail of a Message
        ///// 
        ///// Example output:
        ///// {
        /////     "ReplyCount": 3,
        /////     "Replies": [
        /////         {
        /////             "FromUser": "user2",
        /////             "ToUser": "user1",
        /////             "Message": "test cloud reply",
        /////             "PostTime": "2014-06-24T06:51:02.9789122Z",
        /////             "MessageUser": "user1",
        /////             "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        /////             "ReplyID": "251998708137021_431cab73-f6d7-4484-8872-797ec183ec68"
        /////         },
        /////         {
        /////             "FromUser": "user2",
        /////             "ToUser": "user1",
        /////             "Message": "greate",
        /////             "PostTime": "2014-06-24T06:48:26.5283644Z",
        /////             "MessageUser": "user1",
        /////             "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        /////             "ReplyID": "251998708293471_0e283889-18e2-44d3-a681-c07c35824c19"
        /////         },
        /////         {
        /////             "FromUser": "user2",
        /////             "ToUser": "user1",
        /////             "Message": "hello",
        /////             "PostTime": "2014-06-24T06:48:08.9586062Z",
        /////             "MessageUser": "user1",
        /////             "MessageID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        /////             "ReplyID": "251998708311041_51bfb009-53a2-435f-8559-5bf8e222887b"
        /////         }
        /////     ],
        /////     "User": "user1",
        /////     "ID": "251998708328967_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        /////     "EventID": "none",
        /////     "SchemaID": "none",
        /////     "Owner": null,
        /////     "AtUser": null,
        /////     "TopicName": null,
        /////     "MessageContent": "a new cloud message",
        /////     "PostTime": "2014-06-24T06:47:51.0325756Z",
        /////     "RichMessageID" : null,
        /////     "AttachmentID" : null,
        /////     "Importance": 2
        ///// }
        ///// </summary>
        ///// <param name="userid">user id of whom posted the message</param>
        ///// <param name="messageID">message id</param>
        ///// <returns></returns>
        //[HttpGet]
        //public MessageDetail GetMessage(string userid, string messageID)
        //{
        //    whoami();
        //    return _messageManager.GetMessageDetail(userid, messageID);
        //}

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
        ///     "ID": "251998708328967_microsoft_user1_9cc961ff-0600-43e8-902a-0b60e5087e8b",
        ///     "Group": "microsoft",
        ///     "EventID": "none",
        ///     "SchemaID": "none",
        ///     "Owner": null,
        ///     "AtUser": null,
        ///     "TopicName": null,
        ///     "MessageContent": "a new cloud message",
        ///     "RichMessageID": null,
        ///     "Attachment": null,
        ///     "PostTime": "2014-06-24T06:47:51.0325756Z",
        ///     "Importance": 2
        /// }
        /// </summary>
        /// <param name="msgID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessage GetDisplayMessage(string msgID)
        {
            whoami();
            return new DisplayMessage(_messageManager.GetMessage(msgID), _attManager, _accManager);
        }

        /// <summary>
        /// Return all replies within the message in a list
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "type": "reply",
        ///         "MessageUser": "user1",
        ///         "MessageID": "251997929671837_bdb3414b-3232-48fc-be5b-b6c15d48902f",
        ///         "User": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "ID": "251996885624787_a9ce32f7-5409-496f-a320-3f0283144540",
        ///         "EventID": "none",
        ///         "SchemaID": "none",
        ///         "Owner": null,
        ///         "AtUser": [
        ///             "user2"
        ///         ],
        ///         "TopicName": null,
        ///         "MessageContent": "test new reply",
        ///         "RichMessageID": null,
        ///         "Attachment": null,
        ///         "PostTime": "2014-07-15T09:06:15.2123811Z",
        ///         "Importance": 2
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
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, _accManager, _attManager));
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

        [HttpGet]
        public SearchResult SearchMessage(string keyword)
        {
            string me = whoami();
            return _searchManager.SearchMessage(keyword);
        }

        [HttpGet]
        public DisplayMessagePagination SearchMessageResults(string searchId, int count = 25, [FromUri]string[] group = null, string token = null)
        {
            string me = whoami();

            TableContinuationToken tok = Utils.String2Token(token);
            return CreateDisplayMsgPag(_searchManager.GetSearchResults(searchId, count, tok));
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
        ///     "ID": "251998703315809_microsoft_user2_64a6332e-7809-46cc-89c1-5d0624db7111",
        /// 	"Group": "microsoft",
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
        ///     "PostTime": "2014-06-24T08:11:24.1907127Z",
        ///     "Importance": 2
        /// }
        /// </summary>
        /// <param name="message">message content. If a single word of content starts with @ and the suffix is a valid
        /// userid, such as @user1, the userid will be added into atUser list. If a single word starts with and ends with #,
        /// such as #world cup#, it will be recognized as a topic name and be added into topicName list.
        /// </param>
        /// <param name="group">group id. The group will be set to default group if the group id is null</param>
        /// <param name="schemaID">schema id</param>
        /// <param name="eventID">event id</param>
        /// <param name="owner">user id of the owner. Can be a list.</param>
        /// <param name="atUser">user id of related users. Can be a list</param>
        /// <param name="topicName">topic name of related topic. Can be a list</param>
        /// <param name="richMessage">rich message. Up to 992 kb</param>
        /// <param name="attachmentID">attachment id related. Can be a list</param>
        /// <param name="importance">importance of the message</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayMessage PostMessage(string message,
                                    string group = null,
                                    string schemaID = "none",
                                    string eventID = "none",
                                    [FromUri]string[] owner = null,
                                    [FromUri]string[] atUser = null,
                                    [FromUri]string[] topicName = null,
                                    string richMessage = null,
                                    [FromUri]string[] attachmentID = null,
                                    int importance = 2
                                    )
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

            return new DisplayMessage(_messageManager.PostMessage(whoami(), group, eventID, schemaID, owner, atUser, topicName, message, richMessage, attachmentID, importance, DateTime.UtcNow), new AttachmentManager(), new AccountManager());
            //return new ActionResult();
        }

        /// <summary>
        /// Post a new message. Same as the Get Api.
        /// </summary>
        public class MessageModel
        {
            public string Message { get; set; }
            public string Group { get; set; }
            public string SchemaID { get; set; }
            public string EventID { get; set; }
            public string[] TopicName { get; set; }
            public string[] Owner { get; set; }
            public string[] AtUser { get; set; }
            public string RichMessage { get; set; }
            public string[] AttachmentID { get; set; }
            public int Importance { get; set; }
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
            return PostMessage(msg.Message, msg.Group, msg.SchemaID, msg.EventID, msg.Owner, msg.AtUser, msg.TopicName, msg.RichMessage, msg.AttachmentID, msg.Importance);
            //return new ActionResult();
        }

        private bool GetFilterDateTime(string filter, out DateTime start, out DateTime end)
        {
            start = end = DateTime.UtcNow;
            DateTime daysago = DateTime.UtcNow;

            switch (filter)
            {
                case "latest24hours":
                    start = end.AddDays(-1);
                    break;
                case "latest3days":
                    daysago = end.AddDays(-3);
                    start = new DateTime(daysago.Year, daysago.Month, daysago.Day);
                    break;
                case "latest7days":
                    daysago = end.AddDays(-7);
                    start = new DateTime(daysago.Year, daysago.Month, daysago.Day);
                    break;
                case "latest1month":
                    daysago = end.AddMonths(-1);
                    start = new DateTime(daysago.Year, daysago.Month, daysago.Day);
                    break;
                case null:
                case "":
                case "all":
                default:
                    return false;
            }

            return true;
        }
    }
}
