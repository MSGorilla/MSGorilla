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
        #region private fields
        private readonly string Separator = Environment.NewLine;
        private readonly double SearchDelaySeconds = 600;

        private CloudTable _wordsIndexTable = null;
        private CloudTable _searchResultsTable = null;
        private CloudTable _searchHistoryTable = null;

        /// <summary>Stemmer to use</summary>
        private IStemming _stemmer = new PorterStemmer();

        /// <summary>Stemmer to use</summary>
        private IStopper _stopper = new ListStopper();

        /// <summary>Go word parser to use</summary>
        private IGoWord _goWord = new ListGoWord();

        /// <summary>Display string: time the search too</summary>
        private TimeSpan _searchTime = TimeSpan.Zero;
        #endregion

        public SearchManager()
        {
            _wordsIndexTable = AzureFactory.GetTable(AzureFactory.MSGorillaTable.WordsIndex);
            _searchResultsTable = AzureFactory.GetTable(AzureFactory.MSGorillaTable.SearchResults);
            _searchHistoryTable = AzureFactory.GetTable(AzureFactory.MSGorillaTable.SearchHistory);
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

            // save counted results into table
            SaveWordsIndex(wordsIndex);
        }

        public SearchResult SearchMessage(string keywords)
        {
            if (string.IsNullOrEmpty(keywords.Trim()))
            {
                return null;
            }

            // segment keywords
            string[] keywordsArray = SplitWords(keywords);
            if (keywordsArray == null || keywordsArray.Count() == 0)
            {
                return null;
            }

            // get search id
            string searchId = GenarateSearchId(keywordsArray);

            // check whether we have to search again
            DateTime now = DateTime.UtcNow;
            DateTime lastSearchDate = DateTime.MinValue;
            int lastSearchCount = 0;

            SearchResult lastSearchResult = GetSearchHistory(searchId);
            if (lastSearchResult != null)
            {
                lastSearchDate = lastSearchResult.SearchDate;
                lastSearchCount = lastSearchResult.ResultsCount;
            }

            if (now.Subtract(lastSearchDate).TotalSeconds < SearchDelaySeconds)
            {   // do not search, use last search result
                return lastSearchResult;
            }

            // search every keyword
            DateTime start = DateTime.Now;  // to show 'time taken' to perform search
            var searchResults = SearchKeywords(keywordsArray, lastSearchDate);

            // Time taken calculation
            Int64 ticks = DateTime.Now.Ticks - start.Ticks;
            _searchTime = new TimeSpan(ticks);

            // save results into table
            SaveSearchResults(searchId, searchResults);

            // update search history
            var currentSearchResult = UpdateSearchHistory(searchId, now, _searchTime.TotalSeconds, searchResults.Count() + lastSearchCount);

            // return search result
            return currentSearchResult;
        }

        public MessagePagination GetSearchResults(string resultId, int count = 25, TableContinuationToken continuationToken = null)
        {
            var mm = new MessageManager();

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                resultId);

            TableQuery<SearchResultEntity> tableQuery = new TableQuery<SearchResultEntity>().Where(query).Take(count);
            TableQuerySegment<SearchResultEntity> queryResult = _searchResultsTable.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (SearchResultEntity entity in queryResult)
            {
                var message = mm.GetMessage(entity.MessageId);
                if (message != null)
                {
                    ret.message.Add(message);
                }
            }
            return ret;
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
                try
                {   // assume rich message is html code
                    HTMLDocumentClass doc = new HTMLDocumentClass();
                    doc.designMode = "on";
                    doc.IHTMLDocument2_write(rich);
                    string richmsg = doc.body.innerText;
                    sb.Append(richmsg + Separator);
                }
                catch
                {
                    try
                    {
                        // TODO: read the rich message in another way
                        sb.Append(StripHtml(rich) + Separator);
                    }
                    catch { }
                }
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

                    WordIndexEntity entity = new WordIndexEntity(word.Text, msg, pos);
                    TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                    _wordsIndexTable.Execute(insertOperation);
                }
            }
        }

        private string GenarateSearchId(string[] keywordsArray)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var s in keywordsArray)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    sb.Append(s + "_");
                }
            }
            return Utils.MD5Encoding(sb.ToString());
        }

        private Dictionary<string, int> SearchKeywords(string[] keywordsArray, DateTime lastSearchDate)
        {
            Dictionary<string, int> output = new Dictionary<string, int>();
            // Array of arrays of results that match ONE of the search criteria
            Dictionary<string, List<int>>[] searchResultsArrayArray = new Dictionary<string, List<int>>[keywordsArray.Length];
            // finalResultsArray is populated with pages that *match* either of the search criteria
            Dictionary<string, int[]> finalResultsArray = new Dictionary<string, int[]>();

            // ##### THE SEARCH #####
            for (int i = 0; i < keywordsArray.Length; i++)
            {
                if (string.IsNullOrEmpty(keywordsArray[i]))
                {
                    searchResultsArrayArray[i] = null;
                }
                else
                {
                    searchResultsArrayArray[i] = SearchWordsIndex(keywordsArray[i], lastSearchDate);
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
                        i[1] += weight;
                        finalResultsArray[mi] = i;
                    }
                }
            }

            // Format the results
            if (finalResultsArray.Count > 0)
            {
                int sortrank = 0;

                // build each result row
                foreach (string mi in finalResultsArray.Keys)
                {
                    sortrank = finalResultsArray[mi][0] * finalResultsArray[mi][1];
                    if (output.ContainsKey(mi))
                    {
                        output[mi] = Math.Max(output[mi], sortrank);
                    }
                    else
                    {
                        output.Add(mi, sortrank);
                    }
                    sortrank = 0;	// reset for next pass
                }
            } // else Count == 0, so output SortedList will be empty

            return output;
        }

        private Dictionary<string, List<int>> SearchWordsIndex(string keyword, DateTime lastSearchDate)
        {
            string query = GenerateWordIndexConditionQuery(keyword, lastSearchDate);

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

        private void SaveSearchResults(string searchId, Dictionary<string, int> searchResults)
        {
            foreach (var key in searchResults.Keys)
            {
                var mi = new MessageIdentity().FromMessageString(key);
                var rank = searchResults[key];

                SearchResultEntity entity = new SearchResultEntity(searchId, rank, mi);
                TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                _searchResultsTable.Execute(insertOperation);
            }
        }

        private SearchResult GetSearchHistory(string searchId)
        {
            TableResult result = _searchHistoryTable.Execute(TableOperation.Retrieve<SearchHistoryEntity>(searchId, searchId));
            SearchHistoryEntity entity = result.Result as SearchHistoryEntity;
            if (entity != null)
            {
                return new SearchResult(entity.ResultId, entity.LastSearchDateUTC, entity.TakenTime, entity.ResultsCount);
            }
            else
            {
                return null;
            }
        }

        private SearchResult UpdateSearchHistory(string searchId, DateTime searchDate, double takenTime, int resultsCount)
        {
            SearchHistoryEntity entity = new SearchHistoryEntity(searchId, searchDate, takenTime, resultsCount);
            TableResult result = _searchHistoryTable.Execute(TableOperation.InsertOrReplace(entity));

            return new SearchResult(searchId, searchDate, takenTime, resultsCount);
        }

        private string GenerateWordIndexConditionQuery(string word, DateTime fromDate)
        {
            var wordStr = word + "_";
            // pk = word_-days
            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.LessThan,
                Utils.NextKeyString(wordStr));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    wordStr
                )
            );

            // rk = -ms_msgid
            var dateStr = Utils.ToAzureStorageSecondBasedString(fromDate) + "_";
            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.NextKeyString(dateStr)
                )
            );

            return query;
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

        /// <summary>
        /// Stripping HTML
        /// http://www.4guysfromrolla.com/webtech/042501-1.shtml
        /// </summary>
        /// <remarks>
        /// Using regex to find tags without a trailing slash
        /// http://concepts.waetech.com/unclosed_tags/index.cfm
        ///
        /// http://msdn.microsoft.com/library/en-us/script56/html/js56jsgrpregexpsyntax.asp
        ///
        /// Replace html comment tags
        /// http://www.faqts.com/knowledge_base/view.phtml/aid/21761/fid/53
        /// </remarks>
        private string StripHtml(string Html)
        {
            //Strips the <script> tags from the Html
            string scriptregex = @"<scr" + @"ipt[^>.]*>[\s\S]*?</sc" + @"ript>";
            System.Text.RegularExpressions.Regex scripts = new System.Text.RegularExpressions.Regex(scriptregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            string scriptless = scripts.Replace(Html, " ");

            //Strips the <style> tags from the Html
            string styleregex = @"<style[^>.]*>[\s\S]*?</style>";
            System.Text.RegularExpressions.Regex styles = new System.Text.RegularExpressions.Regex(styleregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            string styleless = styles.Replace(scriptless, " ");

            //Strips the <NOSEARCH> tags from the Html (where NOSEARCH is set in the web.config/Preferences class)
            //TODO: NOTE: this only applies to INDEXING the text - links are parsed before now, so they aren't "excluded" by the region!! (yet)
            string ignoreless = string.Empty;
            //if (Preferences.IgnoreRegions)
            //{
            //    string noSearchStartTag = "<!--" + Preferences.IgnoreRegionTagNoIndex + "-->";
            //    string noSearchEndTag = "<!--/" + Preferences.IgnoreRegionTagNoIndex + "-->";
            //    string ignoreregex = noSearchStartTag + @"[\s\S]*?" + noSearchEndTag;
            //    System.Text.RegularExpressions.Regex ignores = new System.Text.RegularExpressions.Regex(ignoreregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            //    ignoreless = ignores.Replace(styleless, " ");
            //}
            //else
            {
                ignoreless = styleless;
            }

            //Strips the <!--comment--> tags from the Html	
            //string commentregex = @"<!\-\-.*?\-\->";		// alternate suggestion from antonello franzil 
            string commentregex = @"<!(?:--[\s\S]*?--\s*)?>";
            System.Text.RegularExpressions.Regex comments = new System.Text.RegularExpressions.Regex(commentregex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            string commentless = comments.Replace(ignoreless, " ");

            //Strips the HTML tags from the Html
            System.Text.RegularExpressions.Regex objRegExp = new System.Text.RegularExpressions.Regex("<(.|\n)+?>", RegexOptions.IgnoreCase);

            //Replace all HTML tag matches with the empty string
            string output = objRegExp.Replace(commentless, " ");

            //Replace all _remaining_ < and > with &lt; and &gt;
            output = output.Replace("<", "&lt;");
            output = output.Replace(">", "&gt;");

            objRegExp = null;
            return output;
        }
    }

}
