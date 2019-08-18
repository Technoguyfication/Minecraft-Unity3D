using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientKeepAlivePacket : Packet
{
	public long KeepAliveID { get; set; }

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					KeepAliveID = PacketReader.ReadInt64(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}

	public ClientKeepAlivePacket()
	{
		PacketID = (int)ClientboundIDs.KeepAlive;
	}

	public ClientKeepAlivePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
