using System;
using System.Collections.Generic;
using System.Text;

namespace MSGorilla.SearchEngine.Common
{
    public class HtmlAgilityDocument : HtmlDocument
    {
        public HtmlAgilityDocument(Uri location) : base(location) { }
    }
}
