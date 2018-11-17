using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TeleportConfirmPacket : Packet
{
	public int  TeleportID { get; set; }

	public override byte[] Payload {
		get
		{
			return VarInt.GetBytes(TeleportID);
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public TeleportConfirmPacket()
	{
		PacketID = default(int);
	}

	public TeleportConfirmPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
