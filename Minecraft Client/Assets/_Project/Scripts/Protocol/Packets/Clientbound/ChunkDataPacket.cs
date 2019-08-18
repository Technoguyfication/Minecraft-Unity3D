using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ChunkDataPacket : Packet
{
	public int ChunkX { get; set; }
	public int ChunkZ { get; set; }
	public ChunkColumnPos Position => new ChunkColumnPos(ChunkX, ChunkZ);
	public bool GroundUpContinuous { get; set; }
	public int PrimaryBitmask { get; set; }
	public byte[] Data { get; set; }

	public ChunkDataPacket()
	{
		PacketID = (int)ClientboundIDs.ChunkData;
	}

	public ChunkDataPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					ChunkX = PacketReader.ReadInt32(reader);
					ChunkZ = PacketReader.ReadInt32(reader);

					GroundUpContinuous = PacketReader.ReadBoolean(reader);
					PrimaryBitmask = PacketReader.ReadVarInt(reader);

					int byteCount = PacketReader.ReadVarInt(reader);
					Data = PacketReader.ReadBytes(reader, byteCount);
				}
			}

			// todo support block entities
		}
	}

	public override string ToString()
	{
		return $"Chunk at {ChunkX}, {ChunkZ}: Ground Up Continuous: {GroundUpContinuous}, Primary Bitmask: {BitConverter.ToString(BitConverter.GetBytes(PrimaryBitmask))}, Data Length: {Data.Length}";
	}
}
