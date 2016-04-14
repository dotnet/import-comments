﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImportComments;

namespace UnitTestProject1
{
    [TestClass]
    public class CommentFormattingTests
    {
        [TestMethod]
        public void InnerIsntLongEnoughTest()
        {
            var xml = @"Initializes a new instance of the <see cref = ""T:System.Globalization.Calendar"" /> class.";

            var expected = @"/// Initializes a new instance of the <see cref = ""T:System.Globalization.Calendar"" /> class.\r\n";
            var actual = CommentFormatting.FormatInnerSummary(xml);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleInnerTest()
        {
            var xml = @"Indicates that the first week of the year begins on the first occurrence of the designated first day of the week on or after the first day of the year. The value is 1.";

            var expected = "/// Indicates that the first week of the year begins on the first occurrence of the designated\r\n/// first day of the week on or after the first day of the year. The value is 1.\r\n";
            var actual = CommentFormatting.FormatInnerSummary(xml);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LongerSimpleInnerTest()
        {
            var xml = @"Searches for the specified character and returns the zero - based index of the first occurrence within the section of the source string that starts at the specified index and contains the specified number of elements.";

            var expected = "/// Searches for the specified character and returns the zero - based index of the first occurrence\r\n/// within the section of the source string that starts at the specified index and contains the specified\r\n/// number of elements.\r\n";
            var actual = CommentFormatting.FormatInnerSummary(xml);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HasAnXmlTagTest()
        {
            var str = @"asdasdasdasdasdasd asd as asd asd asd <see cref = ""blah blah blah"" />";

            Assert.IsTrue(CommentFormatting.HasAnXmlTag(str));
        }
    }
}