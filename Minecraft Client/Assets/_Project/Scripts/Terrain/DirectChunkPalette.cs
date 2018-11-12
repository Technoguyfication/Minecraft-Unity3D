using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DirectChunkPalette : IChunkPalette
{
	public uint GetBlockState(uint id)
	{
		return id;
	}

	public void Read(List<byte> buffer)
	{
		// read next varint because there isn't really a palette
		VarInt.ReadNext(buffer);
	}
}
