
using System;
using System.Text;
using System.Collections.Generic;

namespace RCL.Kernel
{
  /// <summary>
  /// A streaming parser for RCL.
  /// Breaking strings down into tokens is accomplished by RCLexer.
  /// RCParser takes the resulting token streams and converts them into RCValues.
  ///
  /// This means it needs to do macro instantiation, possibly type inference.
  /// </summary>
  public class RCLParser : RCParser
  {
    protected internal static RCLexer _o2Lexer;
    static RCLParser ()
    {
      RCArray<RCTokenType> types = new RCArray<RCTokenType> ();
      types.Write (RCTokenType.Content);
      types.Write (RCTokenType.WhiteSpace);
      types.Write (RCTokenType.EmptyVector);
      types.Write (RCTokenType.String);
      types.Write (RCTokenType.Time);
      types.Write (RCTokenType.Number);
      types.Write (RCTokenType.Boolean);
      types.Write (RCTokenType.Symbol);
      types.Write (RCTokenType.Incr);
      types.Write (RCTokenType.Literal);
      types.Write (RCTokenType.Paren);
      types.Write (RCTokenType.Block);
      types.Write (RCTokenType.Cube);
      types.Write (RCTokenType.Reference);
      types.Write (RCTokenType.Evaluator);
      types.Write (RCTokenType.Spacer);
      types.Write (RCTokenType.Name);
      types.Write (RCTokenType.Junk);
      _o2Lexer = new RCLexer (types);
    }

    public RCLParser (RCActivator activator)
    {
      if (activator == null) {
        throw new ArgumentNullException ("activator");
      }
      _activator = activator;
      _lefts.Push (new Stack<RCValue> ());
      _operators.Push (new Stack<RCValue> ());
      _lexer = _o2Lexer;
    }

    /// <summary>
    /// Parse a given set of tokens, generally acquired using RCLexer.
    /// </summary>
    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment, bool canonical)
    {
      _canonical = canonical;
      int i = 0;
      try
      {
        if (tokens.Count == 0) {
          fragment = true;
          return RCBlock.Empty;
        }
        for (i = 0; i < tokens.Count; ++i)
        {
          tokens[i].Type.Accept (this, tokens[i]);
        }
        if (_vector != null || _reference != null) {
          FinishValue (false);
        }
        if (_operators.Peek ().Count > 0) {
          MakeExpression ();
        }
        fragment = FinishBlock ();
        CheckForUnfinishedWork (tokens[tokens.Count - 1]);
        if (_block != null) {
          return _block;
        }
        else {
          return _result;
        }
      }
      catch (RCLSyntaxException)
      {
        // The code issue is handled manually within Parse.
        throw;
      }
      catch (Exception ex)
      {
        if (i < tokens.Count) {
          throw new RCLSyntaxException (tokens[i], ex);
        }
        else if (tokens.Count > 0 && i >= tokens.Count) {
          throw new RCLSyntaxException (tokens[tokens.Count - 1], ex);
        }
        else {
          // If for some reason we cannot obtain the token that broke the parsing process.
          throw;
        }
      }
    }

    /// <summary>
    /// The expression currently being appended to.
    /// </summary>
    protected RCValue _result;

    /// <summary>
    /// Activator is used to create operator instances.
    /// </summary>
    protected RCActivator _activator;

    /// <summary>
    /// The block currently under construction.
    /// </summary>
    protected RCBlock _block;

    /// <summary>
    /// The most recently seen variable name.
    /// </summary>
    protected string _variable;

    /// <summary>
    /// The most recently seen evaluator.
    /// </summary>
    protected RCEvaluator _evaluator;

    /// <summary>
    /// While constructing a nested block we need these explicit stacks
    /// because RCBlock has a forward reference to the value being referred to.
    /// I considered building everything including blocks as one big backwards
    /// chain but decided that it would make evaluating blocks either slow or
    /// messy garbage-wise.  I also didn't want to create blocks empty and fill
    /// in the values with assignment later on.
    /// </summary>
    protected Stack<string> _variables = new Stack<string> ();
    protected Stack<RCEvaluator> _evaluators = new Stack<RCEvaluator> ();

