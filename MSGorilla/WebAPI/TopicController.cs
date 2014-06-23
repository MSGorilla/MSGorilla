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

        /// <summary>
        /// Return the specification of a topic.
        /// </summary>
        /// <param name="topicid">id of the topic</param>
        /// <returns></returns>
        [HttpGet]
        public Topic FindTopic(int topicid)
        {
            return _topicManager.FindTopic(topicid);
        }

        /// <summary>
        /// Return a list a topics the topic name of which contain keyword
        /// </summary>
        /// <param name="keyword">key word</param>
        /// <returns></returns>
        [HttpGet]
        public List<Topic> SearchTopic(string keyword)
        {
            return _topicManager.SearchTopic(keyword);
        }

        /// <summary>
        /// Add a new topic.
        /// Return the topic created.
        /// </summary>
        /// <param name="Name">name of the topic</param>
        /// <param name="Description">description of the topic</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the list of all the topics.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Topic> GetAllTopic()
        {
            return _topicManager.GetAllTopics();
        }

        /// <summary>
        /// Return a topics list which contains the most messages order by message count desc
        /// </summary>
        /// <param name="count">count of topic in the list</param>
        /// <returns></returns>
        [HttpGet]
        public List<Topic> HotTopics(int count = 5)
        {
            return _topicManager.GetHotTopics(count);
        }

        /// <summary>
        /// Add the topic into current user's favourite topic list 
        /// </summary>
        /// <param name="topicID">id of the topic</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddFavouriteTopic(int topicID)
        {
            _topicManager.AddFavouriteTopic(whoami(), topicID);
            return new ActionResult();
        }

        /// <summary>
        /// Remove the topic from the favourite topic list of current User
        /// </summary>
        /// <param name="topicID">id of the topic</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RemoveFavouriteTopic(int topicID)
        {
            _topicManager.Remove(whoami(), topicID);
            return new ActionResult();
        }

        /// <summary>
        /// Return the favourite topic list of the current user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayFavouriteTopic> GetMyFavouriteTopic()
        {
            return _topicManager.GetFavouriteTopic(whoami());
        }

        /// <summary>
        /// Return the favourite topic list of a user
        /// </summary>
        /// <param name="userid">id of a user</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayFavouriteTopic> GetUserFavouriteTopic(string userid)
        {
            return _topicManager.GetFavouriteTopic(userid);
        }

        /// <summary>
        /// Return whether the topic is current user's favourite topic or not
        /// </summary>
        /// <param name="topicID">id of the topic</param>
        /// <returns></returns>
        [HttpGet]
        public bool IsFavouriteTopic(int topicID)
        {
            return _topicManager.IsFavouriteTopic(whoami(), topicID);
        }

        /// <summary>
        /// Return whether the topic is current user's favourite topic or not
        /// </summary>
        /// <param name="topic">name of the topic, case insensitive</param>
        /// <returns></returns>
        [HttpGet]
        public bool IsFavouriteTopic(string topic)
        {
            var t = _topicManager.FindTopicByName(topic);
            if (t == null)
            {
                return false;
            }

            return _topicManager.IsFavouriteTopic(whoami(), t.Id);
        }
    }
}