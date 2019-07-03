using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Server status response packet https://wiki.vg/Protocol#Response
/// </summary>
public class UnloadChunkPacket : Packet
{
	public UnloadChunkPacket()
	{
		PacketID = (int)ClientboundIDs.UNLOAD_CHUNK;
	}

	public UnloadChunkPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			List<byte> buffer = new List<byte>(value);
			X = PacketHelper.GetInt32(buffer);
			Z = PacketHelper.GetInt32(buffer);
		}
	}

	public int X { get; set; }
	public int Z { get; set; }

	public ChunkPos Position => new ChunkPos()
	{
		X = X,
		Z = Z
	};
}
