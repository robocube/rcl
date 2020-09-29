
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Mono.Terminal;
#if __MonoCS__
using Mono.Unix;
#endif
using RCL.Kernel;

namespace RCL.Exe
{
  public class Isolate
  {
    [RCVerb ("isolate")]
    public void EvalIsolate (RCRunner runner, RCClosure closure, RCBlock right)
    {
      DoIsolate (runner, closure, new string[] { }, right);
    }

    [RCVerb ("isolate")]
    public void EvalIsolate (RCRunner runner, RCClosure closure, RCString left, RCBlock right)
    {
      DoIsolate (runner, closure, left.ToArray (), right);
    }

    protected void DoIsolate (RCRunner runner, RCClosure closure, string[] argv, RCValue code)
    {
      Thread thread = new Thread (delegate ()
      {
        Queue<string> argQueue = new Queue<string> (argv);
        string result = null;
        // AppDomain appDomain = null;
        AssemblyLoadContext context = null;
        Program program;
        Exception isolateEx = null;
        try
        {
          // AppDomainSetup setupInfo = new AppDomainSetup ();
          string build = "dev";
          if (argQueue.Count > 0) {
            string first = argv[0];
            string rclHome = Environment.GetEnvironmentVariable ("RCL_HOME");
            if (rclHome == null) {
              throw new Exception (
                "RCL_HOME was not set. It is needed in order to locate the specified binary: " +
                first);
            }
            if (first == "dev") {
              build = first;
              // It should use the dev build in this case. The current build is not the
              // dev build.
              // setupInfo = AppDomain.CurrentDomain.SetupInformation;
              // setupInfo.ApplicationBase = rclHome + "/dev/rcl/dbg";
              argQueue.Dequeue ();
            }
            else if (first == "last") {
              argQueue.Dequeue ();
              throw new NotImplementedException (
                "last option is not yet implemented. Please specify a build number");
            }
            else {
              int number;
              if (int.TryParse (first, out number)) {
                build = number.ToString ();
                // setupInfo.ApplicationBase = rclHome + "/bin/rcl_bin/" + number + "/lib";
                argQueue.Dequeue ();
              }
              else {
                // setupInfo = AppDomain.CurrentDomain.SetupInformation;
              }
            }
          }
          string appDomainName = "Isolated build:" + build + " id:" + Guid.NewGuid ();
          // appDomain = AppDomain.CreateDomain (appDomainName, null, setupInfo);
          context = new AssemblyLoadContext (appDomainName, isCollectible:false);
          Type type = typeof (Program);
          AssemblyName assemblyName = AssemblyName.GetAssemblyName (type.Assembly.FullName);
          Assembly assembly = context.LoadFromAssemblyName (assemblyName);
          program = (Program) assembly.CreateInstance ("Program");
          // program = (Program) appDomain.CreateInstanceAndUnwrap (type.Assembly.FullName,
          //                                                        type.FullName);
          // string appDomainVersionString = setupInfo.ApplicationBase;
          string appDomainVersionString = "testing dotnet 5!";
          // RCSystem.CrossDomainTraceHelper.StartListening (appDomain);
          program.IsolateCode = code.ToString ();
          program.InstanceMain (argQueue.ToArray (), appDomainVersionString);
          //result = (string) appDomain.GetData ("IsolateResult");
          //isolateEx = (Exception) appDomain.GetData ("IsolateException");
        }
        catch (Exception ex)
        {
          runner.Finish (closure, ex, 1);
        }
        finally
        {
          // if (appDomain != null) {
          //   AppDomain.Unload (appDomain);
          //   appDomain = null;
          // }
        }
        if (result == null) {
          if (isolateEx == null) {
            isolateEx = new Exception ("Missing IsolateResult for program");
          }
          runner.Finish (closure, isolateEx, 1);
        }
        else {
          runner.Yield (closure, RCSystem.Parse (result));
        }
      });
      thread.IsBackground = true;
      thread.Start ();
    }
  }
}
