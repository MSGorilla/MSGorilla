using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace MSGorilla.SearchEngine
{
    /// <summary>Instance of a word</summary>
    public class Word
    {
        #region Private fields: _text, _MessagePositionCollection
        /// <summary>Collection of messages the word appears in</summary>
        private Dictionary<string, List<int>> _messagePositionCollection = new System.Collections.Generic.Dictionary<string, List<int>>();

        /// <summary>The word itself</summary>
        private string _text;
        #endregion

        /// <summary>
        /// The catalogued word
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Empty constructor required for serialization
        /// </summary>
        public Word() { }

        /// <summary>Constructor with first message reference</summary>
        public Word(string text, MessageIdentity inmessage, int position)
        {
            _text = text;

            List<int> l = new List<int>();
            l.Add(position);
            _messagePositionCollection.Add(inmessage.ToMessageString(), l);
        }

        /// <summary>Add a message referencing this word</summary>
        public void Add(MessageIdentity inmessage, int position)
        {
            var msgid = inmessage.ToMessageString();
            if (_messagePositionCollection.ContainsKey(msgid))
            {
                _messagePositionCollection[msgid].Add(position);
            }
            else
            {
                List<int> l = new List<int>();
                l.Add(position);
                _messagePositionCollection.Add(msgid, l);
            }
        }

        /// <summary>Collection of messages containing this Word (Value=List of position numbers)</summary>
        public Dictionary<string, List<int>> InMessagesWithPosition()
        {
            return _messagePositionCollection; 
        }

        /// <summary>Debug string</summary>
        public override string ToString()
        {
            return Text;
        }
    }  
}
