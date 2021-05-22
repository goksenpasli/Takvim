using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iCalNET.Model
{
    public class ContentLineParameters : Dictionary<string, ContentLineParameter>
    {
        private const string ParameterPattern = "([^;]+)(?=;|$)";

        public ContentLineParameters(string source)
        {
            foreach (Match match in Regex.Matches(source, ParameterPattern))
            {
                ContentLineParameter contentLineParameter = new(match.Groups[1].ToString());
                this[contentLineParameter.Name] = contentLineParameter;
            }
        }
    }
}