using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Used for both ping and pong.
/// </summary>
public class SpawnMobPacket : Packet
{
	public int EntityID { get; set; }
	public Guid UUID { get; set; }
	public Entity.EntityType Type { get; set; }
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
	public byte Yaw { get; set; }
	public byte Pitch { get; set; }
	public byte HeadPitch { get; set; }
	public short VelocityX { get; set; }
	public short VelocityY { get; set; }
	public short VelocityZ { get; set; }

	// TODO: add entity metadata

	public SpawnMobPacket()
	{
		PacketID = (int)ClientboundIDs.SpawnMob;
	}

	public SpawnMobPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityID = PacketReader.ReadVarInt(reader);
					UUID = PacketReader.ReadGuid(reader);
					Type = (Entity.EntityType)PacketReader.ReadVarInt(reader);

					X = PacketReader.ReadDouble(reader);
					Y = PacketReader.ReadDouble(reader);
					Z = PacketReader.ReadDouble(reader);

					Yaw = PacketReader.ReadByte(reader);
					Pitch = PacketReader.ReadByte(reader);
					HeadPitch = PacketReader.ReadByte(reader);

					VelocityX = PacketReader.ReadInt16(reader);
					VelocityY = PacketReader.ReadInt16(reader);
					VelocityZ = PacketReader.ReadInt16(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
