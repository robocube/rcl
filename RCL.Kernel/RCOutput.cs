
namespace RCL.Kernel
{
  public enum RCOutput
  {
    Quiet   = 0,  // No console output.
    Single  = 1,  // Single-line of output per console event.
    Systemd = 2,  // Single-line output for systemd 'new style' daemons.
    Multi   = 3,  // Multiple lines of output per console event.
    Full    = 4,  // Output all available info from console events.
    Clean   = 5,  // Output exactly what is written with print and nothing else.
    Trace   = 6,  // Output a full trace of the evaluation process. Not implemented.
    Test    = 7,  // Output suitable for test purposes. No timestamps, long messages.
  }
}
