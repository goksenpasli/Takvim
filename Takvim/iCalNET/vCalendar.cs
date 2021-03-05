using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iCalNET.Model
{
    public class vCalendar
    {
        public const string vEventPattern = "(BEGIN:VEVENT.+?END:VEVENT)";

        public const RegexOptions vEventRegexOptions = RegexOptions.Singleline;

        private const string CalendarParameterPattern = "BEGIN:VCALENDAR\\r\\n(.+?)\\r\\nBEGIN:VEVENT";

        private const RegexOptions CalendarParameterRegexOptions = RegexOptions.Singleline;

        public vCalendar(string source)
        {
            Source = source;
            Match parameterMatch = Regex.Match(source, CalendarParameterPattern, CalendarParameterRegexOptions);
            string parameterString = parameterMatch.Groups[1].ToString();
            Parameters = new CalendarParameters(parameterString);
            foreach (Match vEventMatch in Regex.Matches(source, vEventPattern, vEventRegexOptions))
            {
                string vEventString = vEventMatch.Groups[1].ToString();
                vEvents.Add(new vEvent(vEventString));
            }
        }

        public CalendarParameters Parameters { get; set; }

        public string Source { get; set; }

        public List<vEvent> vEvents { get; set; } = new List<vEvent>();
    }
}