using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityHeadLookPacket : Packet
{
	public int EntityID { get; set; }
	public sbyte HeadYaw { get; set; }

	public EntityHeadLookPacket()
	{
		PacketID = (int)ClientboundIDs.EntityHeadLook;
	}

	public EntityHeadLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityID = PacketReader.ReadVarInt(reader);
					HeadYaw = (sbyte)PacketReader.ReadByte(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
