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
            int limit = 110;
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
                    // Can't split if we're inside of a tag.
                    if (inTag)
                    {
                        var endAndWithinLimit = EndOfTagAndIsWithinLimit(s, i, limit);

                        if (endAndWithinLimit.Item2)
                        {
                            int end = endAndWithinLimit.Item1;

                            if (end < s.Length && IsPunctuation(s[end + 1]))
                            {
                                // Split after the punctuation.  Also +2 because off-by-one when accounting for including punctuation.
                                substrings.Add(s.Substring(start, end + 2 - start).Trim());
                                start += end + 2 - start;

                                // I guess adjust i? ... TODO: make sure this is needed
                                i = start;
                            }
                            else if (end < s.Length)
                            {
                                // +1 to length to substring because off-by-one errors.
                                substrings.Add(s.Substring(start, end + 1 - start).Trim());
                                start += end + 1 - start;
                            }
                            else
                            {
                                // uhhhhhhh this would be weird
                            }
                        }
                        else // The tag exceeds the reasonable limit, so we split at the beginning of the tag.
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
                    }

                    lowerBound += 100;
                    limit += 100;
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
        private static Tuple<int, bool> EndOfTagAndIsWithinLimit(string s, int i, int limit)
        {
            for (; i <= limit; i++)
            {
                if (s[i] == '>')
                {
                    return Tuple.Create(i, true);
                }
            }

            while (s[++i] != '>') ; // Scan until we reach the end of the tag, because we still need that information.

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
