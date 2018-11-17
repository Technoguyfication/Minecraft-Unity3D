using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerKeepAlivePacket : Packet
{
	public override byte[] Payload { get; set; }

	public ServerKeepAlivePacket()
	{
		PacketID = (int)ServerboundIDs.KEEP_ALIVE;
	}

	public ServerKeepAlivePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
