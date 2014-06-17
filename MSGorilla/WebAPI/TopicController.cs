using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.WebApi
{
    public class TopicController : BaseController
    {
        TopicManager _topicManager = new TopicManager();

        [HttpGet]
        public Topic FindTopic(int topicid)
        {
            return _topicManager.FindTopic(topicid);
        }

        [HttpGet]
        public List<Topic> SearchTopic(string keyword)
        {
            return _topicManager.SearchTopic(keyword);
        }

        [HttpGet]
        public Topic AddTopic(string Name, string Description)
        {
            whoami();
            Topic topic = new Topic();
            topic.Name = Name;
            topic.Description = Description;
            topic.MsgCount = 0;
            return _topicManager.AddTopic(topic);
        }

        [HttpGet]
        public List<Topic> GetAllTopic()
        {
            return _topicManager.GetAllTopics();
        }

        [HttpGet]
        public List<Topic> HotTopics(int count = 5)
        {
            return _topicManager.GetHotTopics(count);
        }

        [HttpGet]
        public ActionResult AddFavouriteTopic(int topicID)
        {
            _topicManager.AddFavouriteTopic(whoami(), topicID);
            return new ActionResult();
        }

        [HttpGet]
        public ActionResult RemoveFavouriteTopic(int topicID)
        {
            _topicManager.Remove(whoami(), topicID);
            return new ActionResult();
        }

        [HttpGet]
        public List<DisplayFavouriteTopic> GetMyFavouriteTopic()
        {
            return _topicManager.GetFavouriteTopic(whoami());
        }

        [HttpGet]
        public bool IsFavouriteTopic(int topicID)
        {
            return _topicManager.IsFavouriteTopic(whoami(), topicID);
        }
    }
}