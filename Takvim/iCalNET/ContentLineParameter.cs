﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace iCalNET.Model
{
    public class ContentLineParameter
    {
        private const string NameValuePattern = "(.+?)=(.+)";

        private const string ValueListPattern = "([^,]+)(?=,|$)";

        public ContentLineParameter(string source)
        {
            Match match = Regex.Match(source, NameValuePattern);
            Name = match.Groups[1].ToString().Trim();
            string valueString = match.Groups[2].ToString();
            foreach (Match paramMatch in Regex.Matches(valueString, ValueListPattern))
            {
                Values.Add(paramMatch.Groups[1].ToString().Trim());
            }
        }

        public string Name { get; set; }

        public List<string> Values { get; set; } = new List<string>();
    }
}