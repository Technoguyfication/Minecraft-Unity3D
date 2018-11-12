using System;
using System.Collections;
using System.Collections.Generic;

public class Block {

	public Block()
	{
		Type = BlockType.AIR;
	}

	public Block(BlockType type)
	{
		Type = type;
	}

	public BlockType Type { get; set; }

	public enum BlockType
	{
		// todo: add all block types here.
		AIR = 0,
		STONE = 1,
		GRASS = 2,
		DIRT = 3,
		COBBLESTONE = 4
	}
}
