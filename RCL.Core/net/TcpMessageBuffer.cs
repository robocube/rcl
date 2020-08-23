
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
  public class TcpMessageBuffer
  {
    protected static readonly int BUFFER_SIZE = 8192;
    protected static readonly int HEADER_SIZE = 12;
    protected int _reading, _read;
    protected long _cid = -1;
    protected byte[] _recvBuffer, _sendBuffer;

    public TcpMessageBuffer ()
    {
      _recvBuffer = new byte[BUFFER_SIZE];
      _sendBuffer = new byte[BUFFER_SIZE];
    }

    public byte[] RecvBuffer {
      get { return _recvBuffer; }
    }
    public int Read {
      get { return _read; }
    }
    public byte[] SendBuffer {
      get { return _sendBuffer; }
    }

    public virtual RCBlock CompleteReceive (RCRunner runner,
                                            int count,
                                            long handle,
                                            long sid,
                                            out
                                            long cid)
    {
      RCBlock message = null;
      if (_read == 0) {
        // Inspect the header to see if we have enough buffer for the message
        // that is going to arrive.  If not create a bigger buffer.
        _reading = IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (_recvBuffer, 0));
        _cid = IPAddress.NetworkToHostOrder (BitConverter.ToInt64 (_recvBuffer, 4));
        if (_reading > _recvBuffer.Length - HEADER_SIZE) {
          Console.Out.WriteLine ("  replacing buffer...", _read, _reading);
          byte[] replacement = new byte[_reading + HEADER_SIZE];
          _recvBuffer.CopyTo (replacement, count);
          _recvBuffer = replacement;
        }
      }
      _read += count;
      cid = _cid;
      if (_read >= _reading + HEADER_SIZE) {
        string text = Encoding.ASCII.GetString (_recvBuffer, HEADER_SIZE, _read - HEADER_SIZE);
        bool fragment = false;
        // This one creates a new parser an so it is threadsafe for multiple calls.
        RCValue body = RCSystem.Parse (text, out fragment);
        if (body != null) {
          // throw new Exception("failed to parse:" + text);
          RCSymbolScalar correlation = new RCSymbolScalar (null, handle);
          if (sid > -1) {
            correlation = new RCSymbolScalar (correlation, sid);
          }
          correlation = new RCSymbolScalar (correlation, _cid);
          message = new RCBlock (null, "id", ":", new RCSymbol (correlation));
          message = new RCBlock (message, "body", ":", body);
        }
        _read = 0;
        _reading = 0;
        _cid = -1;
      }
      return message;
    }

    public virtual int PrepareSend (long cid, byte[] payload)
    {
      // Clearly this strategy will not do when it comes to serializing the whole
      // object, but for this purpose it should be ok.
      byte[] sizeBytes = BitConverter.GetBytes (IPAddress.HostToNetworkOrder (payload.Length));
      byte[] cidBytes = BitConverter.GetBytes (IPAddress.HostToNetworkOrder (cid));

      int start = 0;
      Array.Copy (sizeBytes, 0, _sendBuffer, start, sizeBytes.Length);
      start += sizeBytes.Length;
      Array.Copy (cidBytes, 0, _sendBuffer, start, cidBytes.Length);
      start += cidBytes.Length;
      Array.Copy (payload, 0, _sendBuffer, start, payload.Length);
      return payload.Length + HEADER_SIZE;
    }
  }
}
