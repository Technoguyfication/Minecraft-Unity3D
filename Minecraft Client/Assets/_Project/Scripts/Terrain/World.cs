using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class World
{
	public DimensionType Dimension { get; set; }

	private List<Chunk> _chunks;
	private ChunkRenderer _chunkRenderer;

	public World()
	{
		_chunks = new List<Chunk>();
	}

	/// <summary>
	/// Returns the block at the specified position, or null if it cannot be found
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public BlockState GetBlock(BlockPos pos)
	{
		Chunk c = GetChunk(pos.GetChunk());

		if (c == null)
			return new BlockState(BlockType.VOID_AIR);	// use void air for unloaded chunks
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

	public void AddChunk(Chunk chunk)
	{
		// make sure we don't leak chunks
		if (_chunks.Contains(chunk))
			throw new ArgumentException("Chunk already exists in world!");

		_chunks.Add(chunk);
	}

	public void UnloadChunk(ChunkPos pos)
	{
		_chunks.RemoveAll(c => c.Position.Equals(pos));
	}

	/// <summary>
	/// Gets whether 6 blocks surrounding a block are solid, in order: +X -X +Y -Y +Z -Z
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public bool[] GetNeighbors(BlockPos pos)
	{
		// this code is so ugly but idk what else to do with it honestly
		return new bool[6]
		{
			GetBlock(new BlockPos() { X = pos.X + 1, Y = pos.Y, Z = pos.Z }).IsSolid,
			GetBlock(new BlockPos() { X = pos.X - 1, Y = pos.Y, Z = pos.Z }).IsSolid,
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y + 1, Z = pos.Z }).IsSolid,
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y - 1, Z = pos.Z }).IsSolid,
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y, Z = pos.Z + 1 }).IsSolid,
			GetBlock(new BlockPos() { X = pos.X, Y = pos.Y, Z = pos.Z - 1 }).IsSolid,
		};
	}

	public enum DimensionType
	{
		OVERWORLD = 0,
		NETHER = -1,
		END = 1
	}
}

