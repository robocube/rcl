
namespace RCL.Kernel
{
  public enum RCOutput
  {
    Quiet  = 0,  //No console output.
    Single = 1,  //Single-line of output per console event.
    Multi  = 2,  //Multiple lines of output per console event.
    Full   = 3,  //Output all available info from console events.
    Clean  = 4,  //Output exactly what is written with print and nothing else.
    Trace  = 5,  //Output a full trace of the evaluation process. Not implemented.
  }
}