    /// <summary>
    /// The parents of the block currently under construction,
    /// to which _block will be added when it is done.
    /// </summary>
    protected Stack<RCBlock> _blocks = new Stack<RCBlock> ();

    /// <summary>
    /// The vector currently being read, if any.
    /// </summary>
    protected RCArray<RCToken> _vector;

    /// <summary>
    /// The reference currently being read, if any.
    /// </summary>
    protected RCToken _reference;

    /// <summary>
    /// The text of a symbol that could either be an operator or a variable.
    /// </summary>
    protected string _maybeOperator;

    /// <summary>
    /// The left argument of an expression being processed.
    /// </summary>
    protected RCValue _left;

    /// <summary>
    /// Similar to the way blocks are parsed, we need two stacks to
    /// store the left arguments and the names of the operators as we are
    /// descending an expression tree.
    /// </summary>
    protected Stack<Stack<RCValue>> _lefts = new Stack<Stack<RCValue>> ();
    protected Stack<Stack<RCValue>> _operators = new Stack<Stack<RCValue>> ();

    protected bool _canonical = false;

    protected class TemplateVars
    {
      public bool _multilineTemplate = false;
      public int _minSpaces = int.MaxValue;
    }
    // This stack holds a few extra variables required for parsing nested templates.
    protected Stack<TemplateVars> _templates = new Stack<TemplateVars> ();

    protected RCActivator.ParserExtension _extension = null;
    protected RCActivator.ParserState _extarg = null;

    public override void AcceptName (RCToken token)
    {
      if (_extension != null) {
        _extension.AcceptName (_extarg, token);
      }
      else {
        if (_maybeOperator != null) {
          PushArgument ();
        }
        PushExpression ();
        _maybeOperator = token.Text;
        FinishValue (true);
      }
    }

    public override void AcceptReference (RCToken token)
    {
      PushArgument ();
      PushExpression ();
      _reference = token;
    }

    public override void AcceptParen (RCToken token)
    {
      if (token.Text == "(") {
        // ( Oddly has no meaning.
        PushArgument ();
        PushExpression ();
        _lefts.Push (new Stack<RCValue> ());
        _operators.Push (new Stack<RCValue> ());
      }
      else if (token.Text == ")") {
        FinishValue (true);
        MakeExpression ();
        _lefts.Pop ();
        _operators.Pop ();
      }
      else {
        throw new ArgumentException (token.Text);
      }
    }

    public override void AcceptScalar (RCToken token)
    {
      if (_extension != null) {
        _extension.AcceptScalar (_extarg, token, _lexer);
      }
      else {
        PushInlineMonadicOperator ();
        PushArgument ();
        PushExpression ();
        if (_vector == null) {
          _vector = new RCArray<RCToken> ();
        }
        _vector.Write (token);
      }
    }

    protected void PushArgument ()
    {
      // Any right argument that was already created must
      // actually be a left argument.
      if (_result != null) {
        _left = _result;
        _result = null;
      }
    }

    protected void PushExpression ()
    {
      // Any symbol seen before this must be an operator.
      if (_maybeOperator != null) {
        _operators.Peek ().Push (new RCReference (_maybeOperator));
        _lefts.Peek ().Push (_left);
        _maybeOperator = null;
        _left = null;
      }
    }

    protected void PushInlineDyadicOperator ()
    {
      if (_left != null) {
        _lefts.Peek ().Push (_left);
        _left = null;
      }
    }

    protected void PushInlineMonadicOperator ()
    {
      if (_result != null && _maybeOperator == null && _result is RCBlock) {
        _operators.Peek ().Push (_result);
        _maybeOperator = null;
        _left = null;
      }
    }

