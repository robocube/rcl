
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace RCL.Kernel
{
  [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
  public class RCVerb : Attribute
  {
    public readonly string Name;
    public RCVerb (string name)
    {
      Name = name;
    }
  }
}
