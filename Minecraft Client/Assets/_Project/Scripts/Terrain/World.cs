using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using IEnumerator = System.Collections.IEnumerator;

/// <summary>
/// Defines a single dimension the player can play in.
/// </summary>
public class World
{
	public DimensionType Dimension { get; set; }
	public ChunkRenderer ChunkRenderer;

	private readonly List<Chunk> _chunks;
	private readonly List<Task> _packetDecodeTasks = new List<Task>();

	private static readonly ChunkColumnPos[] _neighborChunkPositions = new ChunkColumnPos[]
	{
			new ChunkColumnPos(1, 0),
			new ChunkColumnPos(-1, 0),
			new ChunkColumnPos(0, 1),
			new ChunkColumnPos(0, -1),
	};

	/// <summary>
	/// Returns an empty chunk
	/// </summary>
	/// <returns></returns>
	public Chunk EmptyChunk => new Chunk(0, 0, this);

	public World()
	{
		_chunks = new List<Chunk>();
	}

	/// <summary>
	/// Gets the positions of each of a chunk column's neighbors
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public static ChunkColumnPos[] GetNeighbors(ChunkColumnPos pos)
	{
		// don't use linq here because it's slow and this function needs to run _fast_
		ChunkColumnPos[] neighbors = new ChunkColumnPos[4];
		for (int i = 0; i < 4; i++)
		{
			neighbors[i] = pos + _neighborChunkPositions[i];
		}

		return neighbors;
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
			Chunk c = GetChunk(pos.GetChunkColumnPos());

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
	public Chunk GetChunk(ChunkColumnPos pos)
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
	public bool IsChunkLoaded(ChunkColumnPos pos)
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
	public IEnumerator AddChunkDataCoroutine(ChunkDataPacket chunkData)
	{
		Chunk chunk;
		lock (_chunks)
		{
			// if the chunk already exists, add data
			// otherwise, make a new chunk
			chunk = _chunks.Find(c => c.Position.Equals(chunkData.Position));
			if (chunk != null)  // chunk already exists
			{
				if (chunkData.GroundUpContinuous)
				{
					Debug.LogWarning($"Packet data for chunk at {chunkData.Position} tried to load GroundUpContinuous for already loaded chunk!");
					yield break;
				}
			}
			else
			{
				// need to create a new chunk
				chunk = new Chunk(chunkData.ChunkX, chunkData.ChunkZ, this);
				_chunks.Add(chunk);
				ChunkRenderer.AddChunk(chunk);
			}
		}

		while (_packetDecodeTasks.Count >= SystemInfo.processorCount)
			yield return null;

		// add chunk data to chunk in another thread
		var task = Task.Run(() =>
		{
			Profiler.BeginThreadProfiling("Add chunk data", "chunk decoder worker");
			Profiler.BeginSample($"Add chunk data {chunk.ToString()}");
			chunk.AddChunkData(chunkData);
			Profiler.EndSample();
			Profiler.EndThreadProfiling();
		});

		_packetDecodeTasks.Add(task);

		// wait for task to complete
		while (!task.IsCompleted)
			yield return null;

		_packetDecodeTasks.Remove(task);

		// check for exceptions
		if (task.IsFaulted)
			throw task.Exception;

		// regenerate chunk sections
		/* we don't use the bitmask provided in the packet here becase we want
		 * to "render" all chunks, including empty ones, so that the game
		 * knows they exist and aren't unloaded
		 * 
		 * plus, the renderer won't render chunks above the bitmask provided in
		 * this packet anyways due to the Chunk.MaxHeight property
		 * */
		ChunkRenderer.MarkChunkForRegeneration(chunk, ChunkRenderer.ALL_SECTIONS);
	}

	/// <summary>
	/// Unloads a chunk from the world.
	/// </summary>
	/// <param name="pos"></param>
	public void UnloadChunk(ChunkColumnPos pos)
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

