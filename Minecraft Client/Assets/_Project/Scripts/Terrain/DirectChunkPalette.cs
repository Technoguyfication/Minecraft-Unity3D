using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DirectChunkPalette : IChunkPalette
{
	public int Length { get; } = 0;

	public uint GetBlockState(uint id)
	{
		return id;
	}

	public void Read(List<byte> buffer)
	{

	}
}
