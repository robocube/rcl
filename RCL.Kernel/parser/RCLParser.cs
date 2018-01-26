
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
    protected internal static RCLexer m_o2Lexer;
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
      m_o2Lexer = new RCLexer (types);
    }

    public RCLParser (RCActivator activator)
    {
      if (activator == null)
      {
        throw new ArgumentNullException ("activator");
      }
      m_activator = activator;
      m_lefts.Push (new Stack<RCValue> ());
      m_operators.Push (new Stack<RCValue> ());
      m_lexer = m_o2Lexer;
    }

    /// <summary>
    /// Parse a given set of tokens, generally aquired using RCLexer.
    /// </summary>
    public override RCValue Parse (RCArray<RCToken> tokens, out bool fragment)
    {
      for (int i = 0; i < tokens.Count; ++i)
      {
        tokens[i].Type.Accept (this, tokens[i]);
      }
      if (m_vector != null || m_reference != null)
      {
        FinishValue (false);
      }
      if (m_operators.Count > 0)
      {
        MakeExpression ();
      } 
      fragment = FinishBlock ();
      if (m_block != null)
      {
        return m_block;
      }
      else
      {
        return m_result;
      }
    }

    /// <summary>
    /// The expression currently being appended to.
    /// </summary>
    protected RCValue m_result;
  
    /// <summary>
    /// Activator is used to create operator instances.
    /// </summary>
    protected RCActivator m_activator;
  
    /// <summary>
    /// The block currently under construction.
    /// </summary>
    protected RCBlock m_block;
  
    /// <summary>
    /// The most recently seen variable name.
    /// </summary>
    protected string m_variable;
  
    /// <summary>
    /// The most recently seen evaluator.
    /// </summary>
    protected RCEvaluator m_evaluator;
  
    /// <summary>
    /// While constructing a nested block we need these explicit stacks
    /// because RCBlock has a forward reference to the value being referred to.
    /// I considered building everything including blocks as one big backwards
    /// chain but decided that it would make evaluating blocks either slow or
    /// messy garbage-wise.  I also didn't want to create blocks empty and fill
    /// in the values with assignment later on.
    /// </summary>
    protected Stack<string> m_variables = new Stack<string> ();
    protected Stack<RCEvaluator> m_evaluators = new Stack<RCEvaluator> ();
  
    /// <summary>
    /// The parents of the block currently under construction,
    /// to which m_block will be added when it is done.
    /// </summary>
    protected Stack<RCBlock> m_blocks = new Stack<RCBlock> ();
  
    /// <summary>
    /// The vector currently being read, if any.
    /// </summary>
    protected RCArray<RCToken> m_vector;
  
    /// <summary>
    /// The reference currently being read, if any.
    /// </summary>
    protected RCToken m_reference;

    /// <summary>
    /// The text of a symbol that could either be an operator or a variable.
    /// </summary>
    protected string m_maybeOperator;
  
    /// <summary>
    /// The left argument of an expression being processed.
    /// </summary>
    protected RCValue m_left;
  
    /// <summary>
    /// Similar to the way blocks are parsed, we need two stacks to 
    /// store the left arguments and the names of the operators as we are 
    /// descending an expression tree.
    /// </summary>
    protected Stack<Stack<RCValue>> m_lefts = new Stack<Stack<RCValue>> ();
    protected Stack<Stack<RCValue>> m_operators = new Stack<Stack<RCValue>> ();

    protected class TemplateVars
    {
      public bool m_multilineTemplate = false;
      //public bool m_parsingContent = false;
      public int m_minSpaces = int.MaxValue;
    }
    //This stack holds a few extra variables required for parsing nested templates.
    protected Stack<TemplateVars> m_templates = new Stack<TemplateVars> ();

    protected RCActivator.ParserExtension m_extension = null;
    protected RCActivator.ParserState m_extarg = null;

    public override void AcceptName (RCToken token)
    {
      if (m_extension != null)
      {
        m_extension.AcceptName (m_extarg, token);
      }
      else
      {
        if (m_maybeOperator != null)
        {
          PushArgument ();
        }
        PushExpression ();
        m_maybeOperator = token.Text;
        FinishValue (true);
      }
    }
  
    public override void AcceptReference (RCToken token)
    {
      PushArgument ();
      PushExpression ();
      m_reference = token;
    }
  
    public override void AcceptParen (RCToken token)
    {
      if (token.Text == "(")
      {
        //( Oddly has no meaning.
        PushArgument ();
        PushExpression ();
        m_lefts.Push (new Stack<RCValue> ());
        m_operators.Push (new Stack<RCValue> ());
      }
      else if (token.Text == ")")
      {
        FinishValue (true);
        MakeExpression ();
        m_lefts.Pop ();
        m_operators.Pop ();
      }
      else throw new ArgumentException (token.Text);
    }
  
    public override void AcceptScalar (RCToken token)
    {
      if (m_extension != null)
      {
        m_extension.AcceptScalar (m_extarg, token, m_lexer);
      }
      else
      {
        PushInlineMonadicOperator ();
        PushArgument ();
        PushExpression ();
        if (m_vector == null)
        {
          m_vector = new RCArray<RCToken> ();
        }
        m_vector.Write (token);
      }
    }

    protected void PushArgument ()
    {
      //Any right argument that was already created must 
      //actually be a left argument.
      if (m_result != null)
      {
        m_left = m_result;
        m_result = null;
      }
    }
  
    protected void PushExpression ()
    {
      //Any symbol seen before this must be an operator.
      if (m_maybeOperator != null)
      {
        m_operators.Peek ().Push (new RCReference (m_maybeOperator));
        m_lefts.Peek ().Push (m_left);
        m_maybeOperator = null;
        m_left = null;
      }
    }
  
    protected void PushInlineDyadicOperator ()
    {
      if (m_left != null)
      {
        m_lefts.Peek ().Push (m_left);
      }
    }
  
    protected void PushInlineMonadicOperator ()
    {
      if (m_result != null && m_maybeOperator == null && m_result is RCBlock)
      {
        m_operators.Peek ().Push (m_result);
        m_maybeOperator = null;
        m_left = null;
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
      //instantiate vectors
      if (m_vector != null)
      {
        m_result = MakeVector (m_vector);
        if (flush)
        {
          m_vector = null;
        }
      }
      //instantiate references
      else if (m_reference != null)
      {
        m_result = MakeReference ();
        if (flush)
        {
          m_reference = null;
        }
      }
    }
  
    protected bool FinishBlock ()
    {
      //instantiate blocks
      if (m_variable != null && m_result != null)
      {
        m_block = new RCBlock (m_block, m_variable, m_evaluator, m_result);
        m_variable = null;
        return true;
        //m_evaluator = null;
      }
      return false;
    }
  
    public override void AcceptEvaluator (RCToken token)
    {
      FinishValue (true);
      MakeExpression ();
      //When this applies there is no actual opening bracket defining the block.
      //This happens in the shell.
      //I call it an "implied" block.
      if (m_block == null)
      {
        m_block = RCBlock.Empty;
      }
      FinishBlock ();
      if (m_maybeOperator != null)
      {
        //m_maybeOp is not an operator, its a variable.
        m_variable = m_maybeOperator;
        m_maybeOperator = null;
      }
      if (m_variable == null)
      {
        //It is an unnamed block.
        m_variable = "";
      }
      //If anything was left over on the right, it doesn't belong here.
      //Should this be an invalid state for the parser?
      m_result = null;
      m_evaluator = RCEvaluator.For (token.Text);
    }
  
    public override void AcceptWhitespace (RCToken token)
    {
  
    }

    public override void AcceptCube (RCToken token)
    {
      if (token.Text == "[")
      {
        FinishValue (true);
        PushArgument ();
        PushExpression ();
        m_extension = m_activator.ExtensionFor (token.Text);
        m_extarg = m_extension.StartParsing ();
      }
      else if (token.Text == "]")
      {
        m_result = m_extension.EndParsing (m_extarg);
        m_extension = null;
      }
      else throw new ArgumentException (token.Text);
    }

    public override void AcceptBlock (RCToken token)
    {
      bool isStartTemplate = token.Text.StartsWith ("[?");
      bool isEndTemplate = token.Text.EndsWith ("?]");
      int escapeCount = token.Text.Length - 1;
      if (token.Text == "{" || isStartTemplate)
      {
        FinishValue (true);
        PushArgument ();
        PushExpression ();
        PushInlineDyadicOperator ();
        m_lefts.Push (new Stack<RCValue> ());
        m_operators.Push (new Stack<RCValue> ());
        if (isStartTemplate)
        {
          m_templates.Push (new TemplateVars ());
        }
        if (m_block != null)
        {
          m_variables.Push (m_variable);
          m_evaluators.Push (m_evaluator);
          m_variable = null;
        }
        m_blocks.Push (m_block);
        m_block = RCBlock.Empty;
      }
      else if (token.Text == "}" || isEndTemplate)
      {
        FinishValue (true);
        MakeExpression ();
        FinishBlock ();
        if (m_variables.Count > 0)
        {
          m_variable = m_variables.Pop ();
          m_evaluator = m_evaluators.Pop ();
          if (isEndTemplate)
          {
            FinishTemplate (escapeCount);
            m_templates.Pop ();
          }
          else
          {
            m_result = m_block;
          }
          m_block = m_blocks.Pop ();
        }
        else
        {
          if (isEndTemplate)
          {
            FinishTemplate (escapeCount);
            m_templates.Pop ();
          }
          else
          {
            m_result = m_block;
          }
          m_block = null;
        }
        m_lefts.Pop ();
        m_operators.Pop ();
        //If the stack contains a null on top, that is the signal
        //that this block should be used as an operator.
        Stack<RCValue> operators = m_operators.Peek ();
        if (operators.Count > 0 && operators.Peek () == null)
        {
          operators.Pop ();
          operators.Push (m_result);
        }
      }
      else if (token.Text.StartsWith ("[!"))
      {
        FinishBlock ();
        m_result = null;
      }
      else if (token.Text.EndsWith ("!]"))
      {
        FinishValue (true);
        MakeExpression ();
        m_variable = "";
        m_evaluator = RCEvaluator.Let;
        FinishBlock ();
      }
      else throw new ArgumentException (token.Text);
    }

    protected void FinishTemplate (int escapeCount)
    {
      RCString section;
      TemplateVars template = m_templates.Peek ();
      if (template.m_multilineTemplate)
      {
        for (int i = 0; i < m_block.Count; ++i)
        {
          RCBlock current = m_block.GetName (i);
          section = current.Value as RCString;
          if (section != null && i % 2 == 0)
          {
            string content = section[0];
            int spaces = 0;
            bool broken = false;
            bool hasLine = false;
            int firstNewline = content.IndexOf ('\n');
            if (firstNewline > -1)
            {
              hasLine = true;
            }
            for (int j = firstNewline + 1; j < content.Length; ++j)
            {
              if (content[j] == ' ')
              {
                if (!broken)
                {
                  ++spaces;
                }
              }
              else if (content[j] == '\n')
              {
                if (j > 0)
                {
                  template.m_minSpaces = Math.Min (template.m_minSpaces, spaces);
                }
                spaces = 0;
                broken = false;
                hasLine = true;
              }
              else
              {
                broken = true;
              }
            }
            if (content.Length > 0 && 
                content[content.Length - 1] != '\n' &&
                hasLine &&
                i < m_block.Count - 1)
            {
              template.m_minSpaces = Math.Min (template.m_minSpaces, spaces);
            }
            if (template.m_minSpaces == int.MaxValue)
            {
              template.m_minSpaces = 0;
            }
          }
        }
        
        RCBlock final = RCBlock.Empty;
        //strip indentation (spaces only) from the lines in the content.
        //Ignore the first and last sections.
        for (int i = 0; i < m_block.Count; ++i)
        {
          RCBlock current = m_block.GetName (i);
          section = current.Value as RCString;
          if (section != null && i % 2 == 0)
          {
            string content = section [0];
            StringBuilder builder = new StringBuilder ();
            
            int start = 0, end = 0;
            //Skip past the initial newline in the first section.
            if (i == 0)
            {
              while (content [start] != '\n')
              {
                ++start;
              }
              ++start;
            }
            else
            {
              while (end < content.Length)
              {
                if (content [end] == '\n')
                {
                  ++end;
                  break;
                }
                ++end;
              }
              if (end < content.Length && end > 1 && content [end - 2] == '\r')
              {
                builder.AppendLine (content.Substring (start, (end - 2) - start));
              }
              else
              {
                builder.Append (content.Substring (start, end - start));
              }
              start = end;
            }
            end = start;
            
            GETLINE:
            start = end + template.m_minSpaces;
            
            while (end < content.Length)
            {
              if (content [end] == '\n')
              {
                ++end;
                break;
              }
              ++end;
            }
            
            //The problem is when the first character is a newline this gets fucked up.
            if (start < content.Length && end <= content.Length)
            {
              //string trimmed;
              if (end < start)
              {
                builder.Append (content.Substring (0, end));
              }
              else
              {
                if (content.Length > 1 && content [end - 2] == '\r')
                {
                  builder.AppendLine (content.Substring (start, (end - 2) - start));
                }
                else
                {
                  builder.Append (content.Substring (start, end - start));
                }
              }
              //builder.Append (trimmed);
              goto GETLINE;
            }
            else
            {
              final = new RCBlock (final, current.Name, current.Evaluator, new RCString (builder.ToString ()));
            }
          }
          else
          {
            final = new RCBlock (final, current.Name, current.Evaluator, current.Value);
          }
        }
        m_result = new RCTemplate (final, escapeCount, true);
      }
      else
      {
        m_result = new RCTemplate (m_block, escapeCount, false);
      }

      //The template must either be all on one line,
      // or the first and last lines with the [? and ?] tokens
      // must be free of any other content.
      //So this loop needs to find out whether there are any newlines.
      //If there are then we also need to find out where the
      //first non-white character is.

      //Reset state for the possible next template.
      //m_multilineTemplate = false;
      //m_parsingContent = false;
      //m_minSpaces = 0;
    }

    //protected bool m_parsingContent = false;
    //protected bool m_multilineTemplate = false;
    //protected int m_minSpaces = 0;
    protected static char[] CRLF = new char[] {'\r', '\n'};
    public override void AcceptContent (RCToken token)
    {
      TemplateVars template = m_templates.Peek ();
      int firstNewline = token.Text.IndexOfAny (CRLF);
      if (firstNewline > -1)
      {
        template.m_multilineTemplate = true;
      }
      //I want it to be AS IF we saw something like {:"foo bar with newlines"}
      //AcceptEvaluator (new RCToken (":", RCTokenType.Evaluator, token.Start, token.Index));
      //soo...
      m_variable = "";
      m_evaluator = RCEvaluator.Let;
      m_result = new RCString (token.Text);
    }
  
    public void AcceptError (RCToken token)
    {

    }
  
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
      //The table may have one or two of these pipes in the header
      //The pipes separate the three different types of columns:
      //  Time|Symbol|Data
      //Begin by assuming there is no time column.  
      //If another pipe appears later then the first one was intended 
      //for the time column.
      if (token.Text == "|")
      {
        m_extension.AcceptSpacer (m_extarg, token);
      }
      //I think a null should really be its own type of token.
      else if (token.Text == "--")
      {
        //m_extension.AcceptSpacer (m_extarg, token);
        AcceptScalar (token);
      }
    }
  
    public override void AcceptJunk (RCToken token)
    {
  
    }
  
    protected void MakeExpression ()
    {
      while (m_operators.Peek ().Count > 0)
      {
        RCValue op = m_operators.Peek ().Pop ();
        RCValue left = m_lefts.Peek ().Count > 0 ? m_lefts.Peek ().Pop () : null;
        //When references are parsed they never have a built in context.
        //Use the module operator to create one.
        m_result = op.AsOperator (m_activator, left, m_result);
      }
    }
  
    protected RCValue MakeReference ()
    {
      string[] typeAndName = m_reference.Text.Split ('$');
      //I want to change the reference syntax a little so that 
      //you can use the characters before the $ to tell the 
      //parser what kind of type the reference resolves to.
      //int$x would have to resolve to an int for example.
      //In nearly all cases though, the interpreter should be 
      //able to figure that out for you.
      //string type = typeAndName[0];
      //string[] name = typeAndName[1].Split('.');
  
      //return new RCReference(type, typeAndName[1]);
      return new RCReference (typeAndName[1]);
      //if (type.Length == 0)
      //{
      //  Type realType = InferType(name, m_reference.Text);
      //  return new RCReference(realType, name);
      //}
      //else
      //{
      //  return new RCReference(type, name);
      //}
    }
  
    /*
      protected Type InferType(string[] name, string original)
      {
      //Let's try to figure it out!
      RCValue target = null;
  
      //Try the block under construction.
      if (m_block != null)
      {
      target = m_block.Get(name);
      }
  
      //Try to find it higher up the stack.
      if (target == null)
      {
      RCBlock[] parents = m_blocks.ToArray();
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
      if (vector[0].Text[0] == '~')
      {
        switch (vector[0].Text[1])
        {
          case 'x' : return RCByte.Empty;
          case 'b' : return RCBoolean.Empty;
          case 'l' : return RCLong.Empty;
          case 'd' : return RCDouble.Empty;
          case 'm' : return RCDecimal.Empty;
          case 's' : return RCString.Empty;
          case 'y' : return RCSymbol.Empty;
          case 't' : return RCTime.Empty;
          case 'n' : return RCIncr.Empty;
          default : throw new Exception ("Unrecognized type code: " + vector[0].Text[1]);
        }
      }
      if (vector[0].Type == RCTokenType.Symbol)
      {
        RCArray<RCSymbolScalar> list = new RCArray<RCSymbolScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
          list.Write (vector[i].ParseSymbol (m_lexer));
        result = new RCSymbol (list);
      }
      else if (vector[0].Type == RCTokenType.String)
      {
        RCArray<string> list = new RCArray<string> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
          list.Write (vector[i].ParseString (m_lexer));
        result = new RCString (list);
      }
      else if (vector[0].Type == RCTokenType.Boolean)
      {
        RCArray<bool> list = new RCArray<bool> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
          list.Write (vector[i].ParseBoolean (m_lexer));
        result = new RCBoolean (list);
      }
      else if (vector[0].Type == RCTokenType.Symbol)
      {
        RCArray<RCSymbolScalar> list = new RCArray<RCSymbolScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
          list.Write (vector[i].ParseSymbol (m_lexer));
        result = new RCSymbol (list);
      }
      else if (vector[0].Type == RCTokenType.Incr)
      {
        RCArray<RCIncrScalar> list = new RCArray<RCIncrScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
          list.Write (vector[i].ParseIncr (m_lexer));
        result = new RCIncr (list);
      }
      else if (vector[0].Type == RCTokenType.Literal)
      {
        char type = vector[0].Text[1];
        switch (type)
        {
          case 'x':
            RCArray<byte> list = new RCArray<byte> (vector.Count);
            for (int i = 0; i < vector.Count; ++i)
              list.Write (vector[i].ParseByte (m_lexer));
            result = new RCByte (list);
            break;
          default : throw new Exception ("Unknown type specifier:" + type);
        }
      }
      else if (vector[0].Type == RCTokenType.Time)
      {
        RCArray<RCTimeScalar> list = new RCArray<RCTimeScalar> (vector.Count);
        for (int i = 0; i < vector.Count; ++i)
          list.Write (vector[i].ParseTime (m_lexer));
        result = new RCTime (list);
      }
      else if (vector[0].Type == RCTokenType.Number)
      {
        //have a look at the last character in the last token
        //if there is a type specifier there we will use it to 
        //create the appropriate type of vector.
        RCToken last = vector[vector.Count - 1];
        char type = last.Text[last.Text.Length - 1];
        if (type == 'l')
        {
          RCArray<long> list = new RCArray<long> (vector.Count);
          for (int i = 0; i < vector.Count; ++i)
            list.Write (vector[i].ParseLong (m_lexer));
          result = new RCLong (list);
        }
        if (type == 'd')
        {
          RCArray<double> list = new RCArray<double> (vector.Count);
          for (int i = 0; i < vector.Count; ++i)
            list.Write (vector[i].ParseDouble (m_lexer));
          result = new RCDouble (list);
        }
        else if (type == 'm')
        {
          RCArray<decimal> list = new RCArray<decimal> (vector.Count);
          for (int i = 0; i < vector.Count; ++i)
            list.Write (vector[i].ParseDecimal (m_lexer));
          result = new RCDecimal (list);
        }
        else //default to double
        {
          if (vector[0].Text.IndexOf ('.') > -1 || vector[0].Text == "NaN")
          {
            RCArray<double> list = new RCArray<double> (vector.Count);
            for (int i = 0; i < vector.Count; ++i)
              list.Write (vector[i].ParseDouble (m_lexer));
            result = new RCDouble (list);
          }
          else
          {
            RCArray<long> list = new RCArray<long> (vector.Count);
            for (int i = 0; i < vector.Count; ++i)
              list.Write (vector[i].ParseLong (m_lexer));
            result = new RCLong (list);
          }
        }
      }
      return result;
    }
  }
}
