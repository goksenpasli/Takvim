using System.Text.RegularExpressions;

namespace iCalNET.Model
{
    public class ContentLine
    {
        private const string ContentLineContentPattern = "(.+?)((;.+?)*):(.+)";

        private const RegexOptions ContentLineContentRegexOptions = RegexOptions.Singleline;

        public ContentLine(string source)
        {
            source = UnfoldAndUnescape(source);
            // TODO Error Handling
            Match match = Regex.Match(source, ContentLineContentPattern, ContentLineContentRegexOptions);
            Name = match.Groups[1].ToString().Trim();
            Parameters = new ContentLineParameters(match.Groups[2].ToString());
            Value = match.Groups[4].ToString().Trim();
        }

        public string Name { get; set; }

        public ContentLineParameters Parameters { get; set; }

        public string Value { get; set; }

        public static string UnfoldAndUnescape(string s)
        {
            string unfold = Regex.Replace(s, "(\\r\\n )", "");
            return Regex.Unescape(unfold);
        }
    }
}