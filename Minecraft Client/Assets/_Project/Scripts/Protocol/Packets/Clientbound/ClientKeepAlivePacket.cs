using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientKeepAlivePacket : Packet
{
	public long KeepAliveID { get; set; }

	public override byte[] Payload
	{
		set => KeepAliveID = PacketHelper.GetInt64(new List<byte>(value));
		get => throw new NotImplementedException();
	}

	public ClientKeepAlivePacket()
	{
		PacketID = (int)ClientboundIDs.KEEP_ALIVE;
	}

	public ClientKeepAlivePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
