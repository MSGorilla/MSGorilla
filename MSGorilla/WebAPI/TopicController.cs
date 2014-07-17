using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using System.Runtime.Serialization;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;


namespace MSGorilla.WebApi
{
    public class DisplayTopic : Topic
    {
        [DataMember]
        public bool IsLiked { get; private set; }

        public DisplayTopic(Topic topic, bool isliked)
        {
            Id = topic.Id;
            Name =topic.Name;
            Description = topic.Description;
            MsgCount = topic.MsgCount;
            IsLiked = isliked;
        }
    }

    public class TopicController : BaseController
    {
        TopicManager _topicManager = new TopicManager();

        /// <summary>
        /// Return the specification of a topic or null.
        /// 
        /// Example out:
        /// {
        ///     "Id": 1007,
        ///     "Name": "worldcup",
        ///     "Description": null,
        ///     "MsgCount": 19
        /// }
        /// </summary>
        /// <param name="topicid">id of the topic</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayTopic FindTopic(int topicid)
        {
            return new DisplayTopic(_topicManager.FindTopic(topicid), IsFavouriteTopic(topicid));
        }

        /// <summary>
        /// Return a list a topics the topic name of which contain keyword
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "Id": 1007,
        ///         "Name": "worldcup",
        ///         "Description": null,
        ///         "MsgCount": 19
        ///     },
        ///     {
        ///         "Id": 2256,
        ///         "Name": "worldcupfinal",
        ///         "Description": "join the discusstion",
        ///         "MsgCount": 0
        ///     }
        /// ]
        /// </summary>
        /// <param name="keyword">key word</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayTopic> SearchTopic(string keyword)
        {
            var topiclist = _topicManager.SearchTopic(keyword);
            var disptopiclist = new List<DisplayTopic>();

            foreach (var t in topiclist)
            {
                disptopiclist.Add(new DisplayTopic(t, IsFavouriteTopic(t.Id)));
            }

            return disptopiclist;
        }

        /// <summary>
        /// Add a new topic and return the topic created.
        /// 
        /// Example output:
        /// {
        ///     "Id": 2256,
        ///     "Name": "worldcupfinal",
        ///     "Description": "join the discusstion",
        ///     "MsgCount": 0
        /// }
        /// </summary>
        /// <param name="Name">name of the topic</param>
        /// <param name="Description">description of the topic</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayTopic AddTopic(string Name, string Description)
        {
            whoami();
            Topic topic = new Topic();
            topic.Name = Name;
            topic.Description = Description;
            topic.MsgCount = 0;
            var newtopic = _topicManager.AddTopic(topic);
            return new DisplayTopic(newtopic, IsFavouriteTopic(newtopic.Id));
        }

        /// <summary>
        /// Return the list of all the topics.
        /// 
        /// Example output
        /// [
        ///     {
        ///         "Id": 1035,
        ///         "Name": "PCV Build",
        ///         "Description": null,
        ///         "MsgCount": 12
        ///     },
        ///     ......,
        ///     ......,
        ///     {
        ///         "Id": 2256,
        ///         "Name": "worldcupfinal",
        ///         "Description": "join the discusstion",
        ///         "MsgCount": 0
        ///     }
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayTopic> GetAllTopic()
        {
            var topiclist = _topicManager.GetAllTopics();
            var disptopiclist = new List<DisplayTopic>();

            foreach (var t in topiclist)
            {
                disptopiclist.Add(new DisplayTopic(t, IsFavouriteTopic(t.Id)));
            }

            return disptopiclist;
        }

        /// <summary>
        /// Return a topics list which contains the most messages order by message count desc.
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "Id": 1015,
        ///         "Name": "WOSS WAES Job",
        ///         "Description": null,
        ///         "MsgCount": 327
        ///     },
        ///     {
        ///         "Id": 1062,
        ///         "Name": "WOSS TFS",
        ///         "Description": null,
        ///         "MsgCount": 181
        ///     },
        ///     {
        ///         "Id": 1066,
        ///         "Name": "WOSS TFS Changed",
        ///         "Description": null,
        ///         "MsgCount": 120
        ///     }
        /// ]
        /// </summary>
        /// <param name="count">count of topic in the list</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayTopic> HotTopics(int count = 5)
        {
            var topiclist = _topicManager.GetHotTopics(count);
            var disptopiclist = new List<DisplayTopic>();

            foreach (var t in topiclist)
            {
                disptopiclist.Add(new DisplayTopic(t, IsFavouriteTopic(t.Id)));
            }

            return disptopiclist;
        }

        /// <summary>
        /// Add the topic into current user's favourite topic list 
        /// 
        /// Example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "success"
        /// }
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
        /// 
        /// Example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "success"
        /// }
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
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "userid": "user1",
        ///         "topicID": 1038,
        ///         "UnreadMsgCount": 0,
        ///         "topicName": "WOSS Bug 294144",
        ///         "topicDescription": null,
        ///         "topicMsgCount": 9
        ///     },
        ///     {
        ///         "userid": "user1",
        ///         "topicID": 1039,
        ///         "UnreadMsgCount": 0,
        ///         "topicName": "WOSS Bugs",
        ///         "topicDescription": null,
        ///         "topicMsgCount": 61
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayFavouriteTopic> GetMyFavouriteTopic()
        {
            return _topicManager.GetFavouriteTopic(whoami());
        }

        /// <summary>
        /// Return the favourite topic list of a user
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "userid": "user2",
        ///         "topicID": 2,
        ///         "UnreadMsgCount": 0,
        ///         "topicName": "test2",
        ///         "topicDescription": "test22",
        ///         "topicMsgCount": 3
        ///     },
        ///     {
        ///         "userid": "user2",
        ///         "topicID": 3,
        ///         "UnreadMsgCount": 2,
        ///         "topicName": "top3",
        ///         "topicDescription": "333",
        ///         "topicMsgCount": 5
        ///     }
        /// ]
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
        /// 
        /// Example output:
        /// false
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
        /// 
        /// Example output:
        /// true
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