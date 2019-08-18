using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using IEnumerator = System.Collections.IEnumerator;

/// <summary>
/// Manages chunk meshes and rendering
/// </summary>
public class ChunkRenderer : MonoBehaviour
{
	public const ushort ALL_SECTIONS = 0xffff;

	public GameObject ChunkMeshPrefab;
	public DebugCanvas DebugCanvas;

	private readonly List<PhysicalChunk> _chunkMeshes = new List<PhysicalChunk>();
	//private readonly ConcurrentQueue<PhysicalChunk> _regenerationQueue = new ConcurrentQueue<PhysicalChunk>();
	private readonly ConcurrentQueue<ChunkMeshData> _finishedMeshData = new ConcurrentQueue<ChunkMeshData>();
	private readonly List<Task> _regenTasks = new List<Task>();
	private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public void Start()
	{
		StartCoroutine(AssignChunkMeshCoroutine(_cancellationTokenSource.Token));
	}

	public void OnDestroy()
	{
		_cancellationTokenSource.Cancel();
	}

	public void Update()
	{
		// remove completed regen tasks from list
		_regenTasks.RemoveAll(t => t.IsCompleted);

		if (DebugCanvas.Displaying)
		{
			//DebugCanvas.QueuedChunks = _regenerationQueue.Count;
			DebugCanvas.ProcessingChunks = _regenTasks.Count;
			DebugCanvas.FinishedChunks = _finishedMeshData.Count;
		}
	}

