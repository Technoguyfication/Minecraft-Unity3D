using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DisconnectPacket : Packet
{
	public string JSONResponse { get; set; }

	public DisconnectPacket()
	{
		PacketID = (int)ClientboundIDs.Disconnect;
	}

	public DisconnectPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					JSONResponse = PacketReader.ReadString(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
