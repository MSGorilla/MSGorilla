using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

using MSGorilla.Library;
using MSGorilla.Library.Models;

namespace MSGorillaIMAPServer
{
    /// <summary>
    /// Cache the message queries from MSgorilla
    /// </summary>
    public class MSGorillaHelper
    {
        private static MessageManager _messageManager = new MessageManager();
        private static RichMsgManager _richMsgManager = new RichMsgManager();

        private static MemoryCache _msgCache = new MemoryCache("message");
        private static MemoryCache _richMsgCache = new MemoryCache("richmessage");

        private const string NULL = "==+null!@#$";

        public static Message GetMessage(string msgID)
        {
            if (string.IsNullOrEmpty(msgID))
            {
                return null;
            }

            if (_msgCache.Contains(msgID))
            {
                object obj = _msgCache[msgID];
                if (NULL.Equals(obj))
                {
                    return null;
                }
                return obj as Message;
            }

            Message message = _messageManager.GetRawMessage(msgID);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now.AddHours(24);
            if (message == null)
            {
                _msgCache.Set(msgID, NULL, policy);
            }
            else
            {
                _msgCache.Set(msgID, message, policy);
            }

            return message;
        }

        public static string GetRichmessage(string richMsgID)
        {
            if (string.IsNullOrEmpty(richMsgID))
            {
                return null;
            }

            if (_richMsgCache.Contains(richMsgID))
            {
                object obj = _richMsgCache[richMsgID];
                if (NULL.Equals(obj))
                {
                    return null;
                }
                return obj as string;
            }

            string richMsg = _richMsgManager.GetRichMessage(richMsgID);

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now.AddHours(24);

            if (richMsg == null)
            {
                _richMsgCache.Set(richMsgID, NULL, policy);
            }
            else
            {
                _richMsgCache.Set(richMsgID, richMsg, policy);
            }

            return richMsg;
        }
    }
}
