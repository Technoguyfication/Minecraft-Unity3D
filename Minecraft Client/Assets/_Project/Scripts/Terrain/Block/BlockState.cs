using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a single state of a block. Can be casted to and from <see cref="int"/>
/// </summary>
public struct BlockState
{
	public BlockState(BlockType type)
	{
		State = type;
	}

	/// <summary>
	/// Gets if the block is solid in all four corners, for use in rendering
	/// </summary>
	public bool IsSolid => State != BlockType.AIR && State != BlockType.CAVE_AIR && State != BlockType.VOID_AIR /*&& State != BlockType.WATER*/;

	/// <summary>
	/// Whether this block can be rendered I.E. not air, etc.
	/// </summary>
	public bool IsRendered => IsSolid;

	public BlockType State { get; set; }

	public static explicit operator uint(BlockState s)
	{
		return (uint)s.State;
	}

	public static explicit operator BlockState(uint i)
	{
		return new BlockState() { State = (BlockType)i };
	}
}
