
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using RCL.Kernel;

namespace RCL.Core
{
  public class TcpSendState
  {
    public TcpSendState (long handle, long id, RCBlock message)
    {
      Handle = handle;
      Id = id;
      Message = message;
    }

    public RCBlock Message;
    public long Handle;
    public long Id;
  }
}