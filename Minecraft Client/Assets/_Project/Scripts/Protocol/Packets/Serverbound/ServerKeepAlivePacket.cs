using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerKeepAlivePacket : Packet
{
	public long KeepAliveID { get; set; }

	public override byte[] Payload
	{
		set => throw new NotImplementedException();
		get => BitConverter.GetBytes(KeepAliveID).ReverseIfLittleEndian();
	}

	public ServerKeepAlivePacket()
	{
		PacketID = (int)ServerboundIDs.KEEP_ALIVE;
	}

	public ServerKeepAlivePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