    /// <summary>
    /// Vectors and references cannot be created until we know the parenting
    /// structure of surrounding operators.  You only know this once you see if
    /// parens follow the values.
    ///
    /// When this is called at the end of a series of tokens, do not destroy
    /// the underying input once the values are created.  That way more tokens
    /// can be added later on.
    /// </summary>
    protected void FinishValue (bool flush)
    {
      // instantiate vectors
      if (_vector != null) {
        _result = MakeVector (_vector);
        if (flush) {
          _vector = null;
        }
      }
      // instantiate references
      else if (_reference != null) {
        _result = MakeReference ();
        if (flush) {
          _reference = null;
        }
      }
    }

    protected bool FinishBlock ()
    {
      // instantiate blocks
      if (_variable != null && _result != null) {
        _block = new RCBlock (_block, _variable, _evaluator, _result);
        _variable = null;
        return true;
        // _evaluator = null;
      }
      return false;
    }

    protected void CheckForUnfinishedWork (RCToken token)
    {
      if (_maybeOperator != null) {
        throw new RCLSyntaxException (token, "Unfinished operator expression.");
      }
    }

    public override void AcceptEvaluator (RCToken token)
    {
      FinishValue (true);
      MakeExpression ();
      // When this applies there is no actual opening bracket defining the block.
      // This happens in the shell.
      // I call it an "implied" block.
      if (_block == null) {
        _block = RCBlock.Empty;
      }
      FinishBlock ();
      if (_maybeOperator != null) {
        // _maybeOp is not an operator, its a variable.
        _variable = _maybeOperator;
        _maybeOperator = null;
      }
      if (_variable == null) {
        // It is an unnamed block.
        _variable = "";
      }
      // If anything was left over on the right, it doesn't belong here.
      // Should this be an invalid state for the parser?
      _result = null;
      _evaluator = RCEvaluator.For (token.Text);
    }

    public override void AcceptWhitespace (RCToken token)
    {}

    public override void AcceptCube (RCToken token)
    {
      if (token.Text == "[") {
        FinishValue (true);
        PushArgument ();
        PushExpression ();
        _extension = _activator.ExtensionFor (token.Text);
        _extarg = _extension.StartParsing (_canonical);
      }
      else if (token.Text == "]") {
        _result = _extension.EndParsing (_extarg);
        _extension = null;
      }
      else {
        throw new ArgumentException (token.Text);
      }
    }

    public override void AcceptBlock (RCToken token)
    {
      bool isStartTemplate = token.Text.StartsWith ("[?");
      bool isEndTemplate = token.Text.EndsWith ("?]");
      int escapeCount = token.Text.Length - 1;
      if (token.Text == "{" || isStartTemplate) {
        FinishValue (true);
        PushArgument ();
        PushExpression ();
        PushInlineDyadicOperator ();
        _lefts.Push (new Stack<RCValue> ());
        _operators.Push (new Stack<RCValue> ());
        if (isStartTemplate) {
          _templates.Push (new TemplateVars ());
        }
        if (_block != null) {
          _variables.Push (_variable);
          _evaluators.Push (_evaluator);
          _variable = null;
        }
        _blocks.Push (_block);
        _block = RCBlock.Empty;
      }
      else if (token.Text == "}" || isEndTemplate) {
        FinishValue (true);
        MakeExpression ();
        FinishBlock ();
        if (_variables.Count > 0) {
          _variable = _variables.Pop ();
          _evaluator = _evaluators.Pop ();
          if (isEndTemplate) {
            FinishTemplate (escapeCount);
            _templates.Pop ();
          }
          else {
            _result = _block;
          }
          _block = _blocks.Pop ();
        }
        else {
          if (isEndTemplate) {
            FinishTemplate (escapeCount);
            _templates.Pop ();
          }
          else {
            _result = _block;
          }
          _block = null;
        }
        _lefts.Pop ();
        _operators.Pop ();
        // If the stack contains a null on top, that is the signal
        // that this block should be used as an operator.
        Stack<RCValue> operators = _operators.Peek ();
        if (operators.Count > 0 && operators.Peek () == null) {
          operators.Pop ();
          operators.Push (_result);
        }
      }
      else if (token.Text.StartsWith ("[!")) {
        FinishBlock ();
        _result = null;
      }
      else if (token.Text.EndsWith ("!]")) {
        FinishValue (true);
        MakeExpression ();
        _variable = "";
        _evaluator = RCEvaluator.Let;
        FinishBlock ();
      }
      else {
        throw new ArgumentException (token.Text);
      }
    }

