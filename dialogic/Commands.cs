using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dialogic
{
    public class NoOp : Command
    {
        public override ChatEvent Fire(ChatRuntime cr)
        {
            return null;
        }
    }

    public class Go : Command
    {
        public Go() : base() { }

        public Go(string text) : base()
        {
            this.Text = text;
        }

        public override ChatEvent Fire(ChatRuntime cr)
        {
            ChatEvent ce = base.Fire(cr);
            cr.Run(cr.FindChat(ce.Command.Text));
            return ce;
        }
    }

    public class Say : Command, IEmittable
    {
        public Say() : base()
        {
            this.PauseAfterMs = 1000;
        }
    }

    public class Do : Command, IEmittable
    {

        public Do() : base()
        {
            this.PauseAfterMs = 1000;
        }
    }

    public class Set : Command // TODO: rethink this
    {
        public string Value;

        public Set() : base() { }

        public Set(string name, string value) : base() // not used (add tests)
        {
            this.Text = name;
            this.Value = value;
        }

        public override void Init(params string[] args)
        {
            string[] parts = ParseSetArgs(args);
            this.Text = parts[0];
            this.Value = parts[1];
        }

        private string[] ParseSetArgs(string[] args)
        {
            if (args.Length != 1)
            {
                throw BadArgs(args, 1);
            }

            var pair = Regex.Split(args[0], @"\s*=\s*");
            if (pair.Length != 2) pair = Regex.Split(args[0], @"\s+");

            if (pair.Length != 2) throw BadArgs(pair, 2);

            if (pair[0].StartsWith("$", StringComparison.Ordinal))
            {
                pair[0] = pair[0].Substring(1); // tmp: leading $ is optional
            }

            return pair;
        }

        public override Command Copy()
        {
            return (Set)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return "[" + TypeName().ToUpper() + "] $" + Text + '=' + Value;
        }

        public override ChatEvent Fire(ChatRuntime cr)
        {
            Set clone = (Set)this.Copy();
            var globals = cr.Globals();
            if (globals != null)
            {
                Substitutions.Do(ref clone.Value, globals);
                globals[Text] = Value; // set the global var
            }
            return new ChatEvent(clone);
        }
    }

    public class Wait : Command
    {
        public Wait() : base()
        {
            PauseAfterMs = -1;
        }

        public override void Init(params string[] args)
        {
            if (args.Length > 0) PauseAfterMs =
                Util.ToMillis(double.Parse(args[0]));
        }
    }

    public class Ask : Command, IEmittable
    {
        public int SelectedIdx { get; protected set; }

        protected List<Opt> options = new List<Opt>();

        public Ask()
        {
            this.PauseAfterMs = Infinite;
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

        public string OptionsJoined(string delim=",")
        {
            var opts = Options();
            var s = "";
            opts.ForEach((o) => s += o.Text+" ");
            return s.Trim().Replace(" ", delim);
        }

        public Opt Selected()
        {
            return Options()[SelectedIdx];
        }

        public Opt Selected(int i)
        {
            this.SelectedIdx = i;
            if (i >= 0 && i < options.Count)
            {
                return Selected();
            }
            throw new InvalidChoice(this);
        }

        public void AddOption(Opt o)
        {
            o.parent = this;
            options.Add(o);
        }

        public override ChatEvent Fire(ChatRuntime cr)
        {
            Ask clone = (Ask)this.Copy();
            Substitutions.Do(ref clone.Text, cr.Globals());
            clone.options.ForEach(delegate (Opt o)
            {
                Substitutions.Do(ref o.Text, cr.Globals());
            });
            return new ChatEvent(clone);
        }

        public override string ToString()
        {
            string s = "[" + TypeName().ToUpper() + "] " + QQ(Text) + " (";
            Options().ForEach(o => s += o.Text + ",");
            return s.Substring(0, s.Length - 1) + ") " + MetaStr();
        }

        /*public string ToTree()
        {
            string s = base.ToString() + "\n";
            Options().ForEach(o => s += "    " + o + "\n");
            return s.Substring(0, s.Length - 1);
        }*/

        public override Command Copy()
        {
            Ask clone = (Ask)this.MemberwiseClone();
            clone.options = new List<Opt>();
            Options().ForEach(delegate (Opt o)
            {
                clone.AddOption((Opt)o.Copy());
            });
            return clone;
        }

    }

    public class Opt : Command
    {
        public Command action;

        public Ask parent;

        public Opt() : this("") { }

        public Opt(string text) : this(text, NOP) { }

        public Opt(string text, Command action) : base()
        {
            this.Text = text;
            this.action = action;
        }

        public override void Init(params string[] args)
        {
            if (args.Length < 1) throw BadArgs(args, 1);
            this.Text = args[0];
            this.action = (args.Length > 1) ? Command.Create(typeof(Go), args[1]) : NOP;
        }

        public override string ToString()
        {
            return "[" + TypeName().ToUpper() + "] " + QQ(Text)
                + (action is NoOp ? "" : " (-> " + action.Text + ")");
        }

        public string ActionText()
        {
            return action != null ? action.Text : "";
        }

        public override Command Copy()
        {
            Opt o = (Opt)this.MemberwiseClone();
            if (o.action != null) o.action = (Command)action.Copy();
            return o;
        }
    }

    /** Command that takes only a set of # separated key-value pairs */
    public abstract class KeyVal : Command
    {
        public override void Init(params string[] args)
        {
            if (args.Length < 1) throw BadArgs(args, 1);
            MetaFromArgs(args);
        }

        public override string ToString()
        {
            return base.ToString() + " " + MetaStr();
        }
    }

    public class Find : KeyVal
    {
        public override ChatEvent Fire(ChatRuntime cr)
        {
            ChatEvent ce = base.Fire(cr);
            cr.Run(cr.Find(((Find)ce.Command).meta));
            return ce;
        }
    }

    public class Meta : KeyVal { }

    public class Cond : KeyVal { }

    public interface IEmittable { }

    public class Chat : Command
    {
        public List<Command> commands = new List<Command>(); // not copied

        public Chat() : this("C" + Util.EpochMs()) { }

        public Chat(string name)
        {
            this.Text = name;
        }

        public void AddCommand(Command c)
        {
            this.commands.Add(c);
        }

        public override void Init(params string[] args)
        {
            if (args.Length < 1)
            {
                throw BadArgs(args, 1);
            }

            this.Text = args[0];

            if (Regex.IsMatch(Text, @"\s+"))
            {
                throw BadArg("CHAT name '" + Text + "' contains spaces!");
            }
        }

        public override string ToString()
        {
            return "[" + TypeName().ToUpper() + "] " + Text + " " + MetaStr();
        }

        public string ToTree()
        {
            string s = "[" + TypeName().ToUpper() + "] " + Text + "\n";
            if (HasMeta()) s += "  [COND] " + MetaStr() + "\n";
            commands.ForEach(c => s += "  " + c + "\n");
            return s;
        }
    }

    public abstract class Command : MetaData
    {
        public const string PACKAGE = "Dialogic.";
        public const int Infinite = -1;

        protected static int IDGEN = 0;

        public static readonly Command NOP = new NoOp();

        public string Id { get; protected set; }

        public int PauseAfterMs { get; protected set; }

        public string Text;

        protected Command()
        {
            this.Id = (++IDGEN).ToString();
            this.PauseAfterMs = 0;
        }

        private static string ToMixedCase(string s)
        {
            return (s[0] + "").ToUpper() + s.Substring(1).ToLower();
        }

        public static Command Create(string type, params string[] args)
        {
            type = ToMixedCase(type);
            var cmd = Create(Type.GetType(PACKAGE + type), args);
            if (cmd != null) return cmd;
            throw new TypeLoadException("No type: " + PACKAGE + type);
        }

        public static Command Create(Type type, params string[] args)
        {
            Command cmd = (Command)Activator.CreateInstance(type);
            cmd.Init(args);
            return cmd;
        }

        public virtual void Init(params string[] args)
        {
            this.Text = args[0];
            if (args.Length > 1) PauseAfterMs =
                Util.ToMillis(double.Parse(args[1]));
        }

        public virtual string TypeName()
        {
            return this.GetType().ToString().Replace(PACKAGE, "");
        }

        public virtual ChatEvent Fire(ChatRuntime cr)
        {
            Command clone = this.Copy();
            Substitutions.Do(ref clone.Text, cr.Globals());
            return new ChatEvent(clone);
        }

        public virtual Command Copy()
        {
            return (Command)this.MemberwiseClone();
        }

        protected Exception BadArg(string msg)
        {
            throw new ArgumentException(msg);
        }

        protected Exception BadArgs(string[] args, int expected)
        {
            return BadArg(TypeName().ToUpper() + " expects " + expected + " args,"
                + " got " + args.Length + "'" + string.Join(" # ", args) + "'\n");
        }

        public override string ToString()
        {
            return "[" + TypeName().ToUpper() + "] " + Text + " " + MetaStr();
        }

        public string TimeStr()
        {
            return PauseAfterMs > 0 ? "wait:" + Util.ToSec(PauseAfterMs) : "";
        }

        protected override string MetaStr()
        {
            var s = base.MetaStr();
            if (PauseAfterMs > 0)
            {
                var t = TimeStr() + "}";
                s = (s.Length < 1) ? "{" + t : s.Replace("}", "," + t);
            }
            return s;
        }

        protected static string QQ(string text)
        {
            return "'" + text + "'";
        }
    }

    public class MetaData
    {
        protected Dictionary<string, string> meta;

        public bool HasMeta()
        {
            return meta != null && meta.Count > 0;
        }

        /* Note: new keys will overwrite old keys with same name */
        public void SetMeta(string key, string val)
        {
            if (meta == null)
            {
                meta = new Dictionary<string, string>();
            }
            meta[key] = val;
        }

        public void AddMeta(Dictionary<string, string> pairs)
        {
            if (pairs != null)
            {
                foreach (var key in pairs.Keys)
                {
                    SetMeta(key, pairs[key]);
                }
            }
        }

        public Dictionary<string, string> ToDict()
        {
            return meta;
        }

        public List<KeyValuePair<string, string>> ToList()
        {
            return meta != null ? meta.ToList() : null;
        }

        protected virtual string MetaStr()
        {
            string s = "";
            if (HasMeta())
            {
                s += "{";
                foreach (var key in meta.Keys)
                {
                    s += key + ":" + meta[key] + ",";
                }
                s = s.Substring(0, s.Length - 1) + "}";
            }
            return s;
        }

        protected void MetaFromArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string[] parts = args[i].Split('=');

                if (parts.Length != 2) throw new Exception
                    ("Expected 2 parts, found " + parts.Length + ": " + parts);

                SetMeta(parts[0].Trim(), parts[1].Trim());
            }
        }
    }

}