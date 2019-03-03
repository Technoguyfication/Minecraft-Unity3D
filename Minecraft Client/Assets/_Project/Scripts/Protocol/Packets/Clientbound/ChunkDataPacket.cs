using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ChunkDataPacket : Packet
{
	public int ChunkX { get; set; }
	public int ChunkZ { get; set; }
	public ChunkPos Position
	{
		get
		{
			return new ChunkPos()
			{
				X = ChunkX,
				Z = ChunkZ
			};
		}
	}
	public bool GroundUpContinuous { get; set; }
	public int PrimaryBitmask { get; set; }
	public byte[] Data { get; set; }

	public ChunkDataPacket()
	{
		PacketID = (int)ClientboundIDs.CHUNK_DATA;
	}

	public ChunkDataPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			List<byte> buffer = new List<byte>(value);
			ChunkX = PacketHelper.GetInt32(buffer);
			ChunkZ = PacketHelper.GetInt32(buffer);
			GroundUpContinuous = PacketHelper.GetBoolean(buffer);
			PrimaryBitmask = VarInt.ReadNext(buffer);
			int dataSize = VarInt.ReadNext(buffer);
			Data = buffer.Read(dataSize).ToArray();

			// todo support block entities
		}
	}

	public override string ToString()
	{
		return $"Chunk at {ChunkX}, {ChunkZ}: Ground Up Continuous: {GroundUpContinuous}, Primary Bitmask: {BitConverter.ToString(BitConverter.GetBytes(PrimaryBitmask))}, Data Length: {Data.Length}";
	}
}
