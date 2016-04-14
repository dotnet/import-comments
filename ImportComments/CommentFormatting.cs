using System;
using System.Collections.Generic;

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
                    var substrings = GetSubstrings(innerXml);
                    return $"/// {string.Join("\r\n/// ", substrings)}\r\n";
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
            int lowerBound = 90;
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

                if (IsLongEnough(i, lowerBound) && char.IsWhiteSpace(s[i]))
                {
                    if (inTag)
                    {
                        var endAndOkay = EndOfTagAndIsItOkay(s, i, limit: 120);

                        if (endAndOkay.Item2)
                        {
                            int end = endAndOkay.Item1;

                            if (end < s.Length && IsPunctuation(s[end + 1]))
                            {
                                // Split after the punctuation.
                                substrings.Add(s.Substring(start, end + 2 - start).Trim());
                                start += end + 2 - start;

                                // I guess adjust i?
                                i = start;
                            }
                        }
                        else // The tag exceeds our limit of 120 chars, so we split at the beginning of the tag.
                        {
                            int beginOfTag = BeginningOfTagIndex(s, i);
                            substrings.Add(s.Substring(start, beginOfTag - start).Trim());

                            start += beginOfTag - start;
                        }
                    }
                    else
                    {
                        substrings.Add(s.Substring(start, i - start).Trim());

                        start += i - start;
                        lowerBound += 100;
                    }
                }
            }

            // for the case when the comment should only be one line, and is within the limit
            if (start < s.Length)
            {
                substrings.Add(s.Substring(start, s.Length - start).Trim());
            }

            return substrings;
        }

        private static int BeginningOfTagIndex(string s, int i)
        {
            while (s[--i] != '<') ;
            return i;
        }

        // Boy it sure would be nice to have those C# 7 tuples.
        private static Tuple<int, bool> EndOfTagAndIsItOkay(string s, int i, int limit)
        {
            for (; i <= limit; i++)
            {
                if (s[i] == '>')
                {
                    return Tuple.Create(i, true);
                }
            }

            while (s[++i] != '>') ; // Scan until we reach the end of the tag.

            return Tuple.Create(i, false);
        }

        private static bool IsPunctuation(char c) => c == '.' || c == ',' || c == '!' || c == '?';

        private static List<string> GetSubstringsWithoutXMLTags(string s)
        {
            var substrings = new List<string>();

            int start = 0;
            int lowerBound = 90;

            for (int i = 0; i < s.Length; i++)
            {
                if (IsLongEnough(i, lowerBound) && char.IsWhiteSpace(s[i]))
                {
                    substrings.Add(s.Substring(start, i - start).Trim());

                    start += i - start;
                    lowerBound += 100;
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
