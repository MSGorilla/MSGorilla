using System;

namespace MSGorilla.SearchEngine.Common
{
    public class NoStemming : IStemming
    {
        public string StemWord(string word)
        {
            return word;
        }
    }
}