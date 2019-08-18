using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLookAndRelativeMovePacket : Packet
{
	public int EntityID { get; set; }
	public short DeltaX { get; set; }
	public short DeltaY { get; set; }
	public short DeltaZ { get; set; }
	public sbyte Yaw { get; set; }
	public sbyte Pitch { get; set; }
	public bool OnGround { get; set; }

	public EntityLookAndRelativeMovePacket()
	{
		PacketID = (int)ClientboundIDs.EntityLookAndRelativeMove;
	}

	public EntityLookAndRelativeMovePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityID = PacketReader.ReadVarInt(reader);

					DeltaX = PacketReader.ReadInt16(reader);
					DeltaY = PacketReader.ReadInt16(reader);
					DeltaZ = PacketReader.ReadInt16(reader);

					Yaw = (sbyte)PacketReader.ReadByte(reader);
					Pitch = (sbyte)PacketReader.ReadByte(reader);

					OnGround = PacketReader.ReadBoolean(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
