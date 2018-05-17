﻿using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace RCL.Kernel
{
  public class RCLArgv
  {
    public static RCLArgv m_instance = new RCLArgv ();
    public static volatile bool m_initInvoked = false;
    public static void Init (string[] argv)
    {
      if (m_initInvoked)
      {
        throw new InvalidOperationException ("RCLArgv.Init may only be invoked once per appDomain");
      }
      m_instance = new RCLArgv (argv);
      m_initInvoked = true;
    }

    public static RCLArgv Instance
    {
      get { return m_instance; }
    }

    public readonly RCBlock Options;
    public readonly RCString Arguments;
    public readonly RCOutput OutputEnum;

    //--exit -x
    //With --program, causes rcl to exit after --program is finished
    //With --batch, causes rcl to exit after input has been evaluated
    //Use --exit to avoid putting calls to :exit in your --program
    //This is to facilitate interactive debugging and development
    public readonly bool Exit;

    //--batch -b
    //Causes rcl to read all input from the console or file at once
    //After the end of the input, no more input will be accepted,
    //either with Console.ReadLine or with Getline.
    //If --exit is set, rcl will exit after processing all input
    //Otherwise the main thread will go to sleep until one of:
    // a unix signal requesting termination is received
    // :exit is invoked as a result of --program
    public readonly bool Batch;

    //--nokeys -k
    //Disables the Getline library which causes problems in non-interactive settings
    //Causes log messages to go to Console.Out rather than Console.Error
    //When --nokeys is set, any input is read with Console.ReadLine
    public readonly bool Nokeys;

    //--noread -r
    //With --nokeys, prevents all calls to Console.ReadLine and related methods
    //Use --noread when attempts to read from stdin would cause errors
    //(For example, under systemd)
    public readonly bool Noread;

    /// <summary>
    /// Display binary version number on the console no matter what.
    /// </summary>
    public readonly bool Version;

    /// <summary>
    /// File name of rcl program to execute on startup.
    /// </summary>
    public readonly string Program;

    /// <summary>
    /// With program, invoke a particular variable as an operator after loading program.
    /// </summary>
    public readonly string Action;

    /// <summary>
    /// The output mode of the rcl interpreter. See RCOutput for valid values.
    /// </summary>
    public readonly string Output;

    /// <summary>
    /// Whitelist of modules and events to display on the console.
    /// </summary>
    public readonly string[] Show;

    public RCLArgv (params string[] argv)
    {
      Exit = false;
      Batch = false;
      Nokeys = false;
      Version = false;
      Program = "";
      Action = "";
      Output = "full";
      Show = new string[] { "*" };
      RCBlock custom = RCBlock.Empty;
      List<string> arguments = new List<string> ();
      for (int i = 0; i < argv.Length; ++i)
      {
        string[] kv = argv[i].Split ('=');
        if (kv.Length == 1)
        {
          if (kv[0].StartsWith ("--"))
          {
            string option = kv[0].Substring (2);
            if (option.Equals ("exit"))
            {
              Exit = true;
            }
            else if (option.Equals ("batch"))
            {
              Batch = true;
            }
            else if (option.Equals ("nokeys"))
            {
              Nokeys = true;
            }
            else if (option.Equals ("noread"))
            {
              Noread = true;
            }
            else if (option.Equals ("version"))
            {
              //This option forces the version to display no matter what.
              //Useful when you need to check the version number from a test.
              Version = true;
            }
            else
            {
              custom = new RCBlock (custom, option, ":", RCBoolean.True);
            }
          }
          else if (kv[0].StartsWith ("-"))
          {
            for (int j = 1; j < kv[0].Length; ++j)
            {
              char c;
              switch (c = kv[0][j])
              {
                case 'x':
                  Exit = true;
                  break;
                case 'b':
                  Batch = true;
                  break;
                case 'k':
                  Nokeys = true;
                  break;
                case 'r':
                  Noread = true;
                  break;
                case 'v':
                  Version = true;
                  break;
                default:
                  Usage ("Unknown option '" + kv[0][j] + "'");
                  break;
              }
            }
          }
          else
          {
            //do position.
            arguments.Add (kv[0]);
          }
        }
        else
        {
          //do named arguments.
          if (kv[0].StartsWith ("--"))
          {
            string option = kv[0].Substring (2);
            if (option.Equals ("program"))
            {
              Program = kv[1];
            }
            else if (option.Equals ("action"))
            {
              Action = kv[1];
            }
            else if (option.Equals ("output"))
            {
              Output = kv[1];
            }
            else if (option.Equals ("show"))
            {
              Show = kv[1].Split (',');
            }
            else
            {
              custom = new RCBlock (custom, option, ":", new RCString (kv[1]));
            }
          }
          else Usage ("Named options start with --");
        }
      }
      Arguments = new RCString (arguments.ToArray ());
      Options = RCBlock.Empty;
      Options = new RCBlock (Options, "program", ":", new RCString (Program));
      Options = new RCBlock (Options, "action", ":", new RCString (Action));
      Options = new RCBlock (Options, "output", ":", new RCString (Output));
      Options = new RCBlock (Options, "show", ":", new RCString (Show));
      Options = new RCBlock (Options, "batch", ":", new RCBoolean (Batch));
      Options = new RCBlock (Options, "nokeys", ":", new RCBoolean (Nokeys));
      Options = new RCBlock (Options, "noread", ":", new RCBoolean (Noread));
      Options = new RCBlock (Options, "exit", ":", new RCBoolean (Exit));
      Options = new RCBlock (Options, "version", ":", new RCBoolean (Version));
      OutputEnum = (RCOutput) Enum.Parse (typeof (RCOutput), Output, true);
      for (int i = 0; i < custom.Count; ++i)
      {
        RCBlock name = custom.GetName (i);
        Options = new RCBlock (Options, name.Name, ":", name.Value);
      }
    }

    static void Usage (string message)
    {
      PrintCopyright ();
      Console.WriteLine ();
      Console.WriteLine ("Usage: rcl [OPTIONS] program action");
      Console.WriteLine ("  {0}", message);
      Console.WriteLine ();
      Console.WriteLine ("Options:");
      Console.WriteLine ("  --program         Program file to run on startup");
      Console.WriteLine ("  --action          Operator within program to eval on launch");
      Console.WriteLine ("  --output          Output format [full|multi|single|clean|none]");
      Console.WriteLine ("  --batch, -b       Non-interactive console (use when reading from standard input)");
      Console.WriteLine ("  --exit, -x        Exit after completion of action");
      Console.WriteLine ("  --version, -v     Force printing of version on start");
      Console.WriteLine ();
      Environment.Exit (1);
    }

    public void PrintStartup (string appDomainVersionString)
    {
      bool copyright = !Batch && OutputEnum != RCOutput.Clean && OutputEnum != RCOutput.Test;
      bool options = !Batch && OutputEnum != RCOutput.Clean && OutputEnum != RCOutput.Test;
      bool version = Version || (!Batch && OutputEnum != RCOutput.Clean && OutputEnum != RCOutput.Test);

      if (version)
      {
        PrintVersion (appDomainVersionString);
      }
      if (copyright)
      {
        PrintCopyright ();
      }
      if (options)
      {
        Console.WriteLine ();
        if (Arguments.Count > 0)
        {
          for (int i = 0; i < Arguments.Count; ++i)
          {
            Console.Write (Arguments[i]);
            if (i < Arguments.Count - 1)
            {
              Console.Write (" ");
            }
          }
          Console.WriteLine ();
        }
        Console.WriteLine (Options.Format (RCFormat.Pretty));
      }
      if (options || copyright)
      {
        Console.WriteLine ();
      }
    }

    protected static void PrintVersion (string appDomainVersionString)
    {
      Assembly assembly = Assembly.GetEntryAssembly ();
      if (assembly != null)
      {
        Version version = assembly.GetName ().Version;
        Console.Out.WriteLine ("Robocube Language {0}", version.ToString ());
      }
      else if (appDomainVersionString != null && appDomainVersionString != "")
      {
        Console.Out.WriteLine ("Robocube Language {0} (isolated)", appDomainVersionString);
      }
      else
      {
        Console.Out.WriteLine ("Robocube Language (isolated)");
      }
    }

    protected static void PrintCopyright ()
    {
      Console.Out.WriteLine ("Copyright (C) 2007-2015 Brian M. Andersen");
      Console.Out.WriteLine ("Copyright (C) 2015-2018 Robocube Corporation");
    }
  }
}