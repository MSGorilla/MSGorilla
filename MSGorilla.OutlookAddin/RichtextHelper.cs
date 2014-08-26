using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MSGorilla.OutlookAddin
{
    /// <summary>
    /// Convert a string into a rich text paragraph with links
    /// </summary>
    public class RichtextHelper
    {
        public class RTBlock
        {
            public LinkType Linktype;
            public string Str;
            public enum LinkType
            {
                None,
                Hyperlink,
                InnerTopiclink,
                InnerUserlink
            }

            public RTBlock(string str, LinkType linktype = LinkType.None)
            {
                this.Str = str;
                this.Linktype = linktype;
            }
        }

        private const string _urlRegex = @"(\s|^)(ht|f)tp(s?)\:\/\/(\S)+(?=\s|$)";
        private const string _topicRegex = @"(\s|^)#([\w][\w \-\.,:&\*\+]*)?[\w]#(?=\s|$)";

        private const string _userRegex = @"(\s|^)@[0-9a-zA-Z\-]+(?=\s|$)";
        private const string _topicUri = "https://msgorilla.cloudapp.net/topic/index?topic={0}&group={1}";
        private const string _userUri = "https://msgorilla.cloudapp.net/profile/index?user={0}";

        public RequestNavigateEventHandler InnerTopicLinkHandler;
        public RequestNavigateEventHandler InnerUserLinkHandler;
        public List<string> RelatedUserIDs;
        public string Group;

        public RichtextHelper(string group, RequestNavigateEventHandler innerLinkHandler)
        {
            this.Group = group;
            this.InnerTopicLinkHandler = innerLinkHandler;
        }

        public Paragraph ParseText(string text)
        {
            List<RTBlock> blocks = ParseInnerTopicLink(text);
            List<RTBlock> ret = new List<RTBlock>();

            foreach (var block in blocks)
            {
                if (block.Linktype != RTBlock.LinkType.None)
                {
                    ret.Add(block);
                    continue;
                }
                ret.AddRange(ParseHyperLink(block.Str));
            }

            blocks = ret;
            ret = new List<RTBlock>();
            foreach (var block in blocks)
            {
                if (block.Linktype != RTBlock.LinkType.None)
                {
                    ret.Add(block);
                    continue;
                }
                ret.AddRange(ParseInnerUserLink(block.Str));
            }

            return ToParagraph(ret);
        }

        private List<RTBlock> ParseHyperLink(string text)
        {
            return ParseLink(text, _urlRegex, RTBlock.LinkType.Hyperlink);
        }

        private List<RTBlock> ParseInnerUserLink(string text)
        {
            if(RelatedUserIDs == null || RelatedUserIDs.Count == 0){
                return new List<RTBlock>() {new RTBlock(text)};
            }

            var temp = from userid in RelatedUserIDs select string.Format("({0})", userid);
            string userRegex = string.Format("@({0})", string.Join("|", temp));
            return ParseLink(text, userRegex, RTBlock.LinkType.InnerUserlink);
        }

        private List<RTBlock> ParseInnerTopicLink(string text)
        {
            return ParseLink(text, _topicRegex, RTBlock.LinkType.InnerTopiclink);
        }

        private List<RTBlock> ParseLink(string text, string regex, RTBlock.LinkType type)
        {
            List<RTBlock> ret = new List<RTBlock>();
            if (string.IsNullOrEmpty(text))
            {
                return ret;
            }

            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            MatchCollection matches = r.Matches(text);

            foreach (Match m in matches)
            {
                string match = m.Value.Trim();
                int start = text.IndexOf(match);
                if (start > 0)
                {
                    ret.Add(new RTBlock(text.Substring(0, start)));
                }
                ret.Add(new RTBlock(match, type));
                text = text.Substring(start + match.Length);
            }
            if (!string.IsNullOrEmpty(text))
            {
                ret.Add(new RTBlock(text));
            }

            return ret;
        }

        private Paragraph ToParagraph(List<RTBlock> blocks)
        {
            Paragraph para = new Paragraph();
            if (blocks == null)
            {
                return para;
            }

            foreach (var block in blocks)
            {
                if (block.Linktype == RTBlock.LinkType.Hyperlink)
                {
                    para.Inlines.Add(Utils.CreateLink(block.Str, block.Str));
                }
                else if (block.Linktype == RTBlock.LinkType.InnerTopiclink)
                {
                    para.Inlines.Add(
                        Utils.CreateLink(block.Str,
                                         string.Format(_topicUri, block.Str.Replace("#", ""), this.Group),
                                         InnerTopicLinkHandler)
                    );
                }
                else if (block.Linktype == RTBlock.LinkType.InnerUserlink)
                {
                    para.Inlines.Add(
                        Utils.CreateLink(block.Str, 
                                            string.Format(_userUri, block.Str.Replace("@", "")), 
                                            InnerUserLinkHandler)
                        );
                }
                else
                {
                    para.Inlines.Add(block.Str);
                }
                para.Inlines.Add(" ");
            }
            return para;
        }
    }
}
