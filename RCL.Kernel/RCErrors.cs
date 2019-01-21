
using System;

namespace RCL.Kernel
{
  [Serializable]
  public enum RCErrors
  {
    Timeout = 0,  // The operation timed out
    Count   = 1,  // The argument had the wrong count
    Type    = 2,  // The argument was the wrong type
    Name    = 3,  // The name could not be resolved
    Varname = 4,  // The variable name could not be resolved
    Lock    = 5,  // Tried to mutate a locked value
    Assert  = 6,  // Explicit assertion failed
    Native  = 7,  // Unintentional operator exception
    Custom  = 8,  // Custom status created with fail
    Exec    = 9,  // Non-zero exit status on external process
    Handle  = 10, // Bad handle number
    Session = 11, // Bad session cookie
    Range   = 12, // Bad array index
    File    = 13, // Bad file name
    Access  = 14, // Inadequete file access
    Debug   = 15, // An internal failed assertion
    Syntax  = 16  // Malformed RCL syntax
  }
}
