using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iCalNET.Model
{
    public class CalendarParameters : Dictionary<string, CalendarParameter>
    {
        private const string ParameterPattern = "(.+?):(.+?)(?=\\r\\n[A-Z]|$)";

        private const RegexOptions ParameteRegexOptions = RegexOptions.Singleline;

        public CalendarParameters(string source)
        {
            foreach (Match parametereMatch in Regex.Matches(source, ParameterPattern, ParameteRegexOptions))
            {
                string parameterString = parametereMatch.Groups[0].ToString();
                CalendarParameter calendarParameter = new CalendarParameter(parameterString);
                this[calendarParameter.Name] = calendarParameter;
            }
        }
    }
}