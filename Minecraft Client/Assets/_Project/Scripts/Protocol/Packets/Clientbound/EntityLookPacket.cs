using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLookPacket : Packet
{
	public int EntityID { get; set; }
	public sbyte Yaw { get; set; }
	public sbyte Pitch { get; set; }
	public bool OnGround { get; set; }

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityID = PacketReader.ReadVarInt(reader);

					Yaw = (sbyte)PacketReader.ReadByte(reader);
					Pitch = (sbyte)PacketReader.ReadByte(reader);

					OnGround = PacketReader.ReadBoolean(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}

	public EntityLookPacket()
	{
		PacketID = (int)ClientboundIDs.EntityMove;
	}

	public EntityLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
