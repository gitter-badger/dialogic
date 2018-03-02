//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.4
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Dialogic.g4 by ANTLR 4.6.4

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Dialogic.Antlr {
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.4")]
public partial class DialogicParser : Parser {
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, DELIM=11, LB=12, RB=13, SP=14, NEWLINE=15, OPS=16, WORD=17, COMMENT=18, 
		LINE_COMMENT=19, ERROR=20;
	public const int
		RULE_script = 0, RULE_line = 1, RULE_command = 2, RULE_args = 3, RULE_arg = 4, 
		RULE_meta = 5;
	public static readonly string[] ruleNames = {
		"script", "line", "command", "args", "arg", "meta"
	};

	private static readonly string[] _LiteralNames = {
		null, "'CHAT'", "'SAY'", "'WAIT'", "'DO'", "'ASK'", "'OPT'", "'GO'", "'FIND'", 
		"'SET'", "'GRAM'", null, "'{'", "'}'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, "DELIM", 
		"LB", "RB", "SP", "NEWLINE", "OPS", "WORD", "COMMENT", "LINE_COMMENT", 
		"ERROR"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[System.Obsolete("Use Vocabulary instead.")]
	public static readonly string[] tokenNames = GenerateTokenNames(DefaultVocabulary, _SymbolicNames.Length);

	private static string[] GenerateTokenNames(IVocabulary vocabulary, int length) {
		string[] tokenNames = new string[length];
		for (int i = 0; i < tokenNames.Length; i++) {
			tokenNames[i] = vocabulary.GetLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = vocabulary.GetSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}

		return tokenNames;
	}

	[System.Obsolete("Use IRecognizer.Vocabulary instead.")]
	public override string[] TokenNames
	{
		get
		{
			return tokenNames;
		}
	}

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Dialogic.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public DialogicParser(ITokenStream input)
		: base(input)
	{
		_interp = new ParserATNSimulator(this,_ATN);
	}
	public partial class ScriptContext : ParserRuleContext {
		public LineContext[] line() {
			return GetRuleContexts<LineContext>();
		}
		public LineContext line(int i) {
			return GetRuleContext<LineContext>(i);
		}
		public ScriptContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_script; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IDialogicVisitor<TResult> typedVisitor = visitor as IDialogicVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitScript(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ScriptContext script() {
		ScriptContext _localctx = new ScriptContext(_ctx, State);
		EnterRule(_localctx, 0, RULE_script);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 13;
			_errHandler.Sync(this);
			_la = _input.La(1);
			do {
				{
				{
				State = 12; line();
				}
				}
				State = 15;
				_errHandler.Sync(this);
				_la = _input.La(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << T__0) | (1L << T__1) | (1L << T__2) | (1L << T__3) | (1L << T__4) | (1L << T__5) | (1L << T__6) | (1L << T__7) | (1L << T__8) | (1L << T__9))) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class LineContext : ParserRuleContext {
		public ITerminalNode NEWLINE() { return GetToken(DialogicParser.NEWLINE, 0); }
		public ITerminalNode Eof() { return GetToken(DialogicParser.Eof, 0); }
		public CommandContext command() {
			return GetRuleContext<CommandContext>(0);
		}
		public ArgsContext args() {
			return GetRuleContext<ArgsContext>(0);
		}
		public ITerminalNode[] SP() { return GetTokens(DialogicParser.SP); }
		public ITerminalNode SP(int i) {
			return GetToken(DialogicParser.SP, i);
		}
		public LineContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_line; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IDialogicVisitor<TResult> typedVisitor = visitor as IDialogicVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitLine(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public LineContext line() {
		LineContext _localctx = new LineContext(_ctx, State);
		EnterRule(_localctx, 2, RULE_line);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 32;
			_errHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(_input,3,_ctx) ) {
			case 1:
				{
				State = 17; command();
				State = 21;
				_errHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(_input,1,_ctx);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.InvalidAltNumber ) {
					if ( _alt==1 ) {
						{
						{
						State = 18; Match(SP);
						}
						} 
					}
					State = 23;
					_errHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(_input,1,_ctx);
				}
				}
				break;

			case 2:
				{
				State = 24; command();
				State = 26;
				_errHandler.Sync(this);
				_la = _input.La(1);
				do {
					{
					{
					State = 25; Match(SP);
					}
					}
					State = 28;
					_errHandler.Sync(this);
					_la = _input.La(1);
				} while ( _la==SP );
				State = 30; args();
				}
				break;
			}
			State = 37;
			_errHandler.Sync(this);
			_la = _input.La(1);
			while (_la==SP) {
				{
				{
				State = 34; Match(SP);
				}
				}
				State = 39;
				_errHandler.Sync(this);
				_la = _input.La(1);
			}
			State = 40;
			_la = _input.La(1);
			if ( !(_la==Eof || _la==NEWLINE) ) {
			_errHandler.RecoverInline(this);
			} else {
				if (_input.La(1) == TokenConstants.Eof) {
					matchedEOF = true;
				}

				_errHandler.ReportMatch(this);
				Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class CommandContext : ParserRuleContext {
		public CommandContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_command; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IDialogicVisitor<TResult> typedVisitor = visitor as IDialogicVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitCommand(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public CommandContext command() {
		CommandContext _localctx = new CommandContext(_ctx, State);
		EnterRule(_localctx, 4, RULE_command);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 42;
			_la = _input.La(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << T__0) | (1L << T__1) | (1L << T__2) | (1L << T__3) | (1L << T__4) | (1L << T__5) | (1L << T__6) | (1L << T__7) | (1L << T__8) | (1L << T__9))) != 0)) ) {
			_errHandler.RecoverInline(this);
			} else {
				if (_input.La(1) == TokenConstants.Eof) {
					matchedEOF = true;
				}

				_errHandler.ReportMatch(this);
				Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArgsContext : ParserRuleContext {
		public ArgContext[] arg() {
			return GetRuleContexts<ArgContext>();
		}
		public ArgContext arg(int i) {
			return GetRuleContext<ArgContext>(i);
		}
		public ITerminalNode[] DELIM() { return GetTokens(DialogicParser.DELIM); }
		public ITerminalNode DELIM(int i) {
			return GetToken(DialogicParser.DELIM, i);
		}
		public ITerminalNode LB() { return GetToken(DialogicParser.LB, 0); }
		public MetaContext meta() {
			return GetRuleContext<MetaContext>(0);
		}
		public ITerminalNode RB() { return GetToken(DialogicParser.RB, 0); }
		public ITerminalNode[] SP() { return GetTokens(DialogicParser.SP); }
		public ITerminalNode SP(int i) {
			return GetToken(DialogicParser.SP, i);
		}
		public ArgsContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_args; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IDialogicVisitor<TResult> typedVisitor = visitor as IDialogicVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitArgs(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ArgsContext args() {
		ArgsContext _localctx = new ArgsContext(_ctx, State);
		EnterRule(_localctx, 6, RULE_args);
		int _la;
		try {
			State = 77;
			_errHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(_input,9,_ctx) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 45;
				_errHandler.Sync(this);
				_la = _input.La(1);
				if (_la==DELIM) {
					{
					State = 44; Match(DELIM);
					}
				}

				{
				State = 47; arg();
				State = 52;
				_errHandler.Sync(this);
				_la = _input.La(1);
				while (_la==DELIM) {
					{
					{
					State = 48; Match(DELIM);
					State = 49; arg();
					}
					}
					State = 54;
					_errHandler.Sync(this);
					_la = _input.La(1);
				}
				}
				}
				break;

			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				{
				State = 55; Match(LB);
				State = 56; meta();
				State = 57; Match(RB);
				}
				}
				break;

			case 3:
				EnterOuterAlt(_localctx, 3);
				{
				{
				State = 59; arg();
				State = 64;
				_errHandler.Sync(this);
				_la = _input.La(1);
				while (_la==DELIM) {
					{
					{
					State = 60; Match(DELIM);
					State = 61; arg();
					}
					}
					State = 66;
					_errHandler.Sync(this);
					_la = _input.La(1);
				}
				}
				State = 70;
				_errHandler.Sync(this);
				_la = _input.La(1);
				while (_la==SP) {
					{
					{
					State = 67; Match(SP);
					}
					}
					State = 72;
					_errHandler.Sync(this);
					_la = _input.La(1);
				}
				{
				State = 73; Match(LB);
				State = 74; meta();
				State = 75; Match(RB);
				}
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArgContext : ParserRuleContext {
		public ITerminalNode[] WORD() { return GetTokens(DialogicParser.WORD); }
		public ITerminalNode WORD(int i) {
			return GetToken(DialogicParser.WORD, i);
		}
		public ITerminalNode[] SP() { return GetTokens(DialogicParser.SP); }
		public ITerminalNode SP(int i) {
			return GetToken(DialogicParser.SP, i);
		}
		public ArgContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_arg; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IDialogicVisitor<TResult> typedVisitor = visitor as IDialogicVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitArg(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ArgContext arg() {
		ArgContext _localctx = new ArgContext(_ctx, State);
		EnterRule(_localctx, 8, RULE_arg);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			{
			State = 79; Match(WORD);
			State = 88;
			_errHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(_input,11,_ctx);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.InvalidAltNumber ) {
				if ( _alt==1 ) {
					{
					{
					State = 81;
					_errHandler.Sync(this);
					_la = _input.La(1);
					do {
						{
						{
						State = 80; Match(SP);
						}
						}
						State = 83;
						_errHandler.Sync(this);
						_la = _input.La(1);
					} while ( _la==SP );
					State = 85; Match(WORD);
					}
					} 
				}
				State = 90;
				_errHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(_input,11,_ctx);
			}
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class MetaContext : ParserRuleContext {
		public ITerminalNode[] SP() { return GetTokens(DialogicParser.SP); }
		public ITerminalNode SP(int i) {
			return GetToken(DialogicParser.SP, i);
		}
		public ITerminalNode[] WORD() { return GetTokens(DialogicParser.WORD); }
		public ITerminalNode WORD(int i) {
			return GetToken(DialogicParser.WORD, i);
		}
		public ITerminalNode[] OPS() { return GetTokens(DialogicParser.OPS); }
		public ITerminalNode OPS(int i) {
			return GetToken(DialogicParser.OPS, i);
		}
		public MetaContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_meta; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IDialogicVisitor<TResult> typedVisitor = visitor as IDialogicVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitMeta(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public MetaContext meta() {
		MetaContext _localctx = new MetaContext(_ctx, State);
		EnterRule(_localctx, 10, RULE_meta);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 94;
			_errHandler.Sync(this);
			_la = _input.La(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << SP) | (1L << OPS) | (1L << WORD))) != 0)) {
				{
				{
				State = 91;
				_la = _input.La(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << SP) | (1L << OPS) | (1L << WORD))) != 0)) ) {
				_errHandler.RecoverInline(this);
				} else {
					if (_input.La(1) == TokenConstants.Eof) {
						matchedEOF = true;
					}

					_errHandler.ReportMatch(this);
					Consume();
				}
				}
				}
				State = 96;
				_errHandler.Sync(this);
				_la = _input.La(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.ReportError(this, re);
			_errHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x3\x16\x64\x4\x2\t"+
		"\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x3\x2\x6\x2\x10"+
		"\n\x2\r\x2\xE\x2\x11\x3\x3\x3\x3\a\x3\x16\n\x3\f\x3\xE\x3\x19\v\x3\x3"+
		"\x3\x3\x3\x6\x3\x1D\n\x3\r\x3\xE\x3\x1E\x3\x3\x3\x3\x5\x3#\n\x3\x3\x3"+
		"\a\x3&\n\x3\f\x3\xE\x3)\v\x3\x3\x3\x3\x3\x3\x4\x3\x4\x3\x5\x5\x5\x30\n"+
		"\x5\x3\x5\x3\x5\x3\x5\a\x5\x35\n\x5\f\x5\xE\x5\x38\v\x5\x3\x5\x3\x5\x3"+
		"\x5\x3\x5\x3\x5\x3\x5\x3\x5\a\x5\x41\n\x5\f\x5\xE\x5\x44\v\x5\x3\x5\a"+
		"\x5G\n\x5\f\x5\xE\x5J\v\x5\x3\x5\x3\x5\x3\x5\x3\x5\x5\x5P\n\x5\x3\x6\x3"+
		"\x6\x6\x6T\n\x6\r\x6\xE\x6U\x3\x6\a\x6Y\n\x6\f\x6\xE\x6\\\v\x6\x3\a\a"+
		"\a_\n\a\f\a\xE\a\x62\v\a\x3\a\x2\x2\x2\b\x2\x2\x4\x2\x6\x2\b\x2\n\x2\f"+
		"\x2\x2\x5\x3\x3\x11\x11\x3\x2\x3\f\x4\x2\x10\x10\x12\x13k\x2\xF\x3\x2"+
		"\x2\x2\x4\"\x3\x2\x2\x2\x6,\x3\x2\x2\x2\bO\x3\x2\x2\x2\nQ\x3\x2\x2\x2"+
		"\f`\x3\x2\x2\x2\xE\x10\x5\x4\x3\x2\xF\xE\x3\x2\x2\x2\x10\x11\x3\x2\x2"+
		"\x2\x11\xF\x3\x2\x2\x2\x11\x12\x3\x2\x2\x2\x12\x3\x3\x2\x2\x2\x13\x17"+
		"\x5\x6\x4\x2\x14\x16\a\x10\x2\x2\x15\x14\x3\x2\x2\x2\x16\x19\x3\x2\x2"+
		"\x2\x17\x15\x3\x2\x2\x2\x17\x18\x3\x2\x2\x2\x18#\x3\x2\x2\x2\x19\x17\x3"+
		"\x2\x2\x2\x1A\x1C\x5\x6\x4\x2\x1B\x1D\a\x10\x2\x2\x1C\x1B\x3\x2\x2\x2"+
		"\x1D\x1E\x3\x2\x2\x2\x1E\x1C\x3\x2\x2\x2\x1E\x1F\x3\x2\x2\x2\x1F \x3\x2"+
		"\x2\x2 !\x5\b\x5\x2!#\x3\x2\x2\x2\"\x13\x3\x2\x2\x2\"\x1A\x3\x2\x2\x2"+
		"#\'\x3\x2\x2\x2$&\a\x10\x2\x2%$\x3\x2\x2\x2&)\x3\x2\x2\x2\'%\x3\x2\x2"+
		"\x2\'(\x3\x2\x2\x2(*\x3\x2\x2\x2)\'\x3\x2\x2\x2*+\t\x2\x2\x2+\x5\x3\x2"+
		"\x2\x2,-\t\x3\x2\x2-\a\x3\x2\x2\x2.\x30\a\r\x2\x2/.\x3\x2\x2\x2/\x30\x3"+
		"\x2\x2\x2\x30\x31\x3\x2\x2\x2\x31\x36\x5\n\x6\x2\x32\x33\a\r\x2\x2\x33"+
		"\x35\x5\n\x6\x2\x34\x32\x3\x2\x2\x2\x35\x38\x3\x2\x2\x2\x36\x34\x3\x2"+
		"\x2\x2\x36\x37\x3\x2\x2\x2\x37P\x3\x2\x2\x2\x38\x36\x3\x2\x2\x2\x39:\a"+
		"\xE\x2\x2:;\x5\f\a\x2;<\a\xF\x2\x2<P\x3\x2\x2\x2=\x42\x5\n\x6\x2>?\a\r"+
		"\x2\x2?\x41\x5\n\x6\x2@>\x3\x2\x2\x2\x41\x44\x3\x2\x2\x2\x42@\x3\x2\x2"+
		"\x2\x42\x43\x3\x2\x2\x2\x43H\x3\x2\x2\x2\x44\x42\x3\x2\x2\x2\x45G\a\x10"+
		"\x2\x2\x46\x45\x3\x2\x2\x2GJ\x3\x2\x2\x2H\x46\x3\x2\x2\x2HI\x3\x2\x2\x2"+
		"IK\x3\x2\x2\x2JH\x3\x2\x2\x2KL\a\xE\x2\x2LM\x5\f\a\x2MN\a\xF\x2\x2NP\x3"+
		"\x2\x2\x2O/\x3\x2\x2\x2O\x39\x3\x2\x2\x2O=\x3\x2\x2\x2P\t\x3\x2\x2\x2"+
		"QZ\a\x13\x2\x2RT\a\x10\x2\x2SR\x3\x2\x2\x2TU\x3\x2\x2\x2US\x3\x2\x2\x2"+
		"UV\x3\x2\x2\x2VW\x3\x2\x2\x2WY\a\x13\x2\x2XS\x3\x2\x2\x2Y\\\x3\x2\x2\x2"+
		"ZX\x3\x2\x2\x2Z[\x3\x2\x2\x2[\v\x3\x2\x2\x2\\Z\x3\x2\x2\x2]_\t\x4\x2\x2"+
		"^]\x3\x2\x2\x2_\x62\x3\x2\x2\x2`^\x3\x2\x2\x2`\x61\x3\x2\x2\x2\x61\r\x3"+
		"\x2\x2\x2\x62`\x3\x2\x2\x2\xF\x11\x17\x1E\"\'/\x36\x42HOUZ`";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace Dialogic.Antlr
