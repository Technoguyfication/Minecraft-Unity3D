using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class IndirectChunkPalette : IChunkPalette
{
	private readonly Dictionary<uint, uint> _blockStates = new Dictionary<uint, uint>();

	public void Read(List<byte> buffer)
	{
		int paletteLength = VarInt.ReadNext(buffer);
		for (int i = 0; i < paletteLength; i++)
		{
			_blockStates.Add((uint)i, (uint)VarInt.ReadNext(buffer));
		}
	}

	public uint GetBlockState(uint i)
	{
		return _blockStates[i];
	}
}
