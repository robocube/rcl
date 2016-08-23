
namespace RCL.Kernel
{
  public enum RCErrors
  {
    Timeout = 0,  //The operation timed out.
    Count   = 1,  //The argument had the wrong count.
    Type    = 2,  //The argument was the wrong type.
    Name    = 3,  //The name could not be resolved.
    Varname = 4,  //The variable name could not be resolved.
    Lock    = 4,  //Tried to mutate a locked value.
    Assert  = 5,  //Explicit assertion failed.
    Native  = 6,  //Unintentional operator exception.
    Custom  = 7,  //Custom status created with fail.
    Exec    = 8   //Non-zero exit status on external process.
  }
}
