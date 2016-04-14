using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImportComments;

namespace UnitTestProject1
{
    [TestClass]
    public class CommentFormattingTests
    {
        [TestMethod]
        public void RightAtTheLimitTest()
        {
            var innerXmlComment = "Writes a string followed by a line terminator asynchronously to the text string or stream. ";

            var expected = "/// Writes a string followed by a line terminator asynchronously to the text string or stream.\r\n";
            var actual = CommentFormatting.FormatInnerContent(innerXmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void InnerIsntLongEnoughTest()
        {
            var xmlComment = @"Initializes a new instance of the <see cref = ""T:System.Globalization.Calendar"" /> class.";

            var expected = "/// Initializes a new instance of the <see cref = \"T:System.Globalization.Calendar\" /> class.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleInnerTest()
        {
            var xmlComment = @"Indicates that the first week of the year begins on the first occurrence of the designated first day of the week on or after the first day of the year. The value is 1.";

            var expected = "/// Indicates that the first week of the year begins on the first occurrence of the designated\r\n/// first day of the week on or after the first day of the year. The value is 1.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LongerSimpleInnerTest()
        {
            var xmlComment = @"Searches for the specified character and returns the zero - based index of the first occurrence within the section of the source string that starts at the specified index and contains the specified number of elements.";

            var expected = "/// Searches for the specified character and returns the zero - based index of the first occurrence\r\n/// within the section of the source string that starts at the specified index and contains the specified\r\n/// number of elements.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HasAnXmlTagTest()
        {
            var xmlComment = @"asdasdasdasdasdasd asd as asd asd asd <see cref = ""blah blah blah"" />";

            Assert.IsTrue(CommentFormatting.HasAnXmlTag(xmlComment));
        }

        [TestMethod]
        public void SimpleHasSingleXmlTagInsideTest()
        {
            var xmlComment = @"Initializes a new instance of the <see cref=""T: System.Globalization.RegionInfo"" /> class based on the country/region or specific culture, specified by name.";

            var expected = "/// Initializes a new instance of the <see cref=\"T: System.Globalization.RegionInfo\" /> class\r\n/// based on the country/region or specific culture, specified by name.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SplitLocationInsideTagShouldSplitAtBeginningOfTagTest()
        {
            var xmlComment = @"Determines whether the specified object is the same instance as the current <see cref=""T: System.Globalization.RegionInfo"" />.";

            var expected = "/// Determines whether the specified object is the same instance as the current\r\n/// <see cref=\"T: System.Globalization.RegionInfo\" />.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SplitLocationInsideTagButWithinLimitTest()
        {
            var xmlComment = @"When overridden in a derived class, returns the day of the week in the specified <see cref=""T: System.DateTime"" />.";

            var expected = "/// When overridden in a derived class, returns the day of the week in the specified\r\n/// <see cref=\"T: System.DateTime\" />.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LongerWithTagInsideTest()
        {
            var xmlComment = @"Searches for the specified character and returns the zero-based index of the last occurrence within the entire source string using the specified <see cref=""T: System.Globalization.CompareOptions"" /> value.";

            var expected = "/// Searches for the specified character and returns the zero-based index of the last occurrence\r\n/// within the entire source string using the specified <see cref=\"T: System.Globalization.CompareOptions\" />\r\n/// value.\r\n";
            var actual = CommentFormatting.FormatInnerContent(xmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AnotherTest()
        {
            var innerXmlComment = "The number of characters that have been read. The number will be less than or equal to <paramref name=\"count\" />, depending on whether all input characters have been read.";

            var expected = "/// The number of characters that have been read. The number will be less than or equal to\r\n/// <paramref name=\"count\" />, depending on whether all input characters have been read.\r\n";
            var actual = CommentFormatting.FormatInnerContent(innerXmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test2()
        {
            var innerXmlComment = "The stream does not support writing. For additional information see <see cref=\"P:System.IO.Stream.CanWrite\" />.-or- The current position is at the end of the stream, and the capacity cannot be modified.";

            var expected = "/// The stream does not support writing. For additional information see <see cref=\"P:System.IO.Stream.CanWrite\" />.\r\n/// -or- The current position is at the end of the stream, and the capacity cannot be modified.\r\n";
            var actual = CommentFormatting.FormatInnerContent(innerXmlComment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test3()
        {
            var innerXmlComment = "A task that represents the asynchronous read operation. The value of the <paramref name=\"TResult\" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.";

            var expected = "/// A task that represents the asynchronous read operation. The value of the <paramref name=\"TResult\" />\r\n/// parameter contains the total number of bytes read into the buffer. The result value can\r\n/// be less than the number of bytes requested if the number of bytes currently available is less\r\n/// than the requested number, or it can be 0 (zero) if the end of the stream has been reached.\r\n";

            var actual = CommentFormatting.FormatInnerContent(innerXmlComment);

            Assert.AreEqual(expected, actual);
        }
    }
}
