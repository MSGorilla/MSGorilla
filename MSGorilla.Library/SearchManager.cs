using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;
using MSGorilla.SearchEngine;
using MSGorilla.SearchEngine.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using mshtml;
using System.Text.RegularExpressions;


namespace MSGorilla.Library
{
    public class SearchManager
    {
        private readonly string Separator = Environment.NewLine;

        private CloudTable _wordsIndexTable = null;

        /// <summary>Stemmer to use</summary>
        private IStemming _stemmer = new PorterStemmer();

        /// <summary>Stemmer to use</summary>
        private IStopper _stopper = new ListStopper();

        /// <summary>Go word parser to use</summary>
        private IGoWord _goWord = new ListGoWord();

        /// <summary>Display string: time the search too</summary>
        private string _DisplayTime;

        /// <summary>Display string: matches (links and number of)</summary>
        private string _Matches = "";

        public SearchManager()
        {
            _wordsIndexTable = AzureFactory.GetTable(AzureFactory.MSGorillaTable.WordsIndex);
        }

        public void SpideMessage(Message message)
        {
            if (message == null)
            {
                return;
            }

            // convert message to text string
            string msgText = GenarateMessageString(message);

            // segment msg text string to word list
            Dictionary<string, Word> wordsIndex = GenarateWordsIndex(message, msgText);

            // add counted result into table
            SaveWordsIndex(wordsIndex);
        }

        public SortedList SearchMessage(string keywords)
        {
            SortedList output = new SortedList();

            if (string.IsNullOrEmpty(keywords.Trim()))
            {
                return output;
            }

            // segment keywords
            string[] keywordsArray = SplitWords(keywords);
            if (keywordsArray == null || keywordsArray.Count() == 0)
            {
                return output;
            }

            // search every keyword
            output = SearchKeywords(keywordsArray);

            // return result
            return output;
        }

        private string GenarateMessageString(Message message)
        {
            StringBuilder sb = new StringBuilder();

            // add send user
            sb.Append(message.User + Separator);

            // add owner users
            if (message.Owner != null)
            {
                foreach (var u in message.Owner)
                {
                    sb.Append(u + Separator);
                }
            }

            // add at users
            if (message.AtUser != null)
            {
                foreach (var u in message.AtUser)
                {
                    sb.Append(u + Separator);
                }
            }

            // add topics
            if (message.TopicName != null)
            {
                foreach (var t in message.TopicName)
                {
                    sb.Append(t + Separator);
                }
            }

            // add event id
            if (!Utils.IsNullOrEmptyOrNone(message.EventID))
            {
                sb.Append(message.EventID + Separator);
            }

            // add schema id
            if (!Utils.IsNullOrEmptyOrNone(message.SchemaID))
            {
                sb.Append(message.SchemaID + Separator);
            }

            // add msg content
            if (!Utils.IsNullOrEmptyOrNone(message.MessageContent))
            {
                sb.Append(message.MessageContent + Separator);
            }

            // add rich msg
            if (!Utils.IsNullOrEmptyOrNone(message.RichMessageID))
            {
                RichMsgManager rmm = new RichMsgManager();
                string rich = rmm.GetRichMessage(message.RichMessageID);
                HTMLDocumentClass doc = new HTMLDocumentClass();
                doc.designMode = "on";
                doc.IHTMLDocument2_write(rich);
                string richmsg = doc.body.innerText;
                sb.Append(richmsg + Separator);
            }

            // add attached filename
            if (message.AttachmentID != null)
            {
                AttachmentManager am = new AttachmentManager();
                foreach (var a in message.AttachmentID)
                {
                    sb.Append(am.GetAttachmentInfo(a).Filename + Separator);
                }
            }

            return sb.ToString();
        }

        private Dictionary<string, Word> GenarateWordsIndex(Message message, string words)
        {
            MessageIdentity inmessage = new MessageIdentity(message.User, message.ID);
            string[] wordsArray = SplitWords(words);

            int i = 0;   // count of words
            Dictionary<string, Word> wordsIndex = new Dictionary<string, Word>();

            foreach (string key in wordsArray)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    // ### Make sure the Word object is in the index ONCE only
                    if (wordsIndex.ContainsKey(key))
                    {
                        Word theword = (Word)wordsIndex[key];	// get Word from Index, then add this message reference to the Word
                        theword.Add(inmessage, i);
                    }
                    else
                    {
                        Word theword = new Word(key, inmessage, i);	// create a new Word object and add to Index
                        wordsIndex.Add(key, theword);
                    }
                }
                i++;
            }

