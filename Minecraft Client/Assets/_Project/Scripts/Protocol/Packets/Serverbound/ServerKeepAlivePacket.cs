using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerKeepAlivePacket : Packet
{
	public long KeepAliveID { get; set; }

	public override byte[] Payload
	{
		set => throw new NotImplementedException();
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteInt64(writer, KeepAliveID);

					return stream.ToArray();
				}
			}
		}
	}

	public ServerKeepAlivePacket()
	{
		PacketID = (int)ServerboundIDs.KeepAlive;
	}

	public ServerKeepAlivePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
