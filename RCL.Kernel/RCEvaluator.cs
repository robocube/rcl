
using System;

namespace RCL.Kernel
{
  public class RCEvaluator
  {
    public static readonly RCEvaluator Let, Quote, Yield, Yiote, Yiyi, Apply, Expand;

    public readonly string Symbol;
    public readonly bool Return;
    public readonly bool Invoke;
    public readonly bool Pass;
    public readonly bool Template;
    public readonly bool FinishBlock;
    public readonly RCEvaluator Next;

    static RCEvaluator ()
    {
      Let = new RCEvaluator (":", true, false, false, false, false, true, Let);
      Quote = new RCEvaluator ("::", false, false, false, false, true, true, Let);
      Yield = new RCEvaluator ("<-", true, true, false, false, false, false, Yield);
      Yiote = new RCEvaluator ("<-:", false, false, false, false, true, true, Yield);
      Yiyi = new RCEvaluator ("<--", true, false, false, false, false, true, Yield);
      Apply = new RCEvaluator ("<+", false, false, true, false, false, false, Apply);
      Expand = new RCEvaluator ("<&", false, true, false, true, false, false, Expand);
    }

    public RCEvaluator (string symbol,
                        bool evaluate,
                        bool @return,
                        bool invoke,
                        bool template,
                        bool pass,
                        bool finishBlock,
                        RCEvaluator next)
    {
      Symbol = symbol;
      Pass = pass;
      Return = @return;
      Invoke = invoke;
      Template = template;
      FinishBlock = finishBlock;
      Next = next == null ? this : next;
    }

    public static RCEvaluator For (string symbol)
    {
      if (symbol.Equals (":"))
      {
        return Let;
      }
      else if (symbol.Equals ("::"))
      {
        return Quote;
      }
      else if (symbol.Equals ("<-"))
      {
        return Yield;
      }
      else if (symbol.Equals ("<-:"))
      {
        return Yiote;
      }
      else if (symbol.Equals ("<--"))
      {
        return Yiyi;
      }
      else if (symbol.Equals ("<+"))
      {
        return Apply;
      }
      else if (symbol.Equals ("<&"))
      {
        return Expand;
      }
      throw new Exception ("Unknown evaluator: " + symbol);
    }
  }
}
