
using System.Text;
using RCL.Kernel;

namespace RCL.Core
{
  public class TcpProtocol : Tcp.Protocol
  {
    public override byte[] Serialize (Tcp.Client client, RCValue message)
    {
      // Hey we should try using ToByte in here.
      return Encoding.ASCII.GetBytes (message.ToString ());
    }

    public override byte[] Serialize (Tcp.Server client, RCValue message)
    {
      return Encoding.ASCII.GetBytes (message.ToString ());
    }
  }
}