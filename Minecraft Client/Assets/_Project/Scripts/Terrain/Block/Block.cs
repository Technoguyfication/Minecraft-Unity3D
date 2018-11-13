using System;
using System.Collections;
using System.Collections.Generic;

public class Block {
	public bool IsSolid { get; }

	public Block()
	{
		Type = BlockType.AIR;
	}

	public Block(BlockType type)
	{
		Type = type;
	}

	public BlockType Type { get; set; }

	public override string ToString()
	{
		return Type.ToString();
	}

	public enum BlockType
	{
		// todo: add all block types here.
		AIR = 0,
		CAVE_AIR = 8592,
		DIORITE = 4,
		GRANITE = 2,
		STONE = 1,
		GRASS = 9,
		DIRT = 10,
		COBBLESTONE = 14
	}
}
