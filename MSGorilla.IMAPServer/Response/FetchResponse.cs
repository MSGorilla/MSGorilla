using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.DataType;

namespace MSGorilla.IMAPServer.Response
{
    public class FetchDataItem
    {
        private static Dictionary<FetchDataItemType, string> table;

        static FetchDataItem()
        {
            table = new Dictionary<FetchDataItemType, string>();
            table.Add(FetchDataItemType.Body, "BODY[]");
            table.Add(FetchDataItemType.BodyPeek, "BODY.PEEK[]");
            table.Add(FetchDataItemType.BodyStructure, "BODYSTRUCTURE");
            table.Add(FetchDataItemType.Envelope, "ENVELOPE");
            table.Add(FetchDataItemType.Flags, "FLAGS");
            table.Add(FetchDataItemType.InternalDate, "INTERNALDATE");
            table.Add(FetchDataItemType.Rfc822, "RFC822");
            table.Add(FetchDataItemType.Rfc822Header, "RFC822.HEADER");
            table.Add(FetchDataItemType.Rfc822Size, "RFC822.SIZE");
            table.Add(FetchDataItemType.Rfc822Text, "RFC822.TEXT");
            table.Add(FetchDataItemType.Uid, "UID");
        }

        public static string FetchDataItemType2String(FetchDataItemType type)
        {
            return table[type];
        }

        public FetchDataItem(FetchDataItemType type = FetchDataItemType.BodyPeek)
        {
            this.Type = type;
        }

        public FetchDataItem(FetchDataItemType type = FetchDataItemType.BodyPeek, string content = "")
        {
            this.Type = type;
            this.Text = content;
        }

        public FetchDataItemType Type;
        public string Text;
        public override string ToString()
        {
            return string.Format("{0} {1}", FetchDataItemType2String(this.Type), this.Text);
        }
    }
    public class FetchResponse : BaseResponse
    {
        public int SequenceNumber;
        public List<FetchDataItem> Items;

        public FetchResponse(int seqNumber)
        {
            this.SequenceNumber = seqNumber;
            this.Items = new List<FetchDataItem>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("* ");
            sb.Append(this.SequenceNumber);
            sb.Append(" FETCH (");

            if (Items != null && Items.Count > 0)
            {
                for (int i = 0; i < Items.Count - 1; i++)
                {
                    sb.Append(Items[i].ToString());
                    sb.Append(" ");
                }
                sb.Append(Items[Items.Count - 1].ToString());
            }

            sb.Append(")");
            return sb.ToString();
        }
    }
}