    protected void FinishTemplate (int escapeCount)
    {
      RCString section;
      TemplateVars template = _templates.Peek ();
      if (template._multilineTemplate) {
        for (int i = 0; i < _block.Count; ++i)
        {
          RCBlock current = _block.GetName (i);
          section = current.Value as RCString;
          if (section != null && i % 2 == 0) {
            string content = section[0];
            int spaces = 0;
            bool broken = false;
            bool hasLine = false;
            int firstNewline = content.IndexOf ('\n');
            if (firstNewline > -1) {
              hasLine = true;
            }
            for (int j = firstNewline + 1; j < content.Length; ++j)
            {
              if (content[j] == ' ') {
                if (!broken) {
                  ++spaces;
                }
              }
              else if (content[j] == '\n') {
                if (j > 0) {
                  template._minSpaces = Math.Min (template._minSpaces, spaces);
                }
                spaces = 0;
                broken = false;
                hasLine = true;
              }
              else {
                broken = true;
              }
            }
            if (content.Length > 0 &&
                content[content.Length - 1] != '\n' &&
                hasLine &&
                i < _block.Count - 1) {
              template._minSpaces = Math.Min (template._minSpaces, spaces);
            }
            if (template._minSpaces == int.MaxValue) {
              template._minSpaces = 0;
            }
          }
        }

        RCBlock final = RCBlock.Empty;
        // strip indentation (spaces only) from the lines in the content.
        // Ignore the first and last sections.
        for (int i = 0; i < _block.Count; ++i)
        {
          RCBlock current = _block.GetName (i);
          section = current.Value as RCString;
          if (section != null && i % 2 == 0) {
            string content = section[0];
            StringBuilder builder = new StringBuilder ();

            int start = 0, end = 0;
            // Skip past the initial newline in the first section.
            if (i == 0) {
              while (content[start] != '\n')
              {
                ++start;
              }
              ++start;
            }
            else {
              while (end < content.Length)
              {
                if (content[end] == '\n') {
                  ++end;
                  break;
                }
                ++end;
              }
              if (end < content.Length && end > 1 && content[end - 2] == '\r') {
                builder.Append (content.Substring (start, (end - 2) - start));
                builder.Append ("\n");
              }
              else {
                builder.Append (content.Substring (start, end - start));
              }
              start = end;
            }
            end = start;

GETLINE:
            start = end + template._minSpaces;

            while (end < content.Length)
            {
              if (content[end] == '\n') {
                ++end;
                break;
              }
              ++end;
            }

            // The problem is when the first character is a newline this gets fucked up.
            if (start < content.Length && end <= content.Length) {
              // string trimmed;
              if (end < start) {
                builder.Append (content.Substring (0, end));
              }
              else {
                if (content.Length > 1 && content[end - 2] == '\r') {
                  builder.Append (content.Substring (start, (end - 2) - start));
                  builder.Append ("\n");
                }
                else {
                  builder.Append (content.Substring (start, end - start));
                }
              }
              // builder.Append (trimmed);
              goto GETLINE;
            }
            else {
              final = new RCBlock (final,
                                   current.Name,
                                   current.Evaluator,
                                   new RCString (builder.ToString ()));
            }
          }
          else {
            final = new RCBlock (final, current.Name, current.Evaluator, current.Value);
          }
        }
        _result = new RCTemplate (final, escapeCount, true);
      }
      else {
        _result = new RCTemplate (_block, escapeCount, false);
      }

      // The template must either be all on one line,
      // or the first and last lines with the [? and ?] tokens
      // must be free of any other content.
      // So this loop needs to find out whether there are any newlines.
      // If there are then we also need to find out where the
      // first non-white character is.

      // Reset state for the possible next template.
      // _multilineTemplate = false;
      // _parsingContent = false;
      // _minSpaces = 0;
    }

