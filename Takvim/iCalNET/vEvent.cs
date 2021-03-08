using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iCalNET.Model
{
    public class VEvent
    {
        private const string ContentLinePattern = "(.+?):(.+?)(?=\\r\\n[A-Z]|$)";

        private const RegexOptions ContentLineTRegexOptions = RegexOptions.Singleline;

        private const string vEventContentPattern = "BEGIN:VEVENT\\r\\n(.+)\\r\\nEND:VEVENT";

        private const RegexOptions vEventContentRegexOptions = RegexOptions.Singleline;

        public VEvent(string source)
        {
            Match contentMatch = Regex.Match(source, vEventContentPattern, vEventContentRegexOptions);
            string content = contentMatch.Groups[1].ToString();
            MatchCollection matches = Regex.Matches(content, ContentLinePattern, ContentLineTRegexOptions);
            ContentLines = new Dictionary<string, ContentLine>();
            foreach (Match match in matches)
            {
                string contentLineString = match.Groups[0].ToString();
                ContentLine contentLine = new(contentLineString);
                ContentLines[contentLine.Name] = contentLine;
            }
        }

        public Dictionary<string, ContentLine> ContentLines { get; set; }
    }
}