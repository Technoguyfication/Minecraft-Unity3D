using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DisconnectPacket : Packet
{
	public string JSONResponse { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			JSONResponse = PacketHelper.GetString(buffer);
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public DisconnectPacket()
	{
		PacketID = (int)ClientboundIDs.DISCONNECT;
	}

	public DisconnectPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
