using System;
using System.Collections.Generic;

namespace ImportComments
{
    public static class CommentFormatting
    {
        public static string FormatInnerContent(string innerXml)
        {
            // There are a few cases where there's leading or trailing whitespace, so let's get rid of that.
            innerXml = innerXml.Trim();

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
            int sliceLength = 0;
            bool inTag = false;

            for (int i = 0; i < s.Length; i++)
            {
                sliceLength += 1;

                if (s[i] == '<')
                {
                    inTag = true;
                }

                if (s[i] == '>')
                {
                    inTag = false;
                }

                if (IsLongEnough(sliceLength, lowerBound) && char.IsWhiteSpace(s[i]))
                {
                    // Can't split if we're inside of a tag.
                    if (inTag)
                    {
                        HandleTag(s, substrings, ref start, limit, i);
                    }
                    else
                    {
                        substrings.Add(s.Substring(start, i - start).Trim());
                        start += i - start;   
                    }

                    limit += 100;
                    sliceLength = 0; // reset the counter for slice length, as we're going to be looking at a new slice now.
                }
            }

            // for the case when the comment should only be one line, and is within the limit
            if (start < s.Length)
            {
                substrings.Add(s.Substring(start, s.Length - start).Trim());
            }

            return substrings;
        }

        private static void HandleTag(string s, List<string> substrings, ref int start, int limit, int i)
        {
            var endAndWithinLimit = EndOfTagAndIsWithinLimit(s, i, limit);

            if (endAndWithinLimit.Item2)
            {
                int end = endAndWithinLimit.Item1;

                if (end + 1 < s.Length && IsPunctuation(s[end + 1]))
                {
                    // Split after the punctuation.  Also +2 because off-by-one when accounting for including punctuation.
                    substrings.Add(s.Substring(start, end + 2 - start).Trim());
                    start += end + 2 - start;
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

            while (s[i] != '>')
            {
                i++;
            }

            return Tuple.Create(i, false);
        }

        private static bool IsPunctuation(char c) => c == '.' || c == ',' || c == '!' || c == '?' || c == ';';

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

            if (start < s.Length)
            {
                substrings.Add(s.Substring(start, s.Length - start).Trim());
            }

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
