using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class World
{
	public DimensionType Dimension { get; set; }

	private readonly List<Chunk> _chunks;

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
		if (!_chunks.Exists(c => c.Position.Equals(pos)))
			return null;
		else
			return _chunks.Find(c => c.Position.Equals(pos));
	}

	public enum DimensionType
	{
		OVERWORLD = 0,
		NETHER = -1,
		END = 1
	}
}

