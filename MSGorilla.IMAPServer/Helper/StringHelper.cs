using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace MSGorilla.IMAPServer.Helper
{
    public static class StringHelper
    {
        public static string[] GetTokens(string src, 
            int maximumTokens = 100, 
            bool enforceSingleSpace = false)
        {
            // This will split into one or more space delimited, respecting quotes

            int tokenStart = 0, tokenEnd = 0, index = 0;
            StringCollection tokens = new StringCollection();
            bool quoted = false;

            char[] chars = src.ToCharArray();

            while (tokens.Count < maximumTokens && index < src.Length)
            {
                while (chars[index] == ' ')
                {
                    // Find the start of the token
                    index++;
                    if (enforceSingleSpace) break;
                }

                if (index >= src.Length)
                {
                    break;
                }

                if (index > 0 && chars[index - 1] != ' ')
                {
                    throw new ApplicationException("Invalid String");
                }

                // Is this string quoted
                quoted = chars[index] == '"';

                if (quoted)
                {
                    // This is a Quoted token, skip the quotes
                    index++;
                }

                tokenStart = index;
                tokenEnd = chars.Length;

                while (index < src.Length)
                {
                    if (quoted)
                    {
                        if (chars[index] == '"')
                        {
                            index++;
                            if (chars[index - 2] == '\\')
                            {
                                continue;
                            }
                            tokenEnd = index - 1;
                            quoted = false;
                            break;
                        }
                    }
                    else if (chars[index] == ' ')
                    {
                        tokenEnd = index;
                        break;
                    }
                    index++;
                }

                if (quoted)
                {
                    // No matching quote found.
                    throw new ApplicationException("Invalid String");
                }

                tokens.Add(src.Substring(tokenStart, tokenEnd - tokenStart));
            }

            string[] result = new string[tokens.Count];

            tokens.CopyTo(result, 0);

            return result;
        }




        /// <summary>
        /// Gets a list of tokens respecting quotes.
        /// </summary>
        /// <example>
        /// 'This "is a" string' would split into the following tokens: 'This' 'is a' 'string'.
        /// </example>
        /// <param name="data">The data to tokenise.</param>
        /// <param name="delimiters">The delimiter to tokenise on.</param>
        /// <param name="maximumTokens">The maximum number of tokens to return.</param>
        /// <param name="removeQuotes">Should quotes be removed from the extracted tokens.</param>
        /// <returns>The array of tokens.</returns>
        public static string[] GetQuotedTokens(string data, char[] delimiters, int maximumTokens = 100, bool removeQuotes = true)
        {
            int tokenStart = 0, tokenEnd = 0, index = 0;
            bool quoted = false;
            StringCollection tokens = new StringCollection();

            char[] chars = data.ToCharArray();

            while (tokens.Count < maximumTokens && index < data.Length)
            {
                // Skip to the end of the current consequtive delimiters
                while (index < data.Length)
                {
                    bool isDelimiter = false;

                    for (int i = 0; i < delimiters.Length; i++)
                    {
                        if (delimiters[i] == chars[index])
                        {
                            isDelimiter = true;
                            break;
                        }
                    }

                    if (isDelimiter)
                    {
                        // Still a delimiter
                        index++;
                    }
                    else
                    {
                        // Start of the token
                        break;
                    }
                }

                if (index >= data.Length)
                {
                    // End of the string
                    break;
                }

                tokenStart = index;
                tokenEnd = chars.Length;

                // Skip to the next delimiter outside of quotes
                while (index < data.Length)
                {
                    if (quoted)
                    {
                        if (chars[index] == '"')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                quoted = false;
                            }
                        }
                    }
                    else
                    {
                        if (chars[index] == '"')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                quoted = true;
                            }
                        }
                        else
                        {
                            bool isDelimiter = false;

                            for (int i = 0; i < delimiters.Length; i++)
                            {
                                if (delimiters[i] == chars[index])
                                {
                                    isDelimiter = true;
                                    break;
                                }
                            }

                            if (isDelimiter)
                            {
                                tokenEnd = index;
                                break;
                            }
                        }
                    }

                    index++;
                }

                string currentToken = data.Substring(tokenStart, tokenEnd - tokenStart);
                if (removeQuotes)
                {
                    currentToken = UnQuoteString(currentToken);
                }
                tokens.Add(currentToken);
            }

            string[] result = new string[tokens.Count];

            tokens.CopyTo(result, 0);

            return result;
        }


        /// <summary>
        /// Removes leading and trailing quotes from the given string.
        /// </summary>
        /// <example>
        /// '  "Quoted string  "' will return 'Quoted string  '
        /// </example>
        /// <param name="input">The string to dequote</param>
        /// <returns>The string with quotes removed.</returns>
        public static string UnQuoteString(string input)
        {
            input = input.Trim();

            if (input[0] == '"')
            {
                input = input.Substring(1, input.Length - 1);
            }

            if (input.Length > 0 && input[input.Length - 1] == '"')
            {
                input = input.Substring(0, input.Length - 1);
            }

            return input;
        }


        /// <summary>
        /// Gets a list of tokens respecting quotes and brackets.
        /// </summary>
        /// <example>
        /// 'This "is a" (very long) string' would split into the following tokens: 'This' 'is a' 'very long' 'string'.
        /// </example>
        /// <param name="data">The data to tokenise.</param>
        /// <param name="delimiters">The delimiter to tokenise on.</param>
        /// <param name="maximumTokens">The maximum number of tokens to return.</param>
        /// <returns>The array of tokens.</returns>
        public static string[] GetQuotedBracketTokens(string data, char[] delimiters, int maximumTokens = 100)
        {
            // This will split into one or more space delimited, respecting quotes

            int tokenStart = 0, tokenEnd = 0, index = 0;
            StringCollection tokens = new StringCollection();
            bool quoted = false;
            bool squareBrackets = false;
            bool curvedBrackets = false;
            bool curlyBrackets = false;

            char[] chars = data.ToCharArray();

            while (tokens.Count < maximumTokens && index < data.Length)
            {
                // Skip to the end of the current consequtive delimiters
                while (index < data.Length)
                {
                    bool isDelimiter = false;

                    for (int i = 0; i < delimiters.Length; i++)
                    {
                        if (delimiters[i] == chars[index])
                        {
                            isDelimiter = true;
                            break;
                        }
                    }

                    if (isDelimiter)
                    {
                        // Still a delimiter
                        index++;
                    }
                    else
                    {
                        // Start of the token
                        break;
                    }
                }

                if (index >= data.Length)
                {
                    // End of the string
                    break;
                }

                tokenStart = index;
                tokenEnd = chars.Length;

                while (index < data.Length)
                {
                    if (quoted)
                    {
                        if (chars[index] == '"')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                quoted = false;
                            }
                        }
                    }
                    else if (squareBrackets)
                    {
                        if (chars[index] == ']')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                squareBrackets = false;
                            }
                        }
                    }
                    else if (curvedBrackets)
                    {
                        if (chars[index] == ')')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                curvedBrackets = false;
                            }
                        }
                    }
                    else if (curlyBrackets)
                    {
                        if (chars[index] == '}')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                curlyBrackets = false;
                            }
                        }
                    }
                    else
                    {
                        if (chars[index] == '"')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                quoted = true;
                            }
                        }
                        else if (chars[index] == '[')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                squareBrackets = true;
                            }
                        }
                        else if (chars[index] == '(')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                curvedBrackets = true;
                            }
                        }
                        else if (chars[index] == '(')
                        {
                            if (index == 0 || chars[index - 1] != '\\')
                            {
                                curlyBrackets = true;
                            }
                        }
                        else
                        {
                            bool isDelimiter = false;

                            for (int i = 0; i < delimiters.Length; i++)
                            {
                                if (delimiters[i] == chars[index])
                                {
                                    isDelimiter = true;
                                    break;
                                }
                            }

                            if (isDelimiter)
                            {
                                tokenEnd = index;
                                break;
                            }
                        }
                    }

                    index++;
                }

                if (quoted || squareBrackets || curvedBrackets || curlyBrackets)
                {
                    // No matching quote found.
                    throw new ApplicationException("Invalid String");
                }

                tokens.Add(data.Substring(tokenStart, tokenEnd - tokenStart));
            }

            string[] result = new string[tokens.Count];

            tokens.CopyTo(result, 0);

            return result;
        }
    }
}
