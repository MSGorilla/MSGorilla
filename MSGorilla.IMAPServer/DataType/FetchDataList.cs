using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Helper;
namespace MSGorilla.IMAPServer.DataType
{
    public enum FetchDataItemType
    {
        /// <summary>
        ///  Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE)
        /// </summary>
        All,

        /// <summary>
        /// Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE)
        /// </summary>
        Fast,

        /// <summary>
        /// Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY)
        /// </summary>
        Full,

        /// <summary>
        /// The text of a particular body section.
        /// </summary>
        Body,

        /// <summary>
        /// An alternate form of BODY[<section>] that does not implicitly set the \Seen flag.
        /// </summary>
        BodyPeek,

        /// <summary>
        /// The [MIME-IMB] body structure of the message.
        /// </summary>
        BodyStructure,

        /// <summary>
        /// The envelope structure of the message. 
        /// </summary>
        Envelope,

        /// <summary>
        /// The flags that are set for this message.
        /// </summary>
        Flags,

        /// <summary>
        /// The internal date of the message.
        /// </summary>
        InternalDate,

        /// <summary>
        /// Functionally equivalent to BODY[] ... (RFC822 is returned).
        /// </summary>
        Rfc822,

        /// <summary>
        /// Functionally equivalent to BODY.PEEK[HEADER] ... (RFC822.HEADER is returned).
        /// </summary>
        Rfc822Header,

        /// <summary>
        /// The [RFC-2822] size of the message.
        /// </summary>
        Rfc822Size,

        /// <summary>
        /// Functionally equivalent to BODY[TEXT] ... (RFC822.TEXT is returned).
        /// </summary>
        Rfc822Text,

        /// <summary>
        /// The unique identifier for the message.
        /// </summary>
        Uid
    }

    public class FetchDataList : List<FetchDataItemType>
    {
        public const int MaxTokenCount = 100;
        public FetchDataList(string fetchDataStr)
        {
            fetchDataStr = fetchDataStr.Trim();
            if (fetchDataStr.StartsWith("(") && fetchDataStr.EndsWith(")"))
            {
                fetchDataStr = fetchDataStr.Substring(1, fetchDataStr.Length - 2);
            }

            string[] fetchDataItemsStr = 
                StringHelper.GetQuotedBracketTokens(fetchDataStr, 
                                                    " ".ToCharArray(), 
                                                    MaxTokenCount
                );

            foreach (string dataItemStr in fetchDataItemsStr)
            {
                // For now server doesn's read <section> or <partial> in BODY[<section>]<<partial>>
                // it return all the body when Body or Body.peek founded
                if(dataItemStr.StartsWith("BODY.PEEK", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(FetchDataItemType.BodyPeek);
                }
                else if (dataItemStr.StartsWith("BODYSTRUCTURE", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(FetchDataItemType.BodyStructure);
                }
                else if (dataItemStr.StartsWith("BODY", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(FetchDataItemType.Body);
                }
                else if (dataItemStr.StartsWith("RFC822.HEADER", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(FetchDataItemType.Rfc822Header);
                }
                else if (dataItemStr.StartsWith("RFC822.SIZE", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(FetchDataItemType.Rfc822Size);
                }
                else if (dataItemStr.StartsWith("RFC822.TEXT", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(FetchDataItemType.Rfc822Text);
                }
                else
                {
                    this.Add((FetchDataItemType)Enum.Parse(typeof(FetchDataItemType), dataItemStr, true));
                }
            }
        }
    }
}
