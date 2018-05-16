
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
    protected int m_reading, m_read;
    protected long m_cid = -1;
    protected byte[] m_recvBuffer, m_sendBuffer;

    public TcpMessageBuffer ()
    {
      m_recvBuffer = new byte[BUFFER_SIZE];
      m_sendBuffer = new byte[BUFFER_SIZE];
    }

    public byte[] RecvBuffer { get { return m_recvBuffer; } }
    public int Read { get { return m_read; } }
    public byte[] SendBuffer { get { return m_sendBuffer; } }

    public virtual RCBlock CompleteReceive (RCRunner runner, int count, long handle, long sid, out long cid)
    {
      RCBlock message = null;
      if (m_read == 0)
      {
        //Inspect the header to see if we have enough buffer for the message
        //that is going to arrive.  If not create a bigger buffer.
        m_reading = IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (m_recvBuffer, 0));
        m_cid = IPAddress.NetworkToHostOrder (BitConverter.ToInt64 (m_recvBuffer, 4));
        if (m_reading > m_recvBuffer.Length - HEADER_SIZE)
        {
          Console.Out.WriteLine ("  replacing buffer...", m_read, m_reading);
          byte[] replacement = new byte[m_reading + HEADER_SIZE];
          m_recvBuffer.CopyTo (replacement, count);
          m_recvBuffer = replacement;
        }
      }
      m_read += count;
      cid = m_cid;
      if (m_read >= m_reading + HEADER_SIZE)
      {
        string text = Encoding.ASCII.GetString (m_recvBuffer, HEADER_SIZE, m_read - HEADER_SIZE);
        bool fragment = false;
        //This one creates a new parser an so it is threadsafe for multiple calls.
        RCValue body = RCSystem.Parse (text, out fragment);
        if (body != null)
        {
          //throw new Exception("failed to parse:" + text);
          RCSymbolScalar correlation = new RCSymbolScalar (null, handle);
          if (sid > -1)
          {
            correlation = new RCSymbolScalar (correlation, sid);
          }
          correlation = new RCSymbolScalar (correlation, m_cid);
          message = new RCBlock (null, "id", ":", new RCSymbol (correlation));
          message = new RCBlock (message, "body", ":", body);
        }
        m_read = 0;
        m_reading = 0;
        m_cid = -1;
      }
      return message;
    }

    public virtual int PrepareSend (long cid, byte[] payload)
    {
      //Clearly this strategy will not do when it comes to serializing the whole
      //object, but for this purpose it should be ok.
      byte[] sizeBytes = BitConverter.GetBytes (IPAddress.HostToNetworkOrder (payload.Length));
      byte[] cidBytes = BitConverter.GetBytes (IPAddress.HostToNetworkOrder (cid));

      int start = 0;
      Array.Copy (sizeBytes, 0, m_sendBuffer, start, sizeBytes.Length);
      start += sizeBytes.Length;
      Array.Copy (cidBytes, 0, m_sendBuffer, start, cidBytes.Length);
      start += cidBytes.Length;
      Array.Copy (payload, 0, m_sendBuffer, start, payload.Length);
      return payload.Length + HEADER_SIZE;
    }
  }
}
