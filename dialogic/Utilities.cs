﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("DialogicTest")]
[assembly: InternalsVisibleTo("DialogicEditor")]

namespace Dialogic
{
    /// <summary>
    /// A (temporary) container for runtime-settable defaults
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default scale for all Command durations
        /// </summary>
        public static double GLOBAL_TIME_SCALE = 1.0;

        /// <summary>
        /// Default Command duration for Say
        /// </summary>
        public static double SAY_DURATION = 2.0;

        /// <summary>
        /// Default Command duration for Do
        /// </summary>
        public static double DO_DURATION = 0.02;

        /// <summary>
        /// Default Command duration for Ask
        /// </summary>
        public static double ASK_DURATION = Util.INFINITE;

        /// <summary>
        /// Default Command duration for Wait
        /// </summary>
        public static double WAIT_DURATION = Util.INFINITE;

        /// <summary>
        /// Default Timeout for Ask
        /// </summary>
        public static double ASK_TIMEOUT = 5.0;

        /// <summary>
        /// Default staleness-threshold for Find
        /// </summary>
        public static double FIND_STALENESS = 10;

        /// <summary>
        /// Amount to relax Find staleness threshold each time
        /// </summary>
        public static double FIND_RELAXATION_INCR = .5;

        /// <summary>
        /// Max staleness-threshold to try for a Find
        /// </summary>
        public static double FIND_MAX_STALENESS = 50;

        /// <summary>
        /// Whether scores should be normalized (0-1) on Finds
        /// </summary>
        public static bool FIND_NORMALIZE_SCORES = true;

        /// <summary>
        /// Default staleness for new Chats
        /// </summary>
        public static double CHAT_STALENESS = 0;

        /// <summary>
        /// Default increment each time Chat is run
        /// </summary>
        public static double CHAT_STALENESS_INCR = 1;

        /// <summary>
        /// Whether to auto-launch 'onResume' smoothing Chats when specified
        /// </summary>
        public static bool CHAT_ENABLE_SMOOTHING = true;

        /// <summary>
        ///  Max recursion depth for Resolver bindings
        /// </summary>
        public static int BIND_MAX_DEPTH = 10;

        // Default Timing fields for Say, Ask, Opt
        public static double SAY_FAST_MULT = 0.5;
        public static double SAY_SLOW_MULT = 2.0;
        public static double SAY_MAX_LEN_MULT = 2.0;
        public static double SAY_MIN_LEN_MULT = 0.5;
        public static int SAY_MAX_LINE_LEN = 80;
        public static int SAY_MIN_LINE_LEN = 2;