	private IEnumerator AssignChunkMeshCoroutine(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			// try to take the next finished mesh from the queue
			// if there are none, yield until the next frame
			ChunkMeshData meshData;
			while (!_finishedMeshData.TryDequeue(out meshData))
				yield return null;

			DebugCanvas.LifetimeFinishedChunks++;
			DebugCanvas.AverageChunkTime.Add(meshData.ElapsedTime);

			lock (_chunkMeshes)
			{
				// if the chunk has been unloaded we can't add any data to it
				if (!_chunkMeshes.Contains(meshData.PhysicalChunk))
					continue;

				Mesh mesh = new Mesh()
				{
					vertices = meshData.Vertices,
					triangles = meshData.Triangles,
					normals = meshData.Normals
				};

				// assign mesh and set generated
				var chunkSection = meshData.PhysicalChunk.Sections[meshData.ChunkSection];
				chunkSection.SetMesh(mesh);
				chunkSection.IsGenerated = true;

				// if we're over the frame budget, wait for the next frame
				/*if (Time.deltaTime > 1 / 60)
					yield return null;*/
			}
		}
	}

	/// <summary>
	/// Returns whether a chunk is generated
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public bool IsChunkSectionGenerated(ChunkSectionPos pos)
	{
		lock (_chunkMeshes)
		{
			return _chunkMeshes.Exists(c => c.Chunk.Position.Equals(pos.ChunkColumnPos) && (c.GetSection(pos.Y)?.IsGenerated ?? false));
		}
	}

	/// <summary>
	/// Adds a chunk to the list of chunks we are rendering. Does not mark it for regeneration
	/// </summary>
	/// <param name="chunk"></param>
	public void AddChunk(Chunk chunk)
	{
		var chunkMeshObject = Instantiate(ChunkMeshPrefab, new Vector3((chunk.Position.Z * 16) + 0.5f, 0.5f, (chunk.Position.X * 16) + 0.5f), Quaternion.identity, transform);
		var chunkMesh = chunkMeshObject.GetComponent<PhysicalChunk>();
		chunkMesh.Chunk = chunk;
		chunkMesh.name = chunk.Position.ToString();

		lock (_chunkMeshes)
		{
			// add chunkmesh to list
			_chunkMeshes.Add(chunkMesh);
		}
	}

	/// <summary>
	/// Unloads a chunkmesh
	/// </summary>
	/// <param name="chunkMesh"></param>
	public void UnloadChunkMesh(PhysicalChunk chunkMesh)
	{
		lock (_chunkMeshes)
		{
			// check if chunk exists
			if (!_chunkMeshes.Contains(chunkMesh) || chunkMesh == null)
				return;

			Destroy(chunkMesh.gameObject);
			_chunkMeshes.Remove(chunkMesh);
		}
	}

	/// <summary>
	/// Unloads a chunkmesh at the specified position
	/// </summary>
	/// <param name="pos"></param>
	public void UnloadChunk(ChunkColumnPos pos)
	{
		lock (_chunkMeshes)
		{
			UnloadChunkMesh(_chunkMeshes.Find(cm =>
			cm.Chunk.Position.Equals(pos)));
		}
	}

	/// <summary>
	/// Unloads all chunkmeshes
	/// </summary>
	public void UnloadAllChunkMeshes()
	{
		lock (_chunkMeshes)
		{
			foreach (var chunkMesh in _chunkMeshes)
			{
				UnloadChunkMesh(chunkMesh);
			}
		}
	}

	/// <summary>
	/// Marks that we need to regenerate the mesh for a chunk
	/// </summary>
	/// <param name="mesh"></param>
	public void MarkChunkForRegeneration(PhysicalChunk mesh, ushort sections)
	{
		StartCoroutine(RegenerateChunkCoroutine(mesh, sections));
	}

	/// <summary>
	/// Marks that we need to regenerate the mesh for a chunk.
	/// </summary>
	/// <param name="chunk"></param>
	public void MarkChunkForRegeneration(Chunk chunk, ushort sections)
	{
		MarkChunkForRegeneration(GetPhysicalChunk(chunk), sections);
	}

	/// <summary>
	/// Gets the Physical chunk for a chunk, or null if it doesn't exist
	/// </summary>
	/// <param name="chunk"></param>
	/// <returns></returns>
	public PhysicalChunk GetPhysicalChunk(Chunk chunk)
	{
		lock (_chunkMeshes)
		{
			return _chunkMeshes.Find(cm => cm.Chunk.Equals(chunk));
		}
	}

	/// <summary>
	/// Gets the physical chunk at a position, or null if it doesn't exist
	/// </summary>
	/// <param name="chunkPos"></param>
	/// <returns></returns>
	public PhysicalChunk GetPhysicalChunk(ChunkColumnPos chunkPos)
	{
		lock (_chunkMeshes)
		{
			return _chunkMeshes.Find(cm => cm.Chunk.Position.Equals(chunkPos));
		}
	}

	public void AddGeneratedChunkMesh(ChunkMeshData data)
	{
		_finishedMeshData.Enqueue(data);
	}

	private IEnumerator RegenerateChunkCoroutine(PhysicalChunk physicalChunk, ushort sections)
	{
		// wait for an available thread
		while (_regenTasks.Count >= SystemInfo.processorCount)
			yield return null;

		// generate the mesh on another thread
		var task = Task.Run(() =>
		{
			var finishedRender = physicalChunk.GenerateMesh(sections);

			// add finished mesh data to queue so it can be assigned to the mesh filter
			foreach (var meshData in finishedRender)
			{
				_finishedMeshData.Enqueue(meshData);
			}
		});

		// add task to list of tasks
		_regenTasks.Add(task);

		// wait for task to complete
		while (!task.IsCompleted)
			yield return null;

		// remove finished task from task list
		_regenTasks.Remove(task);

		if (task.IsFaulted)
			throw task.Exception;
	}
}

/// <summary>
/// Represents raw vertices used to create a mesh of a chunk section in-game
/// </summary>
public struct ChunkMeshData
{
	public PhysicalChunk PhysicalChunk { get; set; }
	public int ChunkSection { get; set; }
	public Vector3[] Vertices { get; set; }
	public Vector3[] Normals { get; set; }
	public int[] Triangles { get; set; }
	public float ElapsedTime { get; set; }
}