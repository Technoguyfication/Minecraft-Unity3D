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

	private readonly List<Chunk> _chunks;
	private readonly DebugCanvas _debugCanvas;

	/// <summary>
	/// Returns an empty chunk
	/// </summary>
	/// <returns></returns>
	public Chunk EmptyChunk => new Chunk(0, 0, this);

	public World(DebugCanvas debug = null)
	{
		_chunks = new List<Chunk>();
		_debugCanvas = debug;
	}

	/// <summary>
	/// Returns the block at the specified position, or null if it cannot be found
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public BlockState GetBlock(BlockPos pos)
	{
		lock (_chunks)
		{
			Chunk c = GetChunk(pos.GetChunk());

			if (c == null)
				return new BlockState(BlockType.VOID_AIR);  // use void air for unloaded chunks
			else
				return c.GetBlockAt(pos);
		}
	}

	/// <summary>
	/// Returns the chunk at the specified position or null if it cannot be found
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public Chunk GetChunk(ChunkPos pos)
	{
		lock (_chunks)
		{
			if (IsChunkLoaded(pos))
				return _chunks.Find(c => c.Position.Equals(pos));
			else
				return null;
		}
	}

	/// <summary>
	/// Returns whether the client has a chunk loaded
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public bool IsChunkLoaded(ChunkPos pos)
	{
		lock (_chunks)
		{
			return _chunks.Exists(c => c.Position.Equals(pos));
		}
	}

	/// <summary>
	/// Adds chunk data to the world
	/// </summary>
	/// <param name="chunkData"></param>
	public void AddChunkData(ChunkDataPacket chunkData)
	{
		lock (_chunks)
		{
			// if the chunk already exists, add data
			// otherwise, make a new chunk
			var existingChunk = _chunks.Find(c => c.Position.Equals(chunkData.Position));
			if (existingChunk != null)  // chunk already exists
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
	}

	/// <summary>
	/// Unloads a chunk from the world.
	/// </summary>
	/// <param name="pos"></param>
	public void UnloadChunk(ChunkPos pos)
	{
		lock (_chunks)
		{
			ChunkRenderer.UnloadChunk(pos);
			_chunks.RemoveAll(c => c.Position.Equals(pos));
		}
	}

	public enum DimensionType
	{
		OVERWORLD = 0,
		NETHER = -1,
		END = 1
	}
}

