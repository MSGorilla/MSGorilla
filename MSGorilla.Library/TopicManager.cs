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

        public List<Topic> GetAllTopics()
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.ToList();
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

        public void incrementTopicCount(string topicID)
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

        public Topic FindTopic(string topicID, MSGorillaContext _gorillaCtx = null)
        {
            Topic ret = null;
            if (_gorillaCtx == null)
            {
                using (_gorillaCtx = new MSGorillaContext())
                {

                    try
                    {
                        ret = _gorillaCtx.Topics.Find(int.Parse(topicID));
                    }
                    catch
                    {
                    }
                    return ret;
                }
            }
            try
            {
                ret = _gorillaCtx.Topics.Find(int.Parse(topicID));
            }
            catch
            {
            }
            return ret;
        }

        public Topic FindTopicByName(string name)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                Topic ret = null;
                try
                {
                    ret = _gorillaCtx.Topics.Where(topic => topic.Name == name).Single();
                }
                catch
                {

                }
                return ret;
            }
        }

        public Topic FindTopic(int topicID, MSGorillaContext _gorillaCtx = null)
        {
            if (_gorillaCtx == null)
            {
                _gorillaCtx = new MSGorillaContext();
            }
            return _gorillaCtx.Topics.Find(topicID);
        }

        public List<Topic> SearchTopic(string keyword)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.Where(topic => topic.Name.Contains(keyword)).ToList();
            }
        }

        public List<Topic> GetHotTopics(int count = 5)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Topics.SqlQuery(
                    @"select top({0}) * from [topic] ORDER BY msgcount desc",
                    new object[] { count }
                ).ToList();
            }
        }

        public void AddFavouriteTopic(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                if (_gorillaCtx.Users.Find(userid) == null)
                {
                    return;
                }
                if (_gorillaCtx.Topics.Find(topicID) == null)
                {
                    return;
                }
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
