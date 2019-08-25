using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Spawns a player on the client
/// </summary>
public class SpawnPlayerPacket : Packet
{
	public int EntityID { get; set; }
	public Guid PlayerUUID { get; set; }
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
	public byte Yaw { get; set; }
	public byte Pitch { get; set; }

	// TODO: add entity metadata

	public SpawnPlayerPacket()
	{
		PacketID = (int)ClientboundIDs.SpawnPlayer;
	}

	public SpawnPlayerPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityID = PacketReader.ReadVarInt(reader);
					PlayerUUID = PacketReader.ReadGuid(reader);

					X = PacketReader.ReadDouble(reader);
					Y = PacketReader.ReadDouble(reader);
					Z = PacketReader.ReadDouble(reader);

					Yaw = PacketReader.ReadByte(reader);
					Pitch = PacketReader.ReadByte(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
