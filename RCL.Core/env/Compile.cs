using System;
using System.Reflection;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RCL.Kernel;

namespace RCL.Core
{
  public class Compile
  {
    [RCVerb ("compile")]
    public void EvalCompile (RCRunner runner, RCClosure closure, RCString right)
    {
      string code = right[0];
      CSharpCodeProvider provider = new CSharpCodeProvider ();
      CompilerParameters parameters = new CompilerParameters ();
      Uri codebase = new Uri (Assembly.GetExecutingAssembly ().CodeBase);
      DirectoryInfo dir = new FileInfo (codebase.LocalPath).Directory;
      parameters.ReferencedAssemblies.Add (dir.FullName + "/RCL.Kernel.dll");
      parameters.GenerateInMemory = true;
      parameters.GenerateExecutable = false;
      CompilerResults results = null;
      try
      {
        RCSystem.Log.Record (closure, "compile", 0, "code", code);
        results = provider.CompileAssemblyFromSource (parameters, code);
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        if (results != null)
        {
          for (int i = 0; i < results.Errors.Count; ++i)
          {
            CompilerError error = results.Errors[i];
            Console.Out.WriteLine (error.ToString ());
            RCSystem.Log.Record (closure, "compile", 0, "error", error.ToString ());
            /*
            error.Column;
            error.ErrorNumber;
            error.ErrorText;
            error.FileName;
            error.IsWarning;
            error.Line;
            */
          }
        }
      }
      if (results.Errors.Count > 0)
      {
        throw new Exception ("compilation failed, show compile:error for details");
      }
      Type[] types = results.CompiledAssembly.GetTypes ();
      RCArray<string> modules = new RCArray<string> ();
      RCBlock result = RCBlock.Empty;
      for (int i = 0; i < types.Length; ++i)
      {
        bool isModule;
        RCBlock typeVerbs = RCSystem.Activator.CreateVerbTable (types[i], out isModule);
        result = new RCBlock (result, types[i].Name, ":", typeVerbs);
        if (isModule)
        {
          modules.Write (types[i].Name);
          RCBot bot = runner.GetBot (closure.Bot);
          bot.PutModule (types[i]);
        }
      }
      runner.Yield (closure, result);
    }
  }
}
