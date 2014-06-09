using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.DAL;
using MSGorilla.Library.Models.SqlModels;
namespace MSGorilla.Library
{
    public class TopicManager
    {
        private MSGorillaContext _gorillaCtx;

        public TopicManager()
        {
            _gorillaCtx = new MSGorillaContext();
        }

        public List<Topic> GetAllTopics()
        {
            return _gorillaCtx.Topics.ToList();
        }

        public Topic AddTopic(Topic topic)
        {
            topic =  _gorillaCtx.Topics.Add(topic);
            _gorillaCtx.SaveChanges();
            return topic;
        }

        public void incrementTopicCount(string topicID)
        {
            Topic topic = FindTopic(topicID);
            if (topic != null)
            {
                topic.MsgCount++;
                _gorillaCtx.SaveChanges();
            }            
        }

        public Topic FindTopic(string topicID)
        {
            Topic ret = null;
            try
            {
                ret = _gorillaCtx.Topics.Find(int.Parse(topicID));
            }
            catch
            {
            }
            return ret;
        }

        public Topic FindTopic(int topicID)
        {
            return _gorillaCtx.Topics.Find(topicID);
        }

        public List<Topic> SearchTopic(string keyword)
        {
            return _gorillaCtx.Topics.Where(topic => topic.Name.Contains(keyword)).ToList();
        }

        public List<Topic> GetHotTopics(int count = 5)
        {
            return _gorillaCtx.Topics.SqlQuery(
                    @"select t.id, t.name, t.description, t.msgcount from (
                        select * , ROW_NUMBER() OVER (ORDER BY msgcount desc) as row from [topic]
                    )t where t.row > 0 and t.row <= {0}",
                    new object[] { count }
                ).ToList();
        }
    }
}
