using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.DAL;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;
namespace MSGorilla.Library
{
    public class TopicManager
    {
        //private MSGorillaContext _gorillaCtx;

        public List<Topic> GetAllTopics(string[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                return new List<Topic>();
            }

            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.Where(topic => groups.Any(groupID => topic.GroupID == groupID)).ToList();
            }
        }

        public Topic AddTopic(Topic topic)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                topic = _gorillaCtx.Topics.Add(topic);
                _gorillaCtx.SaveChanges();
                return topic;
            }
        }

        public void incrementTopicCount(int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                Topic topic = FindTopic(topicID, _gorillaCtx);
                if (topic != null)
                {
                    topic.MsgCount++;
                    _gorillaCtx.SaveChanges();
                }
            }
        }

        public void clearUnreadMsgCountOfFavouriteTopic(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                FavouriteTopic ftopic = _gorillaCtx.favouriteTopic.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).FirstOrDefault();
                if (ftopic != null)
                {
                    ftopic.UnreadMsgCount = 0;
                    _gorillaCtx.SaveChanges();
                }
            }
        }

        public void incrementUnreadMsgCountOfFavouriteTopic(int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                _gorillaCtx.Database.ExecuteSqlCommand(
                    "update [favouritetopic] set UnreadMsgcount = UnreadMsgcount  + 1 where topicid={0}",
                    topicID);
            }
        }

        public Topic FindTopicByName(string name, string[] groupIDs)
        {
            if (groupIDs == null || groupIDs.Length == 0)
            {
                return null;
            }
            using (var _gorillaCtx = new MSGorillaContext())
            {
                Topic ret = null;
                try
                {
                    ret = _gorillaCtx.Topics.Where(topic => topic.Name == name && groupIDs.Any(groupID => groupID == topic.GroupID)).Single();
                }
                catch
                {

                }
                return ret;
            }
        }

        public Topic FindTopic(int topicID, MSGorillaContext _gorillaCtx = null)
        {
            if (_gorillaCtx != null)
            {
                return _gorillaCtx.Topics.Find(topicID);
                
            }
            using (_gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.Find(topicID);
            }
        }

        public List<Topic> SearchTopic(string keyword, string[] groupIDs)
        {
            if (groupIDs == null || groupIDs.Length == 0)
            {
                return new List<Topic>();
            }
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.Where
                    (
                        topic => topic.Name.Contains(keyword) 
                            && groupIDs.Any(groupID => groupID == topic.GroupID)
                    ).ToList();
            }
        }

        public List<Topic> GetHotTopics(string[] groupIDs, int count = 5)
        {
            if (groupIDs == null || groupIDs.Length == 0)
            {
                return new List<Topic>();
            }

            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.Where(
                   topic => groupIDs.Any(groupid => groupid == topic.GroupID)
                ).OrderByDescending(topic => topic.MsgCount).Take(count).ToList();
            }
        }

        public void AddFavouriteTopic(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                if (_gorillaCtx.favouriteTopic.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).Count() == 0)
                {
                    FavouriteTopic ftopic = new FavouriteTopic();
                    ftopic.TopicID = topicID;
                    ftopic.Userid = userid;
                    ftopic.UnreadMsgCount = 0;
                    _gorillaCtx.favouriteTopic.Add(ftopic);
                    _gorillaCtx.SaveChanges();
                }
            }
        }

        public void Remove(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                List<FavouriteTopic> finds = _gorillaCtx.favouriteTopic.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).ToList();
                _gorillaCtx.favouriteTopic.RemoveRange(finds);
                _gorillaCtx.SaveChanges();
            }
        }

        public List<DisplayFavouriteTopic> GetFavouriteTopic(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                List<FavouriteTopic> topics = _gorillaCtx.favouriteTopic.Where(f => f.Userid.Equals(userid)).ToList();
                List<DisplayFavouriteTopic> dtopic = new List<DisplayFavouriteTopic>();

                foreach (FavouriteTopic t in topics)
                {
                    dtopic.Add(new DisplayFavouriteTopic(t, _gorillaCtx));
                }
                return dtopic;
            }
        }

        public int GetFavouriteTopicUnreadCount(string userid, int topicid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                FavouriteTopic topic = _gorillaCtx.favouriteTopic.Where(f => f.Userid.Equals(userid) && f.TopicID.Equals(topicid)).FirstOrDefault();
                if (topic != null)
                    return topic.UnreadMsgCount;

                return 0;
            }
        }


        public bool IsFavouriteTopic(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                FavouriteTopic ftopic = _gorillaCtx.favouriteTopic.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).FirstOrDefault();
                if (ftopic != null)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
