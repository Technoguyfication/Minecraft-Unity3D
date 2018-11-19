using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Defines a single dimension the player can play in.
/// </summary>
public class World
{
	public DimensionType Dimension { get; set; }
	public ChunkRenderer ChunkRenderer;

	private List<Chunk> _chunks;

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
		if (_chunks.Exists(c => c.Position.Equals(pos)))
			return _chunks.Find(c => c.Position.Equals(pos));
		else
			return null;
	}

	/// <summary>
	/// Adds chunk data to the world
	/// </summary>
	/// <param name="chunkData"></param>
	public void AddChunkData(ChunkDataPacket chunkData)
	{
		Debug.Log($"Adding chunk data at {chunkData.Position.X}, {chunkData.Position.Z}");

		// if the chunk already exists, add data
		// otherwise, make a new chunk
		var existingChunk = _chunks.Find(c => c.Position.Equals(chunkData.Position));
		if (existingChunk != null)	// chunk already exists
		{
			if (chunkData.GroundUpContinuous)
			{
				Debug.LogWarning($"Packet data for chunk at {chunkData.Position} tried to load GroundUpContinuous for already loaded chunk!");
				return;
			}

			existingChunk.AddChunkData(chunkData);
			ChunkRenderer.MarkChunkForRegeneration(existingChunk);
		}
		else
		{
			Chunk chunk = new Chunk(chunkData, this);
			_chunks.Add(chunk);
			ChunkRenderer.AddChunk(chunk);
		}
	}

	/// <summary>
	/// Unloads a chunk from the world.
	/// </summary>
	/// <param name="pos"></param>
	public void UnloadChunk(ChunkPos pos)
	{
		_chunks.RemoveAll(c => c.Position.Equals(pos));
		ChunkRenderer.UnloadChunk(pos);
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