    // protected bool _parsingContent = false;
    // protected bool _multilineTemplate = false;
    // protected int _minSpaces = 0;
    protected static char[] CRLF = new char[] {'\r', '\n'};
    public override void AcceptContent (RCToken token)
    {
      TemplateVars template = _templates.Peek ();
      int firstNewline = token.Text.IndexOfAny (CRLF);
      if (firstNewline > -1) {
        template._multilineTemplate = true;
      }
      // I want it to be AS IF we saw something like {:"foo bar with newlines"}
      // AcceptEvaluator (new RCToken (":", RCTokenType.Evaluator, token.Start,
      // token.Index));
      // soo...
      _variable = "";
      _evaluator = RCEvaluator.Let;
      _result = new RCString (token.Text);
    }

    public void AcceptError (RCToken token)
    {}

    public override void AcceptNumber (RCToken token)
    {
      AcceptScalar (token);
    }

    public override void AcceptTime (RCToken token)
    {
      AcceptScalar (token);
    }

    public override void AcceptString (RCToken token)
    {
      AcceptScalar (token);
    }

    public override void AcceptBoolean (RCToken token)
    {
      AcceptScalar (token);
    }

    public override void AcceptIncr (RCToken token)
    {
      AcceptScalar (token);
    }

    public override void AcceptLiteral (RCToken token)
    {
      AcceptScalar (token);
    }

    public override void AcceptSpacer (RCToken token)
    {
      // The table may have one or two of these pipes in the header
      // The pipes separate the three different types of columns:
      //  Time|Symbol|Data
      // Begin by assuming there is no time column.
      // If another pipe appears later then the first one was intended
      // for the time column.
      if (token.Text == "|") {
        _extension.AcceptSpacer (_extarg, token);
      }
      // I think a null should really be its own type of token.
      else if (token.Text == "--") {
        AcceptScalar (token);
      }
    }

    public override void AcceptJunk (RCToken token)
    {}

    protected void MakeExpression ()
    {
      while (_operators.Peek ().Count > 0)
      {
        RCValue op = _operators.Peek ().Pop ();
        RCValue left = _lefts.Peek ().Count > 0 ? _lefts.Peek ().Pop () : null;
        _result = op.AsOperator (_activator, left, _result);
      }
    }

    protected RCValue MakeReference ()
    {
      string[] typeAndName = _reference.Text.Split ('$');
      // I want to change the reference syntax a little so that
      // you can use the characters before the $ to tell the
      // parser what kind of type the reference resolves to.
      // int$x would have to resolve to an int for example.
      // In nearly all cases though, the interpreter should be
      // able to figure that out for you.
      // string type = typeAndName[0];
      // string[] name = typeAndName[1].Split('.');

      // return new RCReference(type, typeAndName[1]);
      return new RCReference (typeAndName[1]);
      // if (type.Length == 0)
      // {
      //  Type realType = InferType(name, _reference.Text);
      //  return new RCReference(realType, name);
      // }
      // else
      // {
      //  return new RCReference(type, name);
      // }
    }

    /*
       protected Type InferType(string[] name, string original)
       {
       //Let's try to figure it out!
       RCValue target = null;

       //Try the block under construction.
       if (_block != null)
       {
       target = _block.Get(name);
       }

       //Try to find it higher up the stack.
       if (target == null)
       {
       RCBlock[] parents = _blocks.ToArray();
       //When you ToArray the stack items come out in the same order
       //you would have taken them off the stack.
       for (int i = 0; i < parents.Length; ++i)
       {
       //There will be null blocks on the stack.
       if (parents[i] != null)
       {
       target = parents[i].Get(name);
       if (target != null) break;
       }
       }
       }

       if (target == null)
       {
       throw new Exception("Unable to infer type for reference " + original + ".");
       }
       else return target.Yields;
       }
     */

