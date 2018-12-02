
using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace RCL.Kernel
{ 
  public class RCActivator
  {
    public static RCActivator CreateDefault ()
    {
      //Not sure how I feel about this setup.
      //It could throw an exception and that would be bad.
      //On the other hand if it throws an exception you are screwed anyhow.
      Uri codebase = new Uri (Assembly.GetExecutingAssembly ().CodeBase);
      DirectoryInfo dir = new FileInfo (codebase.LocalPath).Directory;
      FileInfo[] files = dir.GetFiles ("*.dll");
      FileInfo[] exeFiles = dir.GetFiles ("*.exe");
      List<string> paths = new List<string> ();
      for (int i = 0; i < files.Length; ++i)
      {
        paths.Add (new Uri (files[i].FullName).LocalPath);
      }
      for (int i = 0; i < exeFiles.Length; ++i)
      {
        paths.Add (new Uri (exeFiles[i].FullName).LocalPath);
      }
      return new RCActivator (paths.ToArray ());
    }

    //The assemblies which have been loaded
    protected Dictionary<string, Assembly> m_libs = new Dictionary<string, Assembly> ();

    //All of the available operators are available regardless of how they are implemented.
    protected Dictionary<string, Type> m_operator = new Dictionary<string, Type> ();

    //Lookup for final dispatching of the correct operator overload.
    protected internal Dictionary<OverloadKey, OverloadValue> m_dispatch;

    //List of types to be treated as modules with state maintained by the bot.
    protected HashSet<Type> m_modules;

    public RCActivator (params string[] libs)
    {
      //You can always override the built in macros with extension libraries
      foreach (string lib in libs)
      {
        try 
        {
          m_libs.Add (lib, Assembly.LoadFile (lib));
        }
        catch (ArgumentException)
        {
          throw new Exception (string.Format ("Duplicate lib '{0}'", lib));
        }
        catch (BadImageFormatException)
        {
          //Skip any unmanaged dlls in the dir    
        }
      }
      m_dispatch = new Dictionary<OverloadKey, OverloadValue> ();
      m_modules = new HashSet<Type> ();
      foreach (Assembly lib in m_libs.Values)
      {
        Type[] types = null;
        try
        {
          types = lib.GetTypes ();
        }
        catch (Exception ex)
        {
          RCSystem.Log.Record (0, 0, "runner", 0, "load", ex);
          continue;
        }
        foreach (Type type in types)
        {
          bool module;
          CreateVerbTable (type, out module);
        }
      }
    }

    protected Dictionary<string, ParserExtension> m_extByToken = new Dictionary<string, ParserExtension> ();
    protected Dictionary<char, ParserExtension> m_extByCode = new Dictionary<char, ParserExtension> ();

    public RCBlock CreateVerbTable (Type type, out bool module)
    {
      RCBlock result = RCBlock.Empty;
      //Is it an extension?
      object[] attributes = type.GetCustomAttributes (typeof (RCExtension), false);
      if (attributes.Length > 0)
      {
        RCExtension ext = (RCExtension) attributes[0];
        ParserExtension extension = (ParserExtension) type.GetConstructor (m_ctor).Invoke (m_ctor);
        m_extByToken[ext.StartToken] = extension;
        m_extByCode[ext.TypeCode] = extension;
      }
      //Is it a standalone module (with no operators)
      module = false;
      attributes = type.GetCustomAttributes (typeof (RCModule), false);
      if (attributes.Length > 0)
      {
        module = true;
      }
      foreach (MethodInfo method in type.GetMethods ())
      {
        attributes = method.GetCustomAttributes (typeof (RCVerb), false);
        for (int i = 0; i < attributes.Length; ++i)
        {
          RCVerb verb = (RCVerb) attributes[i];
          //Check to see if this verb has a custom implementation.
          //This is for operators that need to override methods on RCOperator
          //like Next for example. switch and each are good examples.
          //Having an entry in m_operator is what distinguishes a built-in
          //operator from a UserOperator implemented in RCL.
          //note: a user operator may obscure a built-in operator if it has the same name
          Type optype;
          m_operator.TryGetValue (verb.Name, out optype);
          if (optype == null || optype == typeof (RCOperator))
          {
            if (type.IsSubclassOf (typeof (RCOperator)))
            {
              m_operator[verb.Name] = type;
            }
            else
            {
              m_operator[verb.Name] = typeof (RCOperator);
            }
          }
          ParameterInfo[] parameters = method.GetParameters ();
          if (parameters.Length == 3)
          {
            OverloadKey key = new OverloadKey (verb.Name, typeof (object), null, parameters[2].ParameterType);
            if (m_dispatch.ContainsKey (key))
            {
              throw new Exception ("dispatch table already contains the key:" + key);
            }
            OverloadValue value = new OverloadValue (type, method);
            m_dispatch.Add (key, value);
            result = new RCBlock (result, verb.Name, ":", verb.Name);
            module = true;
          }
          else if (parameters.Length == 4)
          {
            OverloadKey key = new OverloadKey (verb.Name, typeof (object), parameters[2].ParameterType, parameters[3].ParameterType);
            if (m_dispatch.ContainsKey (key))
            {
              throw new Exception ("dispatch table already contains the key:" + key);
            }
            OverloadValue value = new OverloadValue (type, method);
            m_dispatch.Add (key, value);
            result = new RCBlock (result, verb.Name, ":", verb.Name);
            module = true;
          }
        }
      }
      if (module)
      {
        m_modules.Add (type);
      }
      return result;
    }

    protected static readonly Type[] m_ctor = new Type[] { };
    public RCOperator New (string op, RCValue left, RCValue right)
    {
      string key = op;
      RCOperator result = null;
      if (!m_operator.ContainsKey (key))
      {
        result = new UserOperator (new RCReference (op));
        result.Init (op, left, right);
      }
      else
      {
        Type type = m_operator[op];
        //This is a tiny optimization, avoids using reflection
        //if we are only going to call the base class RCOperator ctor.
        //But I would like to note that I am ok with New on operators being
        //slightly expensive because it will generally only happen during
        //parsing or macro expansion which are both expensive anyhow.
        if (type == typeof (RCOperator))
        {
          result = new RCOperator ();
          result.Init (op, left, right);
        }
        else if (type.IsSubclassOf (typeof (RCOperator)))
        {
          ConstructorInfo ctor = type.GetConstructor (m_ctor);
          if (ctor == null)
          {
            throw new Exception ("No zero argument constructor found on type " + type);
          }
          result = (RCOperator) ctor.Invoke (null);
          result.Init (op, left, right);
        }
      }
      return result;
    }

    public RCOperator New (string op, RCValue right)
    {
      return New (op, null, right);
    }

    public struct OverloadKey : IEquatable<OverloadKey>
    {
      public readonly string Name;
      public readonly Type Left;
      public readonly Type Right;
      public readonly Type Collection;
      public readonly int Hash;

      /// <summary>
      /// The collection type should be either RCBlock or RCCube.
      /// </summary>
      public OverloadKey (string name, Type collection, Type left, Type right)
      {
        if (collection == null)
        {
          throw new ArgumentNullException ("collection");
        }
        Name = name;
        Left = left;
        Right = right.IsSubclassOf (typeof (RCOperator)) ? typeof (RCOperator) : right;
        Collection = collection;

        //xor the hashes together for name, left and right type.
        //No idea whether this technique is sufficient for good performance.
        //Eternally Confuzzled says it is not.
        //However a lot of the allegedly better algorithms produce uint hashes instead
        //of int hashes.  And I don't really know how to modify them appropriately.
        //http://eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx
        //I kind of wish MSFT gave me a HashCombiner somewhere in the framework.
        //At any rate we will revisit this kind of thing when I start optimizing RCL.
        //int h = 0;
        //h ^= name.GetHashCode ();
        //h ^= left == null ? 0 : left.GetHashCode ();
        //h ^= right.GetHashCode ();
        //Hash = h;

        //Ok the xor hash turned out to be a really bad idea since it produced the
        //same number for every combination of + (long, double) and + (double, long).
        //Here is some more bullshit I got off the internet.  It seems to work
        //a little bit.  Still need to revisit this in seriousness with graphs, charts,
        //labcoats, beakers and such.
        //http://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations

        Hash = 17;
        Hash = Hash * 31 + name.GetHashCode ();
        Hash = Hash * 31 + (left == null ? 0 : left.GetHashCode ());
        Hash = Hash * 31 + (Collection == null ? 0 : Collection.GetHashCode ());
        Hash = Hash * 31 + right.GetHashCode ();
      }

      public bool Equals (OverloadKey other)
      {
        if (Left == null && other.Left != null)
        {
          return false;
        }
        if (other.Left == null && Left != null)
        {
          return false;
        }
        if (!Right.Equals (other.Right))
        {
          return false;
        }
        if (!Name.Equals (other.Name))
        {
          return false;
        }
        if (Collection == null && other.Collection != null)
        {
          return false;
        }
        if (other.Collection == null && Collection != null)
        {
          return false;
        }
        if (Collection != null && !Collection.Equals (other.Collection))
        {
          return false;
        }
        return true;
      }

      public override int GetHashCode ()
      {
        return Hash;
      }

      public override string ToString ()
      {
        if (Left == null)
          return Name + "(" + Right + ")";
        else return Name + "(" + Left + ", " + Right + ")";
      }
    }

    public class OverloadValue
    {
      public readonly Type Module;
      public readonly MethodInfo Implementation;

      public OverloadValue (Type module, MethodInfo implementation)
      {
        Module = module;
        Implementation = implementation;
      }

      public override string ToString ()
      {
        return Module + ":" + Implementation;
      }
    }

    //Shouldn't these move into eval?
    public void Invoke (RCRunner runner, RCClosure closure, string name, object right)
    {
      OverloadValue overload;
      Type rtype = right.GetType ();
      Type ctype = right.GetType ();
      try
      {
        if (m_dispatch.TryGetValue (new OverloadKey (name, ctype, null, rtype), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), null, rtype), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), null, typeof (object)), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), null, typeof (RCOperator)), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, right});
        }
        else throw RCException.Overload (closure, name, right);
      }
      catch (TargetInvocationException tiex)
      {
        Exception ex = tiex.GetBaseException ();
        RCException rcex = ex as RCException;
        if (rcex != null)
        {
          throw rcex;
        }
        else
        {
          //You have to pass the tiex, not ex here so that the interior stack trace will be preserved when/if it is rethrown.
          throw new RCException (closure, tiex, RCErrors.Native, ThrowMessage (name, ex));
        }
      }
    }

    public void Invoke (RCRunner runner, RCClosure closure, string name, object left, object right)
    {
      OverloadValue overload;
      Type ltype = left.GetType ();
      Type rtype = right.GetType ();
      Type ctype = right.GetType ();
      try
      {
        if (m_dispatch.TryGetValue (new OverloadKey (name, ctype, ltype, rtype), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, left, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), ltype, rtype), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, left, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), typeof (object), rtype), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, left, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), ltype, typeof (object)), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, left, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), typeof (object), typeof (object)), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, left, right});
        }
        else if (m_dispatch.TryGetValue (new OverloadKey (name, typeof (object), ltype, typeof (RCOperator)), out overload))
        {
          RCBot bot = runner.GetBot (closure.Bot);
          object state = bot.GetModule (overload.Module);
          overload.Implementation.Invoke (state, new object[] {runner, closure, left, right});
        }
        else throw RCException.Overload (closure, name, left, right);
      }
      catch (TargetInvocationException tiex)
      {
        Exception ex = tiex.GetBaseException ();
        RCException rcex = ex as RCException;
        if (rcex != null)
        {
          throw rcex;
        }
        else
        {
          //You have to pass the tiex, not ex here so that the interior stack trace will be preserved when/if it is rethrown.
          throw new RCException (closure, tiex, RCErrors.Native, ThrowMessage (name, ex));
        }
      }
    }

    protected string ThrowMessage (string name, Exception ex)
    {
      if (RCSystem.Args.OutputEnum == RCOutput.Test)
      {
        return string.Format ("An exception was thrown by the operator {0}.", name);
      }
      else
      {
        return string.Format ("An exception was thrown by the operator {0}:\n-- {1}: {2}", name, ex.GetType ().Name, ex.Message);
      }
    }

    //Seems like a kind of weird way to do this piece.
    public void InjectState (RCBot bot)
    {
      foreach (Type type in m_modules)
      {
        bot.PutModule (type);
      }
    }
      
    //Maybe this should implement the full interface for parser,
    //Or maybe the lexer should not do anything to the tokens between [ and ]
    //Or maybe we need a lexer extension abstraction as well.
    public abstract class ParserExtension
    {
      public abstract RCValue BinaryParse (RCActivator activator, RCArray<byte> data, ref int start);
      public abstract ParserState StartParsing (bool canonical);
      public abstract void AcceptName (object state, RCToken token);
      public abstract void AcceptScalar (object state, RCToken token, RCLexer lexer);
      public abstract void AcceptSpacer (object state, RCToken token);
      public abstract RCValue EndParsing (object state);
    }

    public class ParserState
    {

    }

    public ParserExtension ExtensionFor (string token)
    {
      return m_extByToken[token];
    }

    public ParserExtension ExtensionFor (char code)
    {
      return m_extByCode[code];
    }
  }

}