        /// <summary>
        /// The initial size of the meta and resolved Command dictionaries (should be prime).
        /// </summary>
        public static int INITIAL_DICT_SIZE = 11;
    }

    /// <summary>
    /// A one-to-one mapping from a string command to its object type, e.g., "SAY" -> Dialogic.Say
    /// </summary>
    public class CommandDef
    {
        public readonly Type type;
        public readonly string label;

        public CommandDef(String cmd, Type cmdType)
        {
            this.label = cmd;
            this.type = cmdType;
            if (!typeof(Command).IsAssignableFrom(cmdType))
            {
                throw new DialogicException
                    ("Expected subclass of Command, but found " + cmdType);
            }
        }
    }

    public static class Ch
    {
        internal const char MODIFIER = '&';
        internal const char SYMBOL = '$';
        internal const char SCOPE = '.';
        internal const char OGROUP = '(';
        internal const char CGROUP = ')';
        internal const char OSAVE = '[';
        internal const char CSAVE = ']';
        internal const char OBOUND = '{';
        internal const char CBOUND = '}';
        internal const char LABEL = '#';
        internal const char NOT = '!';
        internal const char EQ = '=';
        internal const char OR = '|';
    }

    /// <summary>
    /// A set of precompiled regular expressions 
    /// </summary>
    public static class RE
    {
        public static Regex ParseChoices = new Regex(@"(?:\[([^=]+)=)*" + PRN + @"\]?(?:\.(" + SYM + @")\(\))*\]?");
        public static Regex ParseSymbols = new Regex(@"(?:(\[)([^=]+)=)?\$(?:(" + SYM + @")((?:\.(?:" + SYM + @")(?:\(\))?)*))(\])?");
        public static Regex ParseTransforms = new Regex(@"\(((?>[^()|$]+|(?<par>\()|(?<-par>\)))*(?(par)(?!)))\)((\.[A-Za-z0-9_-]+(?:\(\)))+)");

        public static Regex SplitOr = new Regex(@"\s*\|\s*");
        public static Regex HasParens = new Regex(@"[\(\)]");
        public static Regex MetaSplit = new Regex(@"\s*,\s*");
        public static Regex ValidGroup = new Regex(@"\([^)]+|[^)]+\)");
        public static Regex GrammarRules = new Regex(@"\s*<([^>]+)>\s*");
        public static Regex SingleComment = new Regex(@"//(.*?)(?:$|\r?\n)");
        public static Regex MultiComment = new Regex(@"/\*[^*]*\*+(?:[^/*][^*]*\*+)*/");
        public static Regex ParseSetArgs = new Regex(@"(\$?[A-Za-z_][^ \+\|\=]*)\s*([\+\|]?=)\s*(.+)");
        public static Regex FindMeta = new Regex(@"^(!?!?$?" + SYM + @")\s*([!*$^=<>]?=|<|>)\s*(\S+)$");
        public static Regex TestTubeChatBaby = new Regex(@"^C[0-9]+$");
        public static Regex MultiSpace = new Regex(@"\s+");
        public static Regex ResolvePost = new Regex(@"[()""]");

        internal const string MTD = @"(?:\{(.+?)\})?\s*";
        internal const string ACT = @"(?:([A-Za-z_][A-Za-z0-9_-]+):)?\s*";
        //internal const string TXT = @"((?:(?:[^}{#])*(?:\$"+SYM+")*)*)";
        internal const string TXT = @"((?:[^}{#])*)";
        internal const string LBL = @"((?:#[A-Za-z][\S]*)\s*|(?:#\(\s*[A-Za-z][^\|]*(?:\|\s*[A-Za-z][^\|]*)+\))\s*)?\s*";

        internal const string PRN = @"(?:\()((?>(?:(?<p>\()|(?<-p>\))|[^()|]+|(?(p)(?!))(?<pipe>\|))*))(?:\))(?(p)(?!))(?(pipe)|(?!))";
        internal const string SYM = "[A-Za-z_][A-Za-z0-9_-]*";
    }

    /// <summary>
    /// Static utility functions for Dialogic
    /// </summary>
    public static class Util
    {
        public static StringComparison INV = StringComparison.InvariantCulture;
        public static StringComparison IC = StringComparison.CurrentCultureIgnoreCase;

        internal static double INFINITE = -1;
        internal static string LABEL_IDENT = "#";
        internal static DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0);

        private static int start;
        private static Random random;

        static Util()
        {
            start = EpochMs();
            random = new Random();
            // TODO: redo timing w' stopwatch
        }

        /// <summary>
        /// Constrains the specified number to the range specified by low and high.
        /// </summary>
        /// <returns>The constrained number</returns>
        /// <param name="n">N.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        public static double Constrain(double n, double low, double high)
        {
            return Math.Max(Math.Min(n, high), low);
        }

        /// <summary>
        /// Maps a number from one range to another.
        /// </summary>
        /// <returns>The map.</returns>
        /// <param name="n">the incoming value to be converted</param>
        /// <param name="min">lower bound of the value's current range</param>
        /// <param name="max">upper bound of the value's current range</param>
        /// <param name="targetMin">lower bound of the value's target range</param>
        /// <param name="targetMax">upper bound of the value's target range</param>
        public static double Map(double n,
            double min, double max, double targetMin, double targetMax)
        {
            return (n - min) / (max - min) * (targetMax - targetMin) + targetMin;
        }

        /// <summary> A version of Equals for floating point comparison </summary>
        public static bool FloatingEquals(double a, double b, double epsilon = 0.00001f)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            // shortcut, handles infinities
