using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using mshtml;


namespace MSGorilla.Library
{
    public class SearchManager
    {
        public void SpideMessage(Message message)
        {
            // convert message to text string
            string msgText = GenarateMessageString(message);

            // segment msg text string to word list

            // count word list 

            // add count result into table

            HTMLDocumentClass doc = new HTMLDocumentClass();
            doc.designMode = "on";
            doc.IHTMLDocument2_write("some html string");
            string richmsg = doc.body.innerText;
        }

        public void Search(string keywords)
        {

        }

        private string GenarateMessageString(Message message)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }
    }
}
