using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    public class MessageIDList : List<MessageIDListItem>
    {
        public MessageIDList(string messageList)
        {
            messageList = messageList.Trim();
			string[] listParts = messageList.Split(',');
			foreach (string part in listParts) {
				this.Add(new MessageIDListItem(part));
			}
		}
    }

    public enum MessageIDListItemType {
		/// <summary>
		/// Process a single message. E.g. "2"
		/// </summary>
		Single,

		/// <summary>
		/// Process a range of messages. E.g. "2:4"
		/// </summary>
		BoundedRange,

		/// <summary>
		/// Process a unbounded range of messages. E.g. "1:*"
		/// </summary>
		UnboundedRange
	}

	public class MessageIDListItem {
		/// <summary>
		/// Creates a new list item from the given string.
		/// </summary>
		/// <param name="itemString">The string to parse.</param>
        public MessageIDListItem(string itemString)
        {
			if (itemString.IndexOf(':') > -1) {
				// A range of messages, e.g. "2:4" or "1:*"
				string startStr = itemString.Substring(0, itemString.IndexOf(':'));
				string endStr = itemString.Substring(itemString.IndexOf(':') + 1,  itemString.Length - itemString.IndexOf(':') - 1);

				this.Start = Int32.Parse(startStr);
                if (endStr.Equals("4294967295"))
                {
                    endStr = "*";
                }

				if (endStr.Trim() == "*") {
					this.ItemType = MessageIDListItemType.UnboundedRange;
				} else {
                    this.ItemType = MessageIDListItemType.BoundedRange;
					this.End = Int32.Parse(endStr);
				}
			} else {
				// A single message number, e.g. "2"				
                this.Start = Int32.Parse(itemString);
                this.ItemType = MessageIDListItemType.Single;
			}
		}

		/// <summary>
		/// Checks if an item number matches this list item.
		/// </summary>
		/// <param name="itemNumber">The number to check.</param>
		/// <returns>True if the number matches.</returns>
		public bool Matches(int itemNumber) {
            switch (this.ItemType)
            {
				case MessageIDListItemType.Single:
					return (itemNumber == this.Start);

				case MessageIDListItemType.BoundedRange:
                    return (itemNumber >= this.Start && itemNumber <= this.End);

				case MessageIDListItemType.UnboundedRange:
                    return (itemNumber >= this.Start);

				default:
					throw new ArgumentException("Invalid item list type.");
			}
		}

        /// <summary>
		/// The type of list item this instance represents.
		/// </summary>
		public MessageIDListItemType ItemType { get; private set; }

        public int Start { get; private set; }

        /// <summary>
        /// The end of the sequence if applicable.
        /// </summary>
        /// <remarks>This will throw an exception if the list item type is not a bounded range.</remarks>
        public int End
        {
            get
            {
                if (this.ItemType != MessageIDListItemType.BoundedRange)
                    throw new ArgumentException("Not a bounded range.");

                return this.end;
            }
            private set
            {
                this.end = value;
            }
        }
        private int end;
	}
}
