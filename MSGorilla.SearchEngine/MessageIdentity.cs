using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.SearchEngine
{
    public class MessageIdentity
    {
        #region Fields
        private string _userId;
        private string _messageId;
        #endregion

        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public string MessageId
        {
            get { return _messageId; }
            set { _messageId = value; }
        }

        public MessageIdentity()
        {
        }

        public MessageIdentity(string userId, string messageId)
        {
            _userId = userId;
            _messageId = messageId;
        }

        public string ToMessageString()
        {
            return string.Format("{0};{1}", _userId, _messageId);
        }

        public MessageIdentity FromMessageString(string msgString)
        {
            var a = msgString.Split(';');
            _userId = a[0];
            _messageId = a[1];

            return this;
        }

        /// <summary>
        /// Debug string
        /// </summary>
        public override string ToString()
        {
            return _messageId;
        }

        public override int GetHashCode()
        {
            return _messageId.GetHashCode();
        }
    }
}