    protected RCVectorBase MakeVector (RCArray<RCToken> vector)
    {
      RCVectorBase result = null;
      if (vector[0].Text[0] == '~') {
        switch (vector[0].Text[1])
        {
        case 'x': return RCByte.Empty;
        case 'b': return RCBoolean.Empty;
        case 'l': return RCLong.Empty;
        case 'd': return RCDouble.Empty;
        case 'm': return RCDecimal.Empty;
        case 's': return RCString.Empty;
        case 'y': return RCSymbol.Empty;
        case 't': return RCTime.Empty;
        case 'n': return RCIncr.Empty;
        default: throw new Exception ("Unrecognized type code: " + vector[0].Text[1]);
        }
      }
      if (vector[0].Type == RCTokenType.Symbol) {
        RCArray<RCSymbolScalar> list = new RCArray<RCSymbolScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i) {
          list.Write (vector[i].ParseSymbol (_lexer));
        }
        result = new RCSymbol (list);
      }
      else if (vector[0].Type == RCTokenType.String) {
        RCArray<string> list = new RCArray<string> (vector.Count);
        for (int i = 0; i < vector.Count; ++i) {
          list.Write (vector[i].ParseString (_lexer));
        }
        result = new RCString (list);
      }
      else if (vector[0].Type == RCTokenType.Boolean) {
        RCArray<bool> list = new RCArray<bool> (vector.Count);
        for (int i = 0; i < vector.Count; ++i) {
          list.Write (vector[i].ParseBoolean (_lexer));
        }
        result = new RCBoolean (list);
      }
      else if (vector[0].Type == RCTokenType.Incr) {
        RCArray<RCIncrScalar> list = new RCArray<RCIncrScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i) {
          list.Write (vector[i].ParseIncr (_lexer));
        }
        result = new RCIncr (list);
      }
      else if (vector[0].Type == RCTokenType.Literal) {
        char type = vector[0].Text[1];
        switch (type)
        {
        case 'x':
          RCArray<byte> list = new RCArray<byte> (vector.Count);
          for (int i = 0; i < vector.Count; ++i) {
            list.Write (vector[i].ParseByte (_lexer));
          }
          result = new RCByte (list);
          break;
        default: throw new Exception ("Unknown type specifier:" + type);
        }
      }
      else if (vector[0].Type == RCTokenType.Time) {
        RCArray<RCTimeScalar> list = new RCArray<RCTimeScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i) {
          list.Write (vector[i].ParseTime (_lexer));
        }
        result = new RCTime (list);
      }
      else if (vector[0].Type == RCTokenType.Number) {
        // have a look at the last character in the last token
        // if there is a type specifier there we will use it to
        // create the appropriate type of vector.
        RCToken last = vector[vector.Count - 1];
        char type = last.Text[last.Text.Length - 1];
        if (type == 'l') {
          RCArray<long> list = new RCArray<long> (vector.Count);
          for (int i = 0; i < vector.Count; ++i) {
            list.Write (vector[i].ParseLong (_lexer));
          }
          result = new RCLong (list);
        }
        if (type == 'd') {
          RCArray<double> list = new RCArray<double> (vector.Count);
          for (int i = 0; i < vector.Count; ++i) {
            list.Write (vector[i].ParseDouble (_lexer));
          }
          result = new RCDouble (list);
        }
        else if (type == 'm') {
          RCArray<decimal> list = new RCArray<decimal> (vector.Count);
          for (int i = 0; i < vector.Count; ++i) {
            list.Write (vector[i].ParseDecimal (_lexer));
          }
          result = new RCDecimal (list);
        }
        else {     // default to double
          if (vector[0].Text.IndexOf ('.') > -1 || vector[0].Text == "NaN") {
            RCArray<double> list = new RCArray<double> (vector.Count);
            for (int i = 0; i < vector.Count; ++i) {
              list.Write (vector[i].ParseDouble (_lexer));
            }
            result = new RCDouble (list);
          }
          else {
            RCArray<long> list = new RCArray<long> (vector.Count);
            for (int i = 0; i < vector.Count; ++i) {
              list.Write (vector[i].ParseLong (_lexer));
            }
            result = new RCLong (list);
          }
        }
      }
      return result;
    }
  }
}
