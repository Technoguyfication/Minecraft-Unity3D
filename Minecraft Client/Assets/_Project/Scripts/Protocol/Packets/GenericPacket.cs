using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GenericPacket : Packet
{
	public override byte[] Payload { get; set; }

	public GenericPacket()
	{
		PacketID = default;
	}

	public GenericPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
