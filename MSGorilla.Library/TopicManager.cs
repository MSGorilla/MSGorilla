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

        public Topic FindTopic(int topicID)
        {
            return _gorillaCtx.Topics.Find(topicID);
        }

        public List<Topic> SearchTopic(string keyword)
        {
            return _gorillaCtx.Topics.Where(topic => topic.Name.Contains(keyword)).ToList();
        }
    }
}