#pragma warning disable RECS0018 // floating point equality 
            if (a == b)
            {
                return true;
            }
            else if (a == 0 || b == 0 || diff < Double.Epsilon)
            {
                // a or b is zero or both are extremely close
                // so relative error is less meaningful here
                return diff < epsilon;
            }
            else // use relative error
            {
                return diff / (absA + absB) < epsilon;
            }
#pragma warning restore RECS0018 // floating point equality 
        }

        /// <summary>
        /// Returns the number of milliseconds elapsed since the start of the program 
        /// </summary>
        /// <returns>The milliseconds.</returns>
        public static int Millis()
        {
            return EpochMs() - start;
        }

        /// <summary>
        /// Returns the number of milliseconds elapsed since the specified timestamp 
        /// </summary>
        /// <returns>The milliseconds.</returns>
        /// <param name="since">Since.</param>
        public static int Millis(int since)
        {
            return Millis() - since;
        }

        /// <summary>
        /// Returns the number of seconds elapsed since the specified timestamp, 
        /// or start of the program if not supplied
        /// </summary>
        /// <returns>The seconds</returns>
        /// <param name="since">Since.</param>
        public static string ElapsedSec(int since = 0)
        {
            return (Millis(since) / 1000.0).ToString("0.##") + "s";
        }

        /// <summary>
        /// Returns the number of milliseconds elapsed since epoch start
        /// </summary>
        /// <returns>The milliseconds</returns>
        public static int EpochMs()
        {
            return Environment.TickCount & Int32.MaxValue;
        }

        /// <summary>
        /// Returns the number of nanoseconds elapsed since epoch start
        /// </summary>
        /// <returns>The nanoseconds</returns>
        public static long EpochNs()
        {
            return (long)((DateTime.UtcNow - EPOCH).TotalMilliseconds * 1000000.0);
        }

        /// <summary>
        /// Returns a random item from the array
        /// </summary>
        /// <returns>a random item.</returns>
        /// <param name="arr">Arr.</param>
        public static object RandItem(object[] arr)
        {
            return arr[Rand(arr.Length)];
        }

        /// <summary>
        /// Returns a random item from the List
        /// </summary>
        /// <returns>a random item.</returns>
        /// <param name="l">L.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T RandItem<T>(List<T> l)
        {
            return l.ElementAt(Rand(l.Count));
        }

        /// <summary>
        /// Returns a random int i, where min &le; i &gt; max
        /// </summary>
        /// <returns>The rand.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public static int Rand(int min, int max)
        {
            return random.Next(min, max);
        }

        /// <summary>
        /// Returns a random int i, where 0 &le; i &gt; max
        /// </summary>
        /// <returns>The rand.</returns>
        /// <param name="max">Max.</param>
        public static int Rand(int max)
        {
            return Rand(0, max);
        }

        /// <summary>
        /// Returns a random double d, where 0 &le; d &gt; 1
        /// </summary>
        /// <returns>The rand.</returns>
        public static double Rand()
        {
            return random.NextDouble();
        }

        /// <summary>
        /// Returns a random double d, where min &le; d &gt; max
        /// </summary>
        /// <returns>The rand.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public static double Rand(double min, double max)
        {
            return min + Rand() * max;
        }

        /// <summary>
        /// Returns a random double d, where 0 &le; d &gt; max
        /// </summary>
        /// <returns>The rand.</returns>
        /// <param name="max">Max.</param>
        public static double Rand(double max)
        {
            return Rand(0, max);
        }

        /// <summary>
        /// Converts seconds to milliseconds if seconds is >= 0, else returns -1
        /// </summary>
        /// <returns>The milliseconds.</returns>
        /// <param name="seconds">Seconds.</param>
        public static int ToMillis(double seconds)
        {
            return (seconds < 0) ? -1 : (int)(seconds * 1000);
        }

        /// <summary>
        /// Converts milliseconds to seconds
        /// </summary>
        /// <returns>The seconds.</returns>
        /// <param name="millis">Millis.</param>
        public static double ToSec(int millis)
        {
            return millis / 1000.0;
        }

        /// <summary>
        /// First trims white-space, then trims the first character if the 
        /// string starts with any of those specified
        /// </summary>
        /// <param name="s">S.</param>
        /// <param name="c">C.</param>
        /// <returns>true if the first char was removed, else false</returns>
        public static bool TrimFirst(ref string s, params char[] c)
        {
            s = s.Trim();
            for (int i = 0; i < c.Length; i++)
            {
                if (s.IndexOf(c[i]) == 0)
                {
                    s = s.Substring(1).Trim();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// First trims white-space, then trim the last character if the 
        /// string ends with any of those specified
        /// </summary>
        /// <param name="s">S.</param>
        /// <param name="c">C.</param>
        /// <returns>true if the first char was removed, else false</returns>
        public static bool TrimLast(ref string s, params char[] c)
        {
            s = s.Trim();
            for (int i = 0; i < c.Length; i++)
            {
                if (s.LastIndexOf(c[i]) == s.Length - 1)
                {
                    s = s.Substring(0, s.Length - 1).Trim();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  Computes the distance (the number of deletions, insertions, or substitutions required to transform one into the other) between the source and target strings 
        /// </summary>
        public static int MinEditDist(string s, string t) => MinEditDistance.Compute(s, t);

        /// <summary>
        ///  Computes the distance (the number of deletions, insertions, or substitutions required to transform one into the other) between the source and target arrays 
        /// </summary>
        public static int MinEditDist(string[] s, string[] t) => MinEditDistance.Compute(s, t);

        /// <summary>
        ///  Computes the distance (the number of deletions, insertions, or substitutions required to transform one into the other) between the source and target strings divided by the length of the longer string
        /// </summary>
        public static double MinEditDistAdj(string s, string t) => MinEditDistance.ComputeAdjusted(s, t);

        /// <summary>
        ///  Computes the distance (the number of deletions, insertions, or substitutions required to transform one into the other) between the source and target arrays divided by the length of the longer array
        /// </summary>
        public static double MinEditDistAdj(string[] s, string[] t) => MinEditDistance.ComputeAdjusted(s, t);


        ////////////////////////////////////////////////////////////////////////


        internal static string JavaSubstr(string s, int beginIndex, int endIndex)
        {
            int len = endIndex - beginIndex;
            return s.Substring(beginIndex, len);
        }

        internal static bool HasOpenGroup(string text)
        {
            return text.IndexOf('|') > -1 &&
                (text.IndexOf('(') < 0 || text.IndexOf('(') < 0);
        }

        internal static object ConvertTo(Type t, object val)
        {
            if (t == typeof(double))
            {
                val = Convert.ToDouble(val);
            }
            else if (t == typeof(int))
            {
                val = Convert.ToInt32(val);
            }
            else if (t == typeof(bool))
            {
                val = Convert.ToBoolean(val);
            }
            return val;
        }

        internal static int SecStrToMs(string s, int defaultMs = -1)
        {
            double d;
            try
            {
                d = (double)Convert.ChangeType(s, typeof(double));
            }
            catch (FormatException)
            {
                return defaultMs;
            }
            return (int)(d * 1000);
        }

        internal static string ToMixedCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            return (s[0].ToString()).ToUpper() + s.Substring(1).ToLower();
        }


        internal static void ShowMatches(MatchCollection matches)
        {
            int i = 0;
            if (matches.Count < 1) Console.WriteLine("<empty>");
            foreach (Match match in matches) ShowMatch(match, i++);
        }

        internal static int ShowMatch(Match match, int i = 0)
        {
            Console.WriteLine("\nMatch {0} has {1} groups:\n", i, match.Groups.Count);

            int groupNo = 0;
            foreach (Group mm in match.Groups)
            {
                Console.WriteLine("  Group {0} has {1} capture(s) '{2}'",
                    groupNo, mm.Captures.Count, mm.Value);

                int captureNo = 0;
                foreach (Capture cc in mm.Captures)
                {
                    Console.WriteLine("       Capture {0} '{1}'", captureNo++, cc);
                }
                groupNo++;
            }

            groupNo = 0;
            Console.WriteLine("\n  match.Value = \"{0}\"", match.Value);
            foreach (Group mm in match.Groups)
            {
                Console.WriteLine("  match.Groups[{0}].Value = \"{1}\"",
                    groupNo, match.Groups[groupNo++].Value);
            }

            groupNo = 0;
            foreach (Group mm in match.Groups)
            {
                int captureNo = 0;
                foreach (Capture cc in mm.Captures)
                {
                    Console.WriteLine("  match.Groups[{0}].Captures[{1}].Value = \"{2}\"",
                        groupNo, captureNo, match.Groups[groupNo].Captures[captureNo++].Value); //**
                }
                groupNo++;
            }

            return i;
        }

        internal static int Round(double p)
        {
            return (int)Math.Round(p);
        }

        internal static int Round(float p)
        {
            return (int)Math.Round(p);
        }

        internal static void ValidateLabel(ref string lbl)
        {
            if (lbl.StartsWith(LABEL_IDENT, INV)) lbl = lbl.Substring(1);
        }

        internal static string[] StripSingleLineComments(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
                int idx = lines[i].IndexOf("//", Util.INV);
                if (idx > -1) lines[i] = lines[i].Substring(0, idx);
            }
            return lines;
        }

        internal static string[] StripMultiLineComments(string[] lines)
        {
            bool commentOpen = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (commentOpen)
                {
                    int endIdx = line.IndexOf("*/", Util.INV);
                    if (endIdx < 0)
                    {
                        line = String.Empty;
                    }
                    else
                    {
                        line = line.Substring(endIdx + 2);
                    }
                    commentOpen = false;
                }

                int startIdx = line.IndexOf("/*", Util.INV);
                if (startIdx > -1)
                {
                    int endIdx = line.IndexOf("*/", Util.INV);

                    if (endIdx < 0) // only open, no close
                    {
                        line = line.Substring(0, startIdx);
                        commentOpen = true;
                    }
                    else if (startIdx == 0 && endIdx == line.Length - 2) // a
                    {
                        line = String.Empty;
                    }
                    else if (startIdx == 0 && endIdx > startIdx)        // b
                    {
                        line = line.Substring(endIdx + 2);
                    }
                    else if (startIdx > 0 && endIdx == line.Length - 2) // c
                    {
                        line = line.Substring(0, startIdx - 1);
                    }
                    else if (startIdx > 0 && endIdx < line.Length - 2) // d
                    {
                        var tmp = line.Substring(0, startIdx - 1);
                        line = tmp + line.Substring(endIdx + 2);
                    }
                }

                lines[i] = line;
            }

            return lines;
        }

        internal static void Log(string logFileName, object msg)
        {
            using (StreamWriter w = File.AppendText(logFileName))
            {
                w.WriteLine(DateTime.Now.ToLongTimeString() + "\t"
                    + Util.EpochMs() + "\t" + msg);
            }
        }

        internal static IEnumerable<string> SortByLength(IEnumerable<string> e)
        {
            return from s in e orderby s.Length descending select s;
        }

    }

    /// <summary>
    /// Minimum-Edit-Distance (or Levenshtein distance) is a measure of the similarity 
    /// between two strings, the source string and the target string. The distance
    /// is the number of deletions, insertions, or substitutions required to transform
    /// the source into the target. The adjusted distance divides this value by the max
    /// length of the two strings<br/><br/>
    /// Adapted from Michael Gilleland's algorithm
    /// </summary>
    public static class MinEditDistance
    {
        public static double ComputeAdjusted(string source, string target)
        {
            return Compute(source, target)
                / (double)Math.Max(source.Length, target.Length);
        }

        public static double ComputeAdjusted(string[] source, string[] target)
        {
            return Compute(source, target)
                / (double)Math.Max(source.Length, target.Length);
        }

        public static int Compute(string s, string t)
        {
            return Compute(
                s.Select(c => c.ToString()).ToArray(),
                t.Select(c => c.ToString()).ToArray());
        }

        public static int Compute(string[] s, string[] t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(Math.Min(d[i - 1,
                        j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }

    /// <summary>
    /// Refers to the Contraints behavior during search, specifically whether
    /// it can be 'relaxed to allow a larger result set
    /// </summary>
    public enum ConstraintType { Soft = 0, Hard = 1, Absolute = 2 };


    /// <summary>
    /// Implements a Constraint (a name, a value, an Operator) that can be checked
    /// </summary>
    public class Constraint
    {
        public readonly string name;
        public readonly Operator op;
        public ConstraintType type;
        public string value;

        public Constraint(string key, string val,
            ConstraintType type = ConstraintType.Soft) :
            this(Operator.EQ, key, val, type)
        { }

        public Constraint(Operator op, string key, string val,
            ConstraintType type = ConstraintType.Soft)
        {
            this.type = type;
            this.value = val;
            this.name = key;
            this.op = op;

            if (val.Contains(Ch.OR) && op != Operator.RE)
            {
                throw new ParseException("Regex operator (*=) expected with |");
            }
        }

        public static IDictionary<string, object> AsDict(params Constraint[] c)
        {
            var dict = new Dictionary<string, object>();
            for (int i = 0; i < c.Length; i++)
            {
                dict.Add(c[i].name, c[i]);
            }
            return dict;
        }

        public bool Check(Resolver resolver, string check, IDictionary<string, object> globals = null)
        {
            string rval = value;

            if (globals != null)
            {
                if (check.Contains(Ch.SYMBOL))
                {
                    check = resolver.BindSymbols(check, null, globals);
                }
                if (value.Contains(Ch.SYMBOL))
                {
                    rval = resolver.BindSymbols(value, null, globals);
                }
            }

            return op.Invoke(check, rval);
        }

        public override string ToString()
        {
            return TypeToString() + name + op + value;
        }

        private string TypeToString()
        {
            switch (type)
            {
                case ConstraintType.Hard: return Ch.NOT.ToString();
                case ConstraintType.Absolute: return Ch.NOT.ToString() + Ch.NOT;
                default: return String.Empty;
            }
        }

        public bool IsStrict()
        {
            return this.type == ConstraintType.Hard
                || this.type == ConstraintType.Absolute;
        }

        public double ValueAsDouble(double def = -1)
        {
            double d = def;
            try
            {
                d = Double.Parse(value);
            }
            catch (Exception) { }
            return d;
        }

        public bool ScaleValue(double scale)
        {
            double dval;
            if (Double.TryParse(value, out dval))
            {
                value = (dval * scale).ToString();
                return true;
            }
            return false;
        }

        public bool IncrementValue(double incr = 1)
        {
            double dval;
            if (Double.TryParse(value, out dval))
            {
                double newval = dval + incr;
                value = newval.ToString();
                if (newval < Defaults.FIND_MAX_STALENESS)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsRelaxable()
        {
            return this.type == ConstraintType.Hard;
        }

        public IDictionary<string, object> AsDict()
        {
            return Constraint.AsDict(this);
        }

        internal Constraint Copy()
        {
            return new Constraint(op, name, value, type);
        }

        internal void Relax()
        {
            if (IsRelaxable()) type = ConstraintType.Soft;
        }

        private static Exception InvalidState(string sub)
        {
            return new BindException("Invalid State: '" + sub + "'");
        }
    }

    public static class Entities
    {
        private static readonly IDictionary<string, string> ESCAPES
            = new Dictionary<string, string>() // replace with C# native?
            {
                {"\"",     "quot"},
                {"$",      "dollar"},
                {"{",      "lcub"},
                {"}",      "rcub"},
                {"(",      "lpar"},
                {")",      "rpar"},
                {"&",      "amp"},
                {"!",      "excl"},
                {"©",      "copy"},
                {"'",      "apos"},
                {"#",      "num"},
                {"<",      "lt"},
                {">",      "gt"},
                {" ",      "nbsp"},
                {"®",      "reg"},
                {"™",      "tm"},
                {"|",      "vert"},
                {"*",      "ast"},
                {":",      "colon"}
            };

        private static int MIN_ESCAPE = 2, MAX_ESCAPE = 6;

        static Entities()
        {
            LOOKUP = ESCAPES.ToLookup(pair => pair.Value, pair => pair.Key);
        }

        public static String Decode(String input)
        {
            if (!input.Contains('&')) return input;

#pragma warning disable XS0001  //  Mono StringBuilder serialization warning

            StringBuilder writer = null;
            int i = 1;
            int st = 0;
            while (true)
            {
                // look for '&'
                while (i < input.Length && input.charAt(i - 1) != '&')
                    i++;
                if (i >= input.Length)
                    break;

                // found '&', look for ';'
                int j = i;
                while (j < input.Length && j < i + MAX_ESCAPE + 1 && input.charAt(j) != ';')
                    j++;
                if (j == input.Length || j < i + MIN_ESCAPE || j == i + MAX_ESCAPE + 1)
                {
                    i++;
                    continue;
                }

                // found escape 
                if (input.charAt(i) == '#')
                {
                    // numeric escape
                    int k = i + 1;
                    int radix = 10;

                    char firstChar = input.charAt(k);
                    if (firstChar == 'x' || firstChar == 'X')
                    {
                        k++;
                        radix = 16;
                    }

                    try
                    {
                        //int entityValue = Int32.Parse(input.Substring(k, j), radix);
                        int entityValue = Convert.ToInt32(Util.JavaSubstr(input, k, j), radix);

                        if (writer == null) writer = new StringBuilder(input.Length);

                        writer.Append(input.Substring(st, i - 1));

                        if (entityValue > 0xFFFF)
                        {
                            writer.Append(entityValue.ToString().Substring(0, 2));
                        }
                        else
                        {
                            writer.Append(entityValue);
                        }

                    }
                    catch (Exception)
                    {
                        i++;
                        continue;
                    }
                }
                else
                {
                    // named escape
                    var res = LOOKUP[Util.JavaSubstr(input, i, j)];
                    string value = res != null ? res.First() : null;
                    if (value == null)
                    {
                        i++;
                        continue;
                    }

                    if (writer == null) writer = new StringBuilder(input.Length);
                    writer.Append(Util.JavaSubstr(input, st, i - 1));
                    writer.Append(value);

                }

                // skip escape
                st = j + 1;
                i = st;
            }

            if (writer != null)
            {
                //Console.WriteLine("input.Substring(st, input.Length) :: "+input);
                writer.Append(Util.JavaSubstr(input, st, input.Length));
                return writer.ToString();
            }
#pragma warning restore XS0001 //  Mono StringBuilder serialization warning

            return input;
        }

        private static ILookup<string, string> LOOKUP;
    }

    public class ObjectPool<T> //@cond unused
    {
        private readonly Func<T> generator;
        private readonly Action<T> recycler;
        private readonly T[] pool;

        private int cursor = 0;

        public ObjectPool(int size, Func<T> generator, Action<T> recycler = null)
        {
            this.pool = new T[size];
            this.recycler = recycler;
            this.generator = generator;
            for (int i = 0; i < size; i++)
            {
                pool[i] = generator();
            }
        }

        public T Get()
        {
            T next = pool[cursor];

            // currently recycles the next object, even if it is in use
            if (recycler != null) recycler(next);

            cursor = ++cursor < pool.Length ? cursor : 0;
            return next;
        }

    }//@endcond


    public static class Exts //@cond hidden
    {
        internal delegate void Action<T1, T2, T3, T4, T5>
            (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        internal static void Apply<T>(this IList<T> il, Action<T, T, T, T, T> action)
        {
            action(il[0], il[1], il[2], il[3], il[4]);
        }

        internal static string[] Values(this GroupCollection groups)
        {
            if (groups == null) throw new ArgumentException("Null groups");

            string[] parts = new string[groups.Count];
            for (int i = 0; i < groups.Count; i++)
            {
                parts[i] = groups[i].Value;
            }

            return parts;
        }

        /// <summary>
        /// Removes the last element of a list and returns it
        /// </summary>
        internal static T Pop<T>(this List<T> list)
        {
            var last = list.Last();
            list.RemoveAt(list.Count - 1);
            return last;
        }

        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> ie)
        {
            if (ie == null) return true;
            var coll = ie as ICollection<T>;
            return (coll != null) ? coll.Count < 1 : !ie.Any();
        }

        internal static bool IsNumber(this object value) // ext
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        internal static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.InvariantCulture);
            if (pos < 0) return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        internal static bool StartsWith(this string str, char c)
        {
            return !str.IsNullOrEmpty() && str[0] == c;
        }

        internal static bool EndsWith(this string str, char c)
        {
            return !str.IsNullOrEmpty() && str[str.Length - 1] == c;
        }

        internal static bool Contains(this string str, params char[] c)
        {
            if (str.IsNullOrEmpty()) return false;
            for (int i = 0; i < c.Length; i++)
            {
                if (str.IndexOf(c[i]) > -1) return true;
            }
            return false;
        }

        internal static bool ContainsAny(this string str, params char[] c)
        {
            if (str.IsNullOrEmpty()) return false;
            for (int i = 0; i < c.Length; i++)
            {
                if (str.IndexOf(c[i]) > -1) return true;
            }
            return false;
        }

        internal static bool ContainsAll(this string str, params char[] c)
        {
            if (str.IsNullOrEmpty()) return false;
            for (int i = 0; i < c.Length; i++)
            {
                if (str.IndexOf(c[i]) < 0) return false;
            }
            return true;
        }

        internal static bool EnclosedBy(this string str, char c, char d, bool strict = false)
        {
            var ok = !str.IsNullOrEmpty() && str[0] == c && str[str.Length - 1] == d;
            if (strict && ok)
            {
                var tmp = str.Substring(1, str.Length - 2);
                //ok = !tmp.Contains(c) && !tmp.Contains(d);
                ok = !(new Regex(@"(?:\([^)]|[^(]\))").IsMatch(tmp));
            }
            return ok;
        }

        internal static bool EqualsAny(this string str, params string[] c)
        {
            if (str.IsNullOrEmpty()) return false;
            for (int i = 0; i < c.Length; i++)
            {
                if (str == c[i]) return true;
            }
            return false;
        }

        internal static bool StartsWithAny(this string str, params string[] c)
        {
            if (str.IsNullOrEmpty()) return false;
            for (int i = 0; i < c.Length; i++)
            {
                if (str.StartsWith(c[i], Util.INV)) return true;
            }
            return false;
        }

        internal static string TrimEnds(this string str, char start, char end)
        {
            str = str.TrimFirst(start);
            str = str.TrimLast(end);
            return str;
        }

        internal static string TrimFirst(this string str, char c)
        {
            return (!str.IsNullOrEmpty() && str[0] == c) ? str.Substring(1) : str;
        }

        internal static string TrimLast(this string str, char c)
        {
            int last = str.Length - 1;
            return (!str.IsNullOrEmpty() && str[last] == c) ? str.Substring(0, last) : str;
        }

        internal static char charAt(this string str, int i)
        {
            return str[i];
        }

        internal static string Parenthify(this string str)
        {
            if (!(str.StartsWith('(') && str.EndsWith(')')))
            {
                str = '(' + str + ')';
            }
            return str;
        }

        public static string Stringify(this object o)
        {
            if (o == null) return "null";

            string s = string.Empty;
            if (o is IDictionary)
            {
                IDictionary id = (System.Collections.IDictionary)o;
                if (id.Count > 0)
                {
                    s += "{";
                    foreach (var k in id.Keys) s += k + ":" + id[k] + ",";
                    s = s.Substring(0, s.Length - 1) + "}";
                }
            }
            else if (o is HashSet<string>)
            {
                s = "[";
                var coll = (ICollection<string>)o;
                if (coll.Count < 1) s += "]";
                foreach (var k in coll) s += k + ",";
                s = (s.Substring(0, s.Length - 1) + "]");
            }
            else if (o is object[])
            {
                var arr = ((object[])o);
                s = "[";
                for (int i = 0; i < arr.Length; i++)
                {
                    s += arr[i];
                    if (i < arr.Length - 1) s += ",";
                }
                s += "]";
            }
            else if (o is ICollection)
            {
                var coll = (ICollection)o;
                s = "[";
                if (coll.Count < 1) s += "]";
                foreach (var k in coll) s += k + ",";
                s = (s.Substring(0, s.Length - 1) + "]");
            }
            else
            {
                s = o.ToString();
            }
            return s;
        }

        internal static bool IsGenType(object obj, Type genericType)
        {
            if (obj == null)
                return false;

            var type = obj.GetType();
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                    return true;

                type = type.BaseType;
            }
            return false;
        }

    }//@endcond

}