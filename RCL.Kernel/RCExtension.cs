
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace RCL.Kernel
{
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
  public class RCExtension : Attribute
  {
    public readonly char TypeCode;
    public readonly string StartToken, EndToken;
    public RCExtension (char typeCode, string startToken, string endToken)
    {
      TypeCode = typeCode;
      StartToken = startToken;
      EndToken = endToken;
    }
  }
}