            return wordsIndex;
        }

        private string[] SplitWords(string words)
        {
            string[] wordsArray = null;

            // COMPRESS ALL WHITESPACE into a single space, seperating words
            if (!String.IsNullOrEmpty(words))
            {
                Regex r = new Regex(@"\s+");            //remove all whitespace
                string compressed = r.Replace(words, " ");
                wordsArray = compressed.Split(' ');
            }
            else
            {
                wordsArray = new string[0];
            }

            int i = 0;
            string key = "";    // temp variables
            foreach (string word in wordsArray)
            {
                key = word.ToLower();
                if (!_goWord.IsGoWord(key))
                {	// not a special case, parse like any other word
                    RemovePunctuation(ref key);

                    if (!IsNumber(ref key))
                    {
                        // not a number, so get rid of numeric seperators and catalog as a word
                        // TODO: remove inline punctuation, split hyphenated words?
                        // http://blogs.msdn.com/ericgu/archive/2006/01/16/513645.aspx
                        key = System.Text.RegularExpressions.Regex.Replace(key, "[,.]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        // Apply Stemmer 
                        key = _stemmer.StemWord(key);

                        // Apply Stopper 
                        key = _stopper.StopWord(key);
                    }
                }
                else
                {
                }

                wordsArray[i] = key;
                i++;
            }

            return wordsArray;
        }

        private void SaveWordsIndex(Dictionary<string, Word> wordsIndex)
        {
            foreach (var key in wordsIndex.Keys)
            {
                var word = wordsIndex[key] as Word;

                var msgWithPos = word.InMessagesWithPosition();
                foreach (var m in msgWithPos)
                {
                    var msg = new MessageIdentity();
                    msg.FromMessageString(m.Key);
                    var pos = m.Value;

                    WordIndexEntity entity = new WordIndexEntity(word.Text, msg.UserId, msg.MessageId, pos);
                    TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                    _wordsIndexTable.Execute(insertOperation);
                }
            }
        }

        private Dictionary<string, List<int>> SearchWordsIndex(string keyword)
        {
            string query = GeneratePKStartWithConditionQuery(keyword + "_");

            TableQuery<WordIndexEntity> tableQuery = new TableQuery<WordIndexEntity>().Where(query);
            var queryResult = _wordsIndexTable.ExecuteQuery<WordIndexEntity>(tableQuery);

            Dictionary<string, List<int>> ret = new Dictionary<string, List<int>>();
            foreach (var entity in queryResult)
            {
                var msgid = entity.GetMessage().ToMessageString();
                var poslist = entity.GetPostionsList();

                if (ret.ContainsKey(msgid))
                {
                    var l = ret[msgid];
                    ret[msgid] = l.Union(poslist).ToList();
                }
                else
                {
                    ret.Add(msgid, poslist);
                }
            }

            return ret;
        }

        private string GeneratePKStartWithConditionQuery(string startWith)
        {
            if (!Utils.IsValidID(startWith))
            {
                throw new InvalidIDException();
            }

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.LessThan,
                Utils.NextKeyString(startWith));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startWith
                )
            );
            return query;
        }

        private SortedList SearchKeywords(string[] keywordsArray)
        {
            SortedList output = new SortedList();
            // Array of arrays of results that match ONE of the search criteria
            Dictionary<string, List<int>>[] searchResultsArrayArray = new Dictionary<string, List<int>>[keywordsArray.Length];
            // finalResultsArray is populated with pages that *match* either of the search criteria
            Dictionary<string, int[]> finalResultsArray = new Dictionary<string, int[]>();
            DateTime start = DateTime.Now;  // to show 'time taken' to perform search

            // ##### THE SEARCH #####
            for (int i = 0; i < keywordsArray.Length; i++)
            {
                if (string.IsNullOrEmpty(keywordsArray[i]))
                {
                    searchResultsArrayArray[i] = null;
                }
                else
                {
                    searchResultsArrayArray[i] = SearchWordsIndex(keywordsArray[i]);
                }
            }

            // merge arraylist
            foreach (var searchResultsArray in searchResultsArrayArray)
            {
                foreach (string mi in searchResultsArray.Keys)
                {
                    int weight = searchResultsArray[mi].Count;
                    if (!finalResultsArray.ContainsKey(mi))
                    {   // new add

                        finalResultsArray.Add(mi, new int[2] { 1, weight });
                    }
                    else
                    {   // modify
                        int[] i = finalResultsArray[mi];
                        i[0]++;
                        i[1] = i[0] * i[1] + weight;
                        finalResultsArray[mi] = i;
                    }
                }
            }

            // Time taken calculation
            Int64 ticks = DateTime.Now.Ticks - start.Ticks;
            TimeSpan taken = new TimeSpan(ticks);
            if (taken.Seconds > 1)
            {
                _DisplayTime = taken.Seconds + " seconds";
            }
            else if (taken.TotalMilliseconds > 1)
            {
                _DisplayTime = Convert.ToInt32(taken.TotalMilliseconds) + " milliseconds";
            }
            else
            {
                _DisplayTime = "less than 1 millisecond";
            }

            // Format the results
            if (finalResultsArray.Count > 0)
            {	// intermediate data-structure for 'ranked' result HTML
                //SortedList 
                output = new SortedList(finalResultsArray.Count); // empty sorted list
                int sortrank = 0;

                // build each result row
                foreach (string mi in finalResultsArray.Keys)
                {
                    sortrank = finalResultsArray[mi][1] * -1000;		// Assume not 'thousands' of results
                    if (output.Contains(sortrank))
                    { // rank exists - drop key index one number until it fits
                        for (int i = 1; i < 999; i++)
                        {
                            sortrank++;
                            if (!output.Contains(sortrank))
                            {
                                MessageIdentity m = new MessageIdentity();
                                m.FromMessageString(mi);
                                output.Add(sortrank, m);
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageIdentity m = new MessageIdentity();
                        m.FromMessageString(mi);
                        output.Add(sortrank, m);
                    }
                    sortrank = 0;	// reset for next pass
                }

            } // else Count == 0, so output SortedList will be empty

            return output;
        }

        /// <summary>
        /// Each word is identified purely by the whitespace around it. It could still include punctuation
        /// attached to either end of the word, or "in" the word (ie a dash, which we will remove for
        /// indexing purposes)
        /// </summary>
        /// <remarks>
        /// Andrey Shchekin suggests 'unicode' regex [\w] - equivalent to [\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Pc}]
        /// http://www.codeproject.com/cs/internet/Searcharoo_4.asp?df=100&forumid=397394&select=1992575#xx1992575xx
        /// so [^\w0-9,.] as a replacement for [^a-z0-9,.]
        /// which might remove the need for 'AssumeAllWordsAreEnglish'. TO BE TESTED.
        /// </remarks>
        private void RemovePunctuation(ref string word)
        {
            // this stuff is a bit 'English-language-centric'
            // if all words are english, this strict parse to remove all punctuation ensures
            // words are reduced to their least unique form before indexing
            // Assume All Words Are English
            word = System.Text.RegularExpressions.Regex.Replace(word, @"[^\w0-9,.]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // by stripping out this specific list of punctuation only, there is potential to leave lots 
            // of cruft in the word before indexing BUT this will allow any language to be indexed
            // word = word.Trim(' ', '?', '\"', ',', '\'', ';', ':', '.', '(', ')', '[', ']', '%', '*', '$', '-');
        }

        /// <summary>
        /// TODO: parse numbers here 
        /// ie remove thousands seperator, currency, etc
        /// and also trim decimal part, so number searches are only on the integer value
        /// </summary>
        private bool IsNumber(ref string word)
        {
            try
            {
                long number = Convert.ToInt64(word); //;int.Parse(word);
                word = number.ToString();
                return (word != String.Empty);
            }
            catch
            {
                return false;
            }
        }
    }

}
