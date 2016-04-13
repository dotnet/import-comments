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
                    var substrings = GetTrimmedSubstrings(innerXml);
                    return $"/// {string.Join("\r\n/// ", substrings)}\r\n";
                }
            }

            return $"/// {innerXml}\r\n";
        }

        private static List<string> GetTrimmedSubstrings(string s)
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
