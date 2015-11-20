using System;
using System.Reflection;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
  public class Primitive : Attribute
  {
    public readonly string Name;
    public readonly Profile Profile;

    public Primitive (string name)
    {
      Name = name;
      Profile = Profile.Dyadic;
    }

    public Primitive (string name, Profile profile)
    {
      Name = name;
      Profile = profile;
    }
  }
}
