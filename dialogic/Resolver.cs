﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dialogic
{
    /// <summary>
    /// Handles resolution of variables, probabilistic groups, and grammar rules
    /// </summary>
    public static class Resolver
    {
        public static bool DBUG = false;

        internal static IDictionary<string, Func<string, string>> ModTable
            = new Dictionary<string, Func<string, string>>
           {
               { "capitalize",    Modifier.Capitalize },
               { "quotify",       Modifier.Quotify },
               { "allCaps",       Modifier.AllCaps }
           };

        /// <summary>
        /// Iteratively resolve any variables or groups in the specified text 
        /// in the appropriate context
        /// </summary>
        public static string Bind(string text, Chat parent, IDictionary<string, object> globals)
        {
            if (text.IsNullOrEmpty() || !IsDynamic(text)) return text;

            if (DBUG) Console.WriteLine("--------------------------------\nBind: " + text);

            var original = text;
            int depth = 0, maxRecursionDepth = Defaults.BIND_MAX_DEPTH;

            do
            {
                // resolve any symbols in the input
                text = BindSymbols(text, parent, globals, depth);

                // resolve any groups in the input
                text = BindGroups(text, parent, depth);

                // throw if we've hit max recursion depth
                if (++depth > maxRecursionDepth)
                {
                    if (text.Contains(Ch.SYMBOL) || text.Contains(Ch.LABEL))
                    {
                        var symbols = Symbol.Parse(text, false);
                        if (!symbols.IsNullOrEmpty())
                        {
                            throw new UnboundSymbolException
                                (symbols[0], parent, globals);
                        }
                    }
                    throw new BindException
                        ("Resolver hit maxRecursionDepth for: " + original);
                }

            } while (IsDynamic(text));

            if (DBUG) Console.WriteLine("Result: " + text + "\n");

            return Html.Decode(text); // resolve any encoded entities
        }

        //// TODO: Add to API (ChatRuntime)
        //internal static void AddModifier(string name, Func<string, string> func, string alias = null)
        //{
        //    ModTable[name] = func;
        //}

        ///// <summary>
        ///// Iteratively resolve any variables in the specified text 
        ///// in the appropriate context
        ///// </summary>
        //public static string BindSymbols(string text, Chat context,
        //    IDictionary<string, object> globals, int level = 0)
        //{
        //    var symbols = Symbol.Parse(text, true);
        //    foreach (var sym in symbols)
        //    {


        //        bool newContext = false;
        //        string toReplace = null;
        //        string replaceWith = null;
        //        string[] parts = null;

        //        switch (sym.Type())
        //        {

        //            case SymbolType.SIMPLE:
        //                var tmp1 = ResolveSymbol(sym.symbol, context, globals);
        //                if (tmp1 != null) replaceWith = tmp1.ToString();
        //                // may be null here for an unrealized alias
        //                break;

        //            case SymbolType.CHAT_SCOPE:
        //                parts = sym.symbol.Split(Ch.SCOPE);
        //                if (parts.Length != 2) throw new ResolverException
        //                    ("Invalid symbol: " + sym);
        //                if (context == null) throw new ResolverException
        //                    ("Null local context: " + sym);
        //                context = context.runtime.FindChatByLabel(parts[0]);
        //                var tmp2 = ResolveSymbol(parts[1], context, globals);
        //                if (tmp2 != null) replaceWith = tmp2.ToString();
        //                newContext = true;
        //                break;

        //            case SymbolType.GLOBAL_SCOPE:
        //                parts = sym.symbol.Split(Ch.SCOPE);
        //                if (parts.Length < 2) throw new ResolverException
        //                    ("Invalid symbol: " + sym);
        //                replaceWith = ResolveObject(parts, globals).ToString();
        //                break;
        //        }

        //        if (replaceWith != null)
        //        {
        //            text = text.Replace(toReplace, replaceWith);
        //        }
        //    }
        //}

        /// <summary>
        /// Iteratively resolve any variables in the specified text 
        /// in the appropriate context
        /// </summary>
        public static string BindSymbols(string text, Chat context,
            IDictionary<string, object> globals, int level = 0)
        {
            var doRepeat = false;           // needs some cleanup
            do
            {
                var symbols = Symbol.Parse(text, true);

                if (DBUG)
                {
                    var s = "  BindSymbols(" + level + ") -> " + text;
                    s += "\n  Symbols(" + symbols.Count + "): ";
                    symbols.ForEach(sym => s += "$" + sym.symbol);
                    Console.WriteLine(s);
                }

                doRepeat = false;
                foreach (var sym in symbols)
                {
                    string theSymbol = sym.symbol;
                    string replaceWith = null;
                    bool switched = false;

                    // 3 cases for each: 1. simple-global, 2. global-object, 3. simple-local 

                    // We have a scoped symbol to process ($a.b || #a.b)
                    if (theSymbol.Contains(Ch.SCOPE))
                    {
                        var parts = theSymbol.Split(Ch.SCOPE);
                        if (parts.Length < 2) throw new BindException
                            ("Invalid symbol: " + sym);

                        //theSymbol = parts[1];

                        if (!sym.chatScoped) // global-traversal  ($a.b)
                        {

                            // SAVE that we are working in global scope if we have an alias

                            var result = ResolveObject(parts, globals);
                            if (result == null)
                            {
                                throw new UnboundSymbolException(sym, context, globals);
                            }
                            replaceWith = result.ToString();
                        }
                        else                // chat-scoped (#a.b)
                        {
                            if (context == null) throw new BindException
                                ("Null context for chat-scoped symbol: " + sym);

                            var chat = context.runtime.FindChatByLabel(parts[0]);

                            // reset the context to the chat
                            context = chat;
                            switched = true;
                        }
                    }
                    else // unscoped global ($a)
                    {
                        var tmp = ResolveSymbol(theSymbol, context, globals);
                        if (tmp != null) replaceWith = tmp.ToString();
                    }

                    //if (replaceWith == null)
                    //{
                    //    // lookup the value for the symbol
                    //    var tmp = ResolveSymbol(theSymbol, context, globals);
                    //    if (tmp != null) replaceWith = tmp.ToString();
                    //    // note: may be null here if an unresolved alias 
                    //}

                    if (replaceWith != null)
                    {
                        // if we've switched contexts and still have replacements
                        // to do, we need to repeat (perhaps better as recursive call?)
                        if (switched && replaceWith.Contains(Ch.SYMBOL)) doRepeat = true;

                        var toReplace = sym.text;

                        // if we have an alias, then include it in our resolved 
                        // value so that it can be handled properly in BindGroups
                        if (sym.alias != null)
                        {
                            if (replaceWith.Contains(Ch.OR))
                            {
                                toReplace = sym.SymbolText();
                            }
                            // TODO: working here
                            // we can't replace the full text without putting the alias in scope
                            // here we need to put the alias into scope


                        }

                        // do the symbol replacement
                        text = text.Replace(toReplace, replaceWith);

                        if (DBUG)
                        {
                            Console.WriteLine("    " + toReplace + " -> '" + replaceWith + "'");
                            if (doRepeat) Console.WriteLine
                                ("    Repeat with context=#" + context.text + " -> " + text);
                        }
                    }

                } // end foreach

            } while (doRepeat);


            return text;
        }

        /// <summary>
        /// Dynamically resolve the path through the object's 
        /// properties in global scope
        /// </summary>
        private static object ResolveObject(string[] parts, IDictionary<string, object> globals)
        {
            // TODO: NEED TO HANDLE FUNCTIONS IN HERE

            var obj = ResolveSymbol(parts[0], null, globals);

            if (obj == null) return null;

            for (int i = 1; i < parts.Length; i++)
            {
                obj = Properties.Get(obj, parts[i]);
                if (obj == null) throw new UnboundSymbolException
                    (string.Join(Ch.SCOPE.ToString(), parts), null, globals);
            }

            return obj;
        }

        /// <summary>
        /// Iteratively resolve any groups in the specified text 
        /// in the appropriate context, creating and caching Resolution
        /// objects as necessary
        /// </summary>
        public static string BindGroups(string text, Chat context = null, int level = 0)
        {
            if (text.Contains(Ch.OR))
            {
                if (DBUG) Console.WriteLine("  BindSymbols(" + level + ") -> " + text);

                var original = text;

                if (!(text.Contains(Ch.OGROUP) && text.Contains(Ch.CGROUP)))
                {
                    text = Ch.OGROUP + text + Ch.CGROUP;
                    Console.WriteLine("[WARN] BindGroups added parens to: " + text);
                }

                List<string> groups = new List<string>();
                ParseGroups(text, groups);

                if (DBUG) Console.WriteLine("    Groups: " + groups.Stringify());

                foreach (var opt in groups)
                {
                    var pick = Resolution.Choose(opt);
                    if (DBUG) Console.WriteLine("      " + opt + " -> " + pick);
                    text = text.ReplaceFirst(opt, pick);
                }

                var match = RE.ParseAlias.Match(text);
                if (match != null && match.Groups.Count == 3)
                {
                    var full = match.Groups[0].Value;
                    var symbol = match.Groups[1].Value;
                    var choice = match.Groups[2].Value;

                    // store the alias as a symbol in current scope
                    //context.scope[match.Groups[1].Value] = choice;
                    Assignment.EQ.Invoke(symbol, choice, context.scope);

                    // now do the replacement
                    text = text.ReplaceFirst(full, choice);

                    if (DBUG) Console.WriteLine("      " + full + " -> " + choice
                        + "\n      scope: " + context.scope.Stringify());
                }
            }

            return text;
        }

        /// <summary>
        /// Handles Chat-scoping of variables by updating symbol name and switching to specified context
        /// </summary>
        internal static bool ContextSwitch(ref string symbol, ref Chat context)
        {
            if (symbol.Contains(Ch.SCOPE))
            {
                if (context == null) throw new BindException
                    ("Null context for chat-scoped symbol: " + symbol);

                // need to process a chat-scoped symbol
                var parts = symbol.Split(Ch.SCOPE);
                if (parts.Length > 2) throw new BindException
                    ("Unexpected variable format: " + symbol);

                var chat = context.runtime.FindChatByLabel(parts[0]);
                if (chat == null) throw new BindException
                    ("No Chat found with label #" + parts[0]);

                symbol = parts[1];
                context = chat;

                return true;
            }

            return false;
        }

        /// <summary>
        /// First do local lookup from Chat context, then if not found, try global lookup
        /// </summary>
        private static object ResolveSymbol(string symbol, Chat context,
            IDictionary<string, object> globals)
        {
            object result = null;
            if (context != null && context.scope.ContainsKey(symbol)) // check locals
            {
                result = context.scope[symbol];
            }
            else if (globals != null && globals.ContainsKey(symbol))   // check globals
            {
                result = globals[symbol];
            }
            return result;
        }

        private static void ParseGroups(string input, List<string> results)
        {
            foreach (Match m in RE.MatchParens.Matches(input))
            {
                var expr = m.Value.Substring(1, m.Value.Length - 2);

                if (RE.HasParens.IsMatch(expr))
                {
                    ParseGroups(expr, results);
                }
                else
                {
                    results.Add(m.Value);
                }
            }
        }

        private static bool IsDynamic(string text)
        {
            return text != null && text.Contains(Ch.OR, Ch.SYMBOL, Ch.LABEL);
        }

    }

    static class Modifier
    {
        /// <summary>
        /// Capitalize the first character.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The modified string</returns>
        public static string Capitalize(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Capitalize every character
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The modified string</returns>
        public static string AllCaps(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        /// <summary>
        /// Wraps the string in double-quotes
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The modified string</returns>
        public static string Quotify(string str)
        {
            return "\"" + str + "\"";
        }
    }
}