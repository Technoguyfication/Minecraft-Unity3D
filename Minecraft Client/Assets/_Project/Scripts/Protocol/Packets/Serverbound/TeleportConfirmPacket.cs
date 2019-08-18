using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TeleportConfirmPacket : Packet
{
	public int TeleportID { get; set; }

	public TeleportConfirmPacket()
	{
		PacketID = (int)ServerboundIDs.TeleportConfirm;
	}

	public TeleportConfirmPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteVarInt(writer, TeleportID);

					return stream.ToArray();
				}
			}
		}
		set => throw new NotImplementedException();
	}
}
