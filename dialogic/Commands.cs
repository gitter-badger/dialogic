using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Dialogic
{
    public abstract class Command
    {
        const string PACKAGE = "Dialogic.";

        protected static int IDGEN = 0;

        protected static readonly Command NOP = new NoOp();

        public string Id { get; protected set; }
        public string Text { get; set; }
        internal float WaitSecs = 0;

        protected Command() => this.Id = (++IDGEN).ToString();

        private static string ToMixedCase(string s) =>
            (s[0] + "").ToUpper() + s.Substring(1).ToLower();

        public static Command Create(string type, params string[] args)
        {
            type = ToMixedCase(type);
            return Create(Type.GetType(PACKAGE + type) ??
                throw new TypeLoadException("No type: " + PACKAGE + type), args);
        }

        public static Command Create(Type type, params string[] args)
        {
            Command cmd = (Command)Activator.CreateInstance(type);
            cmd.Init(args);
            return cmd;
        }

        public virtual int WaitTime() => (int)(WaitSecs * 1000); // no wait

        public virtual void Init(params string[] args) => this.Text = String.Join("", args);

        public virtual string TypeName() => this.GetType().ToString().Replace(PACKAGE, "");

        public virtual void Fire(ChatRuntime cr) { this.HandleVars(cr.globals); }

        public virtual void HandleVars(Dictionary<string, object> globals)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                foreach (string s in SortByLength(globals.Keys))
                {
                    //System.Console.WriteLine($"s=${s} -> {globals[s]}"); 
                    Text = Text.Replace("$" + s, globals[s].ToString());
                }
            }
        }

        public override string ToString() => "[" + TypeName().ToUpper() + "] " + Text;

        protected static string QQ(string text) => "'" + text + "'";

        protected static IEnumerable<string> SortByLength(IEnumerable<string> e)
        {
            return from s in e orderby s.Length descending select s;
        }
    }

    public class Go : Command
    {
        public Go() : base() { }

        public Go(string text) : base() => this.Text = text;

        public override void Fire(ChatRuntime cr)
        {
            Chat chat = cr.FindChat(Text);
            cr.Run(chat);
        }
    }

    public class NoOp : Command { }

    public class Say : Command
    {
        public override string ToString() => "[" + TypeName().ToUpper() + "] " + QQ(Text);
    }

    public class Set : Command
    {
        public object Value { get; protected set; }

        public override void Init(params string[] args)
        {
            string[] pair = DoSetArgs(args);
            base.Init(pair[0]);
            this.Value = pair[1];
        }

        private static string[] DoSetArgs(string[] args)
        {
            if (args.Length != 1)
            {
                throw new TypeLoadException("Bad args(" + args.Length + "): " + String.Join(",", args));
            }

            var pair = Regex.Split(args[0], @"\s*=\s*");
            if (pair.Length != 2) pair = Regex.Split(args[0], @"\s+");

            if (pair.Length != 2)
            {
                throw new TypeLoadException("Bad args(" + args.Length + "): " + String.Join(",", args));
            }

            if (pair[0].StartsWith("$", StringComparison.Ordinal))
            {
                pair[0] = pair[0].Substring(1); // tmp: leading $ is optional
            }

            return pair;
        }

        public override string ToString() => "[" + TypeName().ToUpper() + "] $" + Text + '=' + Value;

        public override void HandleVars(Dictionary<string, object> globals) { } // no-op

        public override void Fire(ChatRuntime cr) => cr.Globals()[Text] = Value;
    }

    public class Do : Command { }

    public class Chat : Command
    {
        public List<Command> commands = new List<Command>();

        public Chat() : this("C" + Environment.TickCount) { }

        public Chat(string name) : base() => this.Text = name;

        public string ToTree()
        {
            string s = base.ToString() + "\n";
            commands.ForEach(c => s += "  " + c + "\n");
            return s;
        }

        public void AddCommand(Command c) => this.commands.Add(c);
    }

    public class Wait : Command
    {
        public override void Init(params string[] args) =>
            WaitSecs = args.Length == 1 ? float.Parse(args[0]) : 0;

        public override string ToString() => "[" + TypeName().ToUpper() + "] " + WaitSecs;

        public override void HandleVars(Dictionary<string, object> globals) { }

        public override int WaitTime() => WaitSecs > 0
            ? (int)(WaitSecs * 1000) : Timeout.Infinite;
    }

    public class Opt : Command
    {
        public Command action;

        public Opt() : this("") { }

        public Opt(string text) : this(text, NOP) { }

        public Opt(string text, Command action) : base()
        {
            this.Text = text;
            this.action = action;
        }

        public override void Init(params string[] args)
        {
            if (args.Length < 1)
            {
                throw new TypeLoadException("Bad args: " + args);
            }
            this.Text = args[0];
            this.action = (args.Length > 1) ? Command.Create(typeof(Go), args[1]) : null;
        }

        public override string ToString() => "[" + TypeName().ToUpper() + "] "
            + QQ(Text) + (action is NoOp ? "" : " (-> " + action.Text + ")");

        public string ActionText() => action != null ? action.Text : "";

        public override void HandleVars(Dictionary<string, object> globals)
        {
            base.HandleVars(globals);

            if (action != null && !string.IsNullOrEmpty(action.Text))
            {
                foreach (string s in SortByLength(globals.Keys))
                {
                    //System.Console.WriteLine($"Global.${s} -> {globals[s]}");
                    action.Text = action.Text.Replace("$" + s, globals[s].ToString());
                }
            }
        }
    }

    public class Ask : Command
    {
        public int SelectedIdx { get; protected set; }

        public int attempts = 0;

        private readonly List<Opt> options = new List<Opt>();

        public override void Init(params string[] args)
        {
            if (args.Length < 0 || args.Length > 2)
            {
                throw new TypeLoadException("Bad args: " + args.Length);
            }
            base.Init(args[0]);
            if (args.Length > 1) WaitSecs = float.Parse(args[1]);
        }

        public List<Opt> Options()
        {
            if (options.Count < 1)
            {
                options.Add(new Opt("Yes"));
                options.Add(new Opt("No"));
            }
            return options;
        }

        public override int WaitTime() => WaitSecs > 0 
            ?  (int)(WaitSecs * 1000) : Timeout.Infinite;

        public Opt Selected() => options[SelectedIdx];

        public void AddOption(Opt o) => options.Add(o);

        public Command Choose(string input) // return next Command or null
        {
            attempts++;
            if (int.TryParse(input, out int i))
            {
                if (i > 0 && i <= options.Count)
                {
                    SelectedIdx = --i;
                    return this.options[SelectedIdx].action;
                }
            }
            throw new InvalidChoice(this);
        }

        public override void Fire(ChatRuntime cr)
        {
            Options().ForEach(o => o.Fire(cr)); // fire for child options
            base.Fire(cr);
        }

        public override string ToString()
        {
            string s = "[" + TypeName().ToUpper() + "] " + QQ(Text) + " (";
            Options().ForEach(o => s += o.Text + ",");
            return s.Substring(0, s.Length - 1) + ")";
        }

        public string ToTree()
        {
            string s = "[" + TypeName().ToUpper() + "] " + QQ(Text) + "\n";
            Options().ForEach(o => s += "    " + o + "\n");
            return s.Substring(0, s.Length - 1);
        }
    }
}