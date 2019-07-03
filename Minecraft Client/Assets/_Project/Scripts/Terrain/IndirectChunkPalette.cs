using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IndirectChunkPalette : IChunkPalette
{
	private readonly Dictionary<uint, uint> _blockStates = new Dictionary<uint, uint>();

	public int Length => _blockStates.Count;

	public void Read(List<byte> buffer)
	{
		int paletteLength = VarInt.ReadNext(buffer);

		for (int i = 0; i < paletteLength; i++)
		{
			uint blockState = (uint)VarInt.ReadNext(buffer);
			_blockStates.Add((uint)i, blockState);
		}
	}

	public uint GetBlockState(uint i)
	{
		return _blockStates[i];
	}
}
