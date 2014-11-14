using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.IO;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.MailStore.Helper;
using MSGorilla.Library;
using Newtonsoft.Json;
using MSGorilla.MailStore;
using System.Runtime.Caching;

namespace MSGorilla.MailDispatcher
{
    public class WorkerRole : RoleEntryPoint
    {
        private MemoryCache _mailboxCache;
        private MemoryCache _mailStoreCache;
        private Dictionary<string, string> _groupID2Name;

        private TopicManager _topicManager;
        private GroupManager _groupManager;
        private AccountManager _accountManager;
        private RichMsgManager _richMsgManager;

        string GetGroupDisplayName(string groupID)
        {
            if (_groupID2Name.ContainsKey(groupID))
            {
                return _groupID2Name[groupID];
            }

            Group g = _groupManager.GetGroupByID(groupID);
            _groupID2Name[groupID] = g.DisplayName;
            return g.DisplayName;
        }

        List<string> GetGroupFollower(string groupID, string userid)
        {
            HashSet<string> groupMember = new HashSet<string>();
            foreach (var member in _groupManager.GetAllGroupMember(groupID))
            {
                groupMember.Add(member.MemberID.ToLower());
            }

            List<string> followerInGroup = new List<string>();
            List<UserProfile> followers = _accountManager.Followers(userid);
            foreach (var user in followers)
            {
                if (groupMember.Contains(user.Userid))
                {
                    followerInGroup.Add(user.Userid);
                }
            }

            return followerInGroup;
        }

        MailStore.MailStore GetMailStore(string userid)
        {
            if (_mailStoreCache.Contains(userid))
            {
                return _mailStoreCache[userid] as MailStore.MailStore;
            }

            MailStore.MailStore store = new MailStore.MailStore(userid);
            _mailStoreCache.Add(userid, store, DateTime.Now.AddHours(12));  //cache 12 hours
            return store;
        }

        int GetMailBoxID(string userid, string path)
        {
            string key = string.Format("{0};{1}", userid, path);
            if (_mailboxCache.Contains(key))
            {
                return (int)_mailboxCache[key];
            }

            MailStore.MailStore store = GetMailStore(userid);
            MailBox mailbox = store.CreateMailboxIfNotExist(path);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now.AddHours(24);
            _mailboxCache.Set(key, mailbox.MailboxID, policy);

            //_mailboxCache.Add(key, mailbox.MailboxID, DateTime.Now.AddHours(12));
            return mailbox.MailboxID;
        }

        public void DispatchMail(Message msg)
        {
            int defaultMailLength = 0;
            string richMessage = null;
            if (string.IsNullOrEmpty(msg.RichMessageID))
            {
                richMessage = _richMsgManager.GetRichMessage(richMessage);
            }

            defaultMailLength = MSGorillaMailGenerator.CreateTextMessageMail(msg, richMessage, "somebody").Length;

            try
            {
                //Owner
                if(msg.Owner != null)
                {
                    foreach (string owner in msg.Owner)
                    {
                        MailStore.MailStore store = GetMailStore(owner);
                        int boxID = GetMailBoxID(owner, "Inbox/Own");
                        store.AddMailMessage(msg.ID, 
                            defaultMailLength - "somebody".Length + owner.Length,
                            msg.Importance,
                            boxID);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }

            try
            {
                //Mention
                if (msg.AtUser != null)
                {
                    foreach (string userid in msg.AtUser)
                    {
                        MailStore.MailStore store = GetMailStore(userid);
                        int boxID = GetMailBoxID(userid, "Inbox/Mention");
                        store.AddMailMessage(msg.ID,
                            defaultMailLength - "somebody".Length + userid.Length,
                            msg.Importance,
                            boxID);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }

            try
            {
                //Home/Group
                foreach (string userid in GetGroupFollower(msg.Group, msg.User))
                {
                    MailStore.MailStore store = GetMailStore(userid);
                    int boxID = GetMailBoxID(userid, "Inbox/Home/" + GetGroupDisplayName(msg.Group));
                    store.AddMailMessage(msg.ID,
                        defaultMailLength - "somebody".Length + userid.Length,
                        msg.Importance,
                        boxID);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }

            try
            {
                //Topic/Topics
                if (msg.TopicName != null)
                {
                    foreach (var topicName in msg.TopicName)
                    {
                        Topic topic = _topicManager.FindTopicByName(topicName, msg.Group);
                        List<FavouriteTopic> whoLikesTopic = null;
                        using (var ctx = new MSGorillaEntities())
                        {
                            whoLikesTopic = ctx.FavouriteTopics.Where(fa => fa.TopicID == topic.Id).ToList();
                        }
                        foreach (var fa in whoLikesTopic)
                        {
                            string userid = fa.Userid;
                            MailStore.MailStore store = GetMailStore(userid);
                            int boxID = GetMailBoxID(userid,
                                string.Format("Inbox/Topic/{0}({1})", topic.Name, GetGroupDisplayName(topic.GroupID)));
                            store.AddMailMessage(msg.ID,
                                defaultMailLength - "somebody".Length + userid.Length,
                                msg.Importance,
                                boxID);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }
        }

        public override void Run()
        {
            Trace.TraceInformation("MSGorilla.MailDispatcher entry point called");

            _mailboxCache = new MemoryCache("mailbox");
            _mailStoreCache = new MemoryCache("mailstore");
            _groupID2Name = new Dictionary<string, string>();
            _topicManager = new TopicManager();
            _groupManager = new GroupManager();
            _accountManager = new AccountManager();
            _richMsgManager = new RichMsgManager();

            CloudQueue _queue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.MailMessage);;

            while (true)
            {
                try
                {
                    CloudQueueMessage message = _queue.GetMessage();
                    while (message == null)
                    {
                        Thread.Sleep(1000);
                        message = _queue.GetMessage();
                    }
                    _queue.UpdateMessage(message,
                        TimeSpan.FromSeconds(60.0 * 1),  // Make it in five minutes
                        MessageUpdateFields.Visibility);

                    Message msg = JsonConvert.DeserializeObject<Message>(message.AsString);

                    DispatchMail(msg);

                    _queue.DeleteMessage(message);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception in worker role", e.StackTrace);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
