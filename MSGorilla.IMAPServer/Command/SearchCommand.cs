using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("SEARCH")]
    public class SearchCommand : BaseCommand, IUIDCommand
    {
        //Todo many many things
        public bool IsUIDCommand { get; set; }

        public List<SearchItem> SearchItems { get; private set; }

        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            SearchItems = new List<SearchItem>();

            List<string> tokens = new List<string>(GetDataTokens());

            while (tokens.Count > 0)
            {
                string token = tokens[0].ToUpper();
                if(token.Contains(':'))
                {
                    this.SearchItems.Add(new SequenceSetSearchItem(token));
                    tokens.RemoveRange(0, 1);
                }
                else if (token.Equals("BCC") ||
                    token.Equals("BEFORE") ||
                    token.Equals("BODY") ||
                    token.Equals("CC") ||
                    token.Equals("FROM") ||
                    token.Equals("KEYWORD") ||
                    token.Equals("LARGER") ||
                    token.Equals("ON") ||
                    token.Equals("SENTBEFORE") ||
                    token.Equals("SENTON") ||
                    token.Equals("SENTSINCE") ||
                    token.Equals("SINCE") ||
                    token.Equals("SMALLER") ||
                    token.Equals("SUBJECT") ||
                    token.Equals("TEXT") ||
                    token.Equals("TO"))
                {
                    if (tokens.Count < 2)
                    {
                        throw new ArgumentException("Require argument for search keyword: " + token);
                    }
                    SearchItem item = new SearchItem();
                    item.SearchType = (SearchItemType)Enum.Parse(typeof(SearchItemType), token, true);
                    item.Argument = tokens[1];
                    this.SearchItems.Add(item);
                    tokens.RemoveRange(0, 2);
                }
                else if (token.Equals("HEADER"))
                {
                    if (tokens.Count < 3)
                    {
                        throw new ArgumentException("Require argument for search keyword: HEADER");
                    }
                    SearchItem item = new SearchItem();
                    item.SearchType = SearchItemType.Header;
                    item.Argument = tokens[1] + " " + tokens[2];
                    this.SearchItems.Add(item);
                    tokens.RemoveRange(0, 3);
                }
                else if(token.Equals("UID"))
                {
                    if (tokens.Count < 2)
                    {
                        throw new ArgumentException("Require argument for search keyword: " + token);
                    }
                    this.SearchItems.Add(new UidSetSearchItem(tokens[1]));
                    tokens.RemoveRange(0, 2);
                }
                else if(token.Equals("NOT") || token.Equals("OR"))
                {
                    throw new ArgumentException("Key word 'not' and 'or' is not supported yet");
                }
                else{
                    SearchItem item = new SearchItem();
                    item.SearchType = (SearchItemType)Enum.Parse(typeof(SearchItemType), token, true);
                    this.SearchItems.Add(item);
                    tokens.RemoveRange(0, 1);
                }
                //switch (tokens[0].ToUpper())
                //{
                //    case "UID":
                //        if (tokens.Count == 1)
                //        {
                //            throw new ArgumentException("UID search items requires a set of messages.");
                //        }
                //        this.SearchItems.Add(new UidSearchItem(tokens[1]));
                //        tokens.RemoveRange(0, 2);
                //        break;

                //    default:
                //        throw new ArgumentException("Invalid item in SEARCH list.");
                //}
            }
        }
    }

    public class SearchItem
    {
        public SearchItemType SearchType { get; set; }

        public string Argument { get; set; }
    }

    public class UidSetSearchItem : SearchItem
    {
        public MessageIDList MessageIDList { get; private set; }
        public UidSetSearchItem(string sequenceSet)
        {
            this.SearchType = SearchItemType.Uid;

            this.MessageIDList = new MessageIDList(sequenceSet);
        }
    }

    public class SequenceSetSearchItem : SearchItem
    {
        public MessageIDList MessageIDList { get; private set; }
        public SequenceSetSearchItem(string sequenceSet)
        {
            this.SearchType = SearchItemType.SequenceSet;
            this.MessageIDList = new MessageIDList(sequenceSet);
        }
    }

    #region SearchItemType
    /// <summary>
    /// The possible types of search items.
    /// </summary>
    public enum SearchItemType
    {
        /// <summary>
        /// All messages in the mailbox.
        /// </summary>
        All,

        /// <summary>
        /// Messages that have the answered flag set.
        /// </summary>
        Answered,

        /// <summary>
        /// The BCC header matches.
        /// </summary>
        Bcc,

        /// <summary>
        /// Sent before the given date (ignoring time and timezone).
        /// </summary>
        Before,

        /// <summary>
        /// The message body matches.
        /// </summary>
        Body,

        /// <summary>
        /// The CC header matches.
        /// </summary>
        CC,

        /// <summary>
        /// The message has the deleted flag set.
        /// </summary>
        Deleted,

        /// <summary>
        /// The message has the draft flag set.
        /// </summary>
        Draft,

        /// <summary>
        /// The message has the flagged flag set.
        /// </summary>
        Flagged,

        /// <summary>
        /// The message is from the given sender.
        /// </summary>
        From,

        /// <summary>
        /// Matches a given header field.
        /// </summary>
        Header,

        /// <summary>
        /// Matches messages with the given keyword set.
        /// </summary>
        Keyword,

        /// <summary>
        /// Matches message larger than a given size.
        /// </summary>
        Larger,

        /// <summary>
        /// The message has the recent flag set but not the seen flag.
        /// </summary>
        New,

        /// <summary>
        /// Negates the related search item (turns it into AND NOT ...).
        /// </summary>
        Not,

        /// <summary>
        /// Messages that don't have the recent flag set.
        /// </summary>
        Old,

        /// <summary>
        /// The message is sent on the given date (ignoring time and timezone).
        /// </summary>
        On,

        /// <summary>
        /// Matches either search key.
        /// </summary>
        Or,

        /// <summary>
        /// The message has the recent flag set.
        /// </summary>
        Recent,

        /// <summary>
        /// Matches a set of message sequence numbers.
        /// </summary>
        SequenceSet,

        /// <summary>
        /// The message has the seen flag set.
        /// </summary>
        Seen,

        /// <summary>
        /// The message was sent before the given date (ignoring time and timezone).
        /// </summary>
        SentBefore,

        /// <summary>
        /// The message was sent on the given date (ignoring time and timezone).
        /// </summary>
        SentOn,

        /// <summary>
        /// The message was sent since the given date (ignoring time and timezone).
        /// </summary>
        SentSince,

        /// <summary>
        /// The message arrived since the given date (ignoring time and timezone).
        /// </summary>
        Since,

        /// <summary>
        /// The message is smaller than the given size.
        /// </summary>
        Smaller,

        /// <summary>
        /// The subject contains the given search string.
        /// </summary>
        Subject,

        /// <summary>
        /// The message contains the given string.
        /// </summary>
        Text,

        /// <summary>
        /// The recipient matches the given string.
        /// </summary>
        To,

        /// <summary>
        /// The Uid version of the sequence set.
        /// </summary>
        Uid,

        /// <summary>
        /// Messages without the answered flag.
        /// </summary>
        Unanswered,

        /// <summary>
        /// Messages without the deleted flag.
        /// </summary>
        Undeleted,

        /// <summary>
        /// Messages without the draft flag.
        /// </summary>
        Undraft,

        /// <summary>
        /// Messages without the flagged flag.
        /// </summary>
        Unflagged,

        /// <summary>
        /// Messages without the given keyword.
        /// </summary>
        Unkeyword,

        /// <summary>
        /// Messages without the seen flag.
        /// </summary>
        Unseen
    }
    #endregion
}
