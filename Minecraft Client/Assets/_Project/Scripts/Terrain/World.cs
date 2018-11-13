using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class World
{
	public DimensionType Dimension { get; set; }

	// only for now until i get chunk loading implemented
	public List<Chunk> _chunks;

	public World()
	{
		_chunks = new List<Chunk>();
	}

	/// <summary>
	/// Returns the block at the specified position, or null if it cannot be found
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public Block GetBlock(BlockPos pos)
	{
		Chunk c = GetChunk(pos.GetChunk());

		if (c == null)
			return null;
		else
			return c.GetBlockAt(pos);
	}

	/// <summary>
	/// Returns the chunk at the specified position or null if it cannot be found
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public Chunk GetChunk(ChunkPos pos)
	{
		return _chunks.Find(c => c.Position.Equals(pos));
	}

	/// <summary>
	/// Gets the 6 blocks surrounding a block, in order: +X -X +Y -Y +Z -Z
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public Block[] GetNeighbors(BlockPos pos)
	{
		return new Block[6]
		{
			GetBlock(new BlockPos() { X = pos.X + 1, Y = pos.Y, Z = pos.Z }),
			GetBlock(new BlockPos() { X = pos.X - 1, Y = pos.Y, Z = pos.Z }),
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y + 1, Z = pos.Z }),
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y - 1, Z = pos.Z }),
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y, Z = pos.Z + 1 }),
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y, Z = pos.Z - 1 }),
		};
	}

	public enum DimensionType
	{
		OVERWORLD = 0,
		NETHER = -1,
		END = 1
	}
}

