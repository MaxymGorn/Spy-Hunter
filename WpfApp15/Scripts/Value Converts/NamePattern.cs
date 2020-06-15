using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MaxsGorn
{
    public delegate void PatternEventHandler(object sender, EventArgs e);

    public class NamePattern
    {
        public event PatternEventHandler OnValidPattern;
        public event PatternEventHandler OnInvalidPattern;
        public event PatternEventHandler OnNumberIncremented;

        private delegate string code2text();
        Dictionary<string, code2text> dicCodeTable = new Dictionary<string, code2text>();

        public int Number { get; set; } = 1;

        private string _pattern = string.Empty;
        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set
            {
                _pattern = value;
                if (Verify())
                    OnValidPattern?.Invoke(this, new EventArgs());
                else
                    OnInvalidPattern?.Invoke(this, new EventArgs());
            }
        }

        public NamePattern(string pattern = "", int number = 1)
        {
            Pattern = pattern;
            Number = number;
            initCodeTable();
        }

        private void initCodeTable()
        {
            var dic = dicCodeTable;
            dic.Add("<day>", getDay);
            dic.Add("<week>", getWeek);
            dic.Add("<month>", getMonth);
            dic.Add("<year>", getYear);
            dic.Add("<second>", getSecond);
            dic.Add("<minute>", getMinute);
            dic.Add("<hour>", getHour);
            dic.Add("<#>", getNumber);
        }

        private string getDay() { return DateTime.Now.Day.ToString(); }

        private string getWeek() { return DateTime.Now.DayOfWeek.ToString(); }

        private string getMonth() { return DateTime.Now.Month.ToString(); }

        private string getYear() { return DateTime.Now.Year.ToString(); }

        private string getSecond() { return DateTime.Now.Second.ToString(); }

        private string getMinute() { return DateTime.Now.Minute.ToString(); }

        private string getHour() { return DateTime.Now.Hour.ToString(); }

        private string getNumber() { return Number.ToString(); }

        public bool Verify(string pattern = "")
        {
            return _verify(pattern == "" ? Pattern : pattern);
        }

        private bool _verify(string pattern)
        {
            pattern = _convert(pattern);
            return (Regex.Matches(pattern, "[\\/:*?\"<>|]").Count == 0)
                && (pattern != "") ? true : false;
        }

        public string Convert(string pattern = "")
        {
            return _convert(pattern == "" ? Pattern : pattern);
        }

        private string _convert(string pattern)
        {
            // Pad number convertion
            pattern = convertPadNumber(pattern);

            // Normal convertion
            var ms = Regex.Matches(pattern, @"<[^<>]*>");
            foreach (var m in ms)
            {
                var key = m.ToString().ToLower();
                if (dicCodeTable.ContainsKey(key))
                    pattern = pattern.Replace(m.ToString(), dicCodeTable[key].Invoke());
            }
            return pattern;
        }

        private string convertPadNumber(string pattern)
        {
            var ms = Regex.Matches(pattern, @"<\d+#>");
            foreach (var m in ms)
            {
                var number = int.Parse(m.ToString().Substring(1, m.ToString().Length - 3));
                pattern = pattern.Replace(m.ToString(), Number.ToString().PadLeft(number, '0'));
            }
            return pattern;
        }

        public void IncrementNumber() { Number++; OnNumberIncremented?.Invoke(this, new EventArgs()); }

        public HashSet<string> GetVariables()
        {
            var has = new HashSet<string>();
            foreach (var pair in dicCodeTable)
                has.Add(pair.Key);
            return has;
        }
    }
}
