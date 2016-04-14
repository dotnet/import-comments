using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ImportComments
{
    public static class CommentFormatting
    {
        public static string FormatInnerSummary(string innerXml)
        {
            if (IsLongEnough(innerXml.Length, 90))
            {
                var hastag = HasAnXmlTag(innerXml);

                if (hastag)
                {
                    // we know to be a bit more careful here
                }
                else
                {
                    var substrings = GetSubstringsWithoutXMLTags(innerXml);
                    return $"/// {string.Join("\r\n/// ", substrings)}\r\n";
                }
            }

            return $"/// {innerXml}\r\n";
        }

        private static List<string> GetSubstrings(string s)
        {
            var substrings = new List<string>();

            int start = 0;
            int low = 90;
            bool inTag = false;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '<')
                {
                    inTag = true;
                }

                if (s[i] == '>')
                {
                    inTag = false;
                }

                if (IsLongEnough(i, low) && char.IsWhiteSpace(s[i]))
                {
                    if (inTag)
                    {
                        // if end of tag <= 120 and if what follows isn't punctuation, split at end
                        // if what follows is a period and we're at the end, break out

                        var endOfTagAndIsItOkay = EndOfTagAndIsItOkay(s, i, limit: 120);

                        if (endOfTagAndIsItOkay.Item2)
                        {
                            int end = endOfTagAndIsItOkay.Item1;

                            if (end < s.Length && IsPunctuation(s[end + 1]))
                            {
                                // split after the punctuation
                                substrings.Add(s.Substring(start, end + 1 - start).Trim());
                            }
                        }
                    }

                    substrings.Add(s.Substring(start, i - start).Trim());

                    start += i - start;
                    low += 100;
                }
            }

            substrings.Add(s.Substring(start, s.Length - start).Trim());

            return substrings;
        }

        private static Tuple<int, bool> EndOfTagAndIsItOkay(string s, int i, int limit)
        {
            for (; i <= limit; i++)
            {
                if (s[i] == '>')
                {
                    return Tuple.Create(i, true);
                }
            }

            return Tuple.Create(limit, false);
        }

        private static bool IsPunctuation(char c) => c == '.' || c == ',' || c == '!' || c == '?';

        private static List<string> GetSubstringsWithoutXMLTags(string s)
        {
            var substrings = new List<string>();

            int start = 0;
            int low = 90;

            for (int i = 0; i < s.Length; i++)
            {
                if (IsLongEnough(i, low) && char.IsWhiteSpace(s[i]))
                {
                    substrings.Add(s.Substring(start, i - start).Trim());

                    start += i - start;
                    low += 100;
                }
            }

            substrings.Add(s.Substring(start, s.Length - start).Trim());

            return substrings;
        }

        private static bool IsLongEnough(int i, int lowerBound) => i >= lowerBound;

        public static bool HasAnXmlTag(string xml)
        {
            bool foundStart = false;

            for (int i = 0; i < xml.Length; i++)
            {
                if (xml[i] == '<')
                {
                    foundStart = true;
                }

                if (foundStart && xml[i] == '>') // totally doesn't cover nested tags but I don't think that's a problem here               
                {
                    return true;
                }
            }

            return false;
        }
    }
}
