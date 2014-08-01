using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;
namespace MSGorilla.Library
{
    public class TopicManager
    {
        //private MSGorillaEntities _gorillaCtx;

        public List<Topic> GetAllTopics(string[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                return new List<Topic>();
            }

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Topics.Where(topic => groups.Any(groupID => topic.GroupID == groupID)).ToList();
            }
        }

        public Topic AddTopic(Topic topic)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                topic = _gorillaCtx.Topics.Add(topic);
                _gorillaCtx.SaveChanges();
                return topic;
            }
        }

        public void incrementTopicCount(int topicID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
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
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                FavouriteTopic ftopic = _gorillaCtx.FavouriteTopics.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).FirstOrDefault();
                if (ftopic != null)
                {
                    ftopic.UnreadMsgCount = 0;
                    _gorillaCtx.SaveChanges();
                }
            }
        }

        public void incrementUnreadMsgCountOfFavouriteTopic(int topicID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                _gorillaCtx.Database.ExecuteSqlCommand(
                    "update [favouritetopic] set UnreadMsgcount = UnreadMsgcount  + 1 where topicid={0}",
                    topicID);
            }
        }

        public Topic FindTopicByName(string name, string groupID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Topic ret = null;
                try
                {
                    ret = _gorillaCtx.Topics.Where(topic => topic.Name == name && groupID == topic.GroupID).Single();
                }
                catch
                {

                }
                return ret;
            }
        }

        public Topic FindTopicByName(string name, string[] groupIDs)
        {
            if (groupIDs == null || groupIDs.Length == 0)
            {
                return null;
            }
            using (var _gorillaCtx = new MSGorillaEntities())
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

        public Topic FindTopic(int topicID, MSGorillaEntities _gorillaCtx = null)
        {
            if (_gorillaCtx != null)
            {
                return _gorillaCtx.Topics.Find(topicID);
                
            }
            using (_gorillaCtx = new MSGorillaEntities())
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
            using (var _gorillaCtx = new MSGorillaEntities())
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

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Topics.Where(
                   topic => groupIDs.Any(groupid => groupid == topic.GroupID)
                ).OrderByDescending(topic => topic.MsgCount).Take(count).ToList();
            }
        }

        public void AddFavouriteTopic(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                if (_gorillaCtx.FavouriteTopics.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).Count() == 0)
                {
                    FavouriteTopic ftopic = new FavouriteTopic();
                    ftopic.TopicID = topicID;
                    ftopic.Userid = userid;
                    ftopic.UnreadMsgCount = 0;
                    _gorillaCtx.FavouriteTopics.Add(ftopic);
                    _gorillaCtx.SaveChanges();
                }
            }
        }

        public void Remove(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                List<FavouriteTopic> finds = _gorillaCtx.FavouriteTopics.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).ToList();
                _gorillaCtx.FavouriteTopics.RemoveRange(finds);
                _gorillaCtx.SaveChanges();
            }
        }

        public List<DisplayFavouriteTopic> GetFavouriteTopic(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                List<FavouriteTopic> topics = _gorillaCtx.FavouriteTopics.Where(f => f.Userid.Equals(userid)).ToList();
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
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                FavouriteTopic topic = _gorillaCtx.FavouriteTopics.Where(f => f.Userid.Equals(userid) && f.TopicID.Equals(topicid)).FirstOrDefault();
                if (topic != null)
                    return topic.UnreadMsgCount;

                return 0;
            }
        }


        public bool IsFavouriteTopic(string userid, int topicID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return IsFavouriteTopic(userid, topicID, _gorillaCtx);
            }
        }

        public bool IsFavouriteTopic(string userid, int topicID, MSGorillaEntities _gorillaCtx)
        {
            FavouriteTopic ftopic = _gorillaCtx.FavouriteTopics.Where(f => f.Userid.Equals(userid) && f.TopicID == topicID).FirstOrDefault();
            if (ftopic != null)
            {
                return true;
            }
            return false;
        }

        public List<DisplayTopic> ToDisplayTopicList(IEnumerable<Topic> topics, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                HashSet<int> likedTopicID = new HashSet<int>();
                foreach(var fa in _gorillaCtx.UserProfiles.Find(userid).FavouriteTopics.ToArray())
                {
                    likedTopicID.Add(fa.TopicID);
                }
                List<DisplayTopic> list = new List<DisplayTopic>();
                foreach (Topic topic in topics)
                {
                    list.Add(new DisplayTopic(topic, likedTopicID.Contains(topic.Id)));
                }
                return list;
            }
        }
    }
}
