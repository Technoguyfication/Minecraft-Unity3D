using System;
using System.Collections.Generic;
using System.IO;
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
		PacketID = (int)ClientboundIDs.UnloadChunk;
	}

	public UnloadChunkPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					X = PacketReader.ReadInt32(reader);
					Z = PacketReader.ReadInt32(reader);
				}
			}
		}
	}

	public int X { get; set; }
	public int Z { get; set; }

	public ChunkColumnPos Position => new ChunkColumnPos(X, Z);
}
