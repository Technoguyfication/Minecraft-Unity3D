using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityTeleportPacket : Packet
{
	public int EntityID { get; set; }
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
	public byte Yaw { get; set; }
	public byte Pitch { get; set; }
	public bool OnGround { get; set; }

	public EntityTeleportPacket()
	{
		PacketID = (int)ClientboundIDs.EntityTeleport;
	}

	public EntityTeleportPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityID = PacketReader.ReadVarInt(reader);

					X = PacketReader.ReadDouble(reader);
					Y = PacketReader.ReadDouble(reader);
					Z = PacketReader.ReadDouble(reader);

					Yaw = PacketReader.ReadByte(reader);
					Pitch = PacketReader.ReadByte(reader);

					OnGround = PacketReader.ReadBoolean(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
