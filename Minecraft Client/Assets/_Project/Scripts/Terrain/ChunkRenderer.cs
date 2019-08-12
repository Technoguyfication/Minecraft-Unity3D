using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

/// <summary>
/// Manages chunk meshes and rendering
/// </summary>
public class ChunkRenderer : MonoBehaviour
{
	public GameObject ChunkMeshPrefab;
	public DebugCanvas DebugCanvas;

	private readonly List<PhysicalChunk> _chunkMeshes = new List<PhysicalChunk>();
	private readonly BlockingCollection<PhysicalChunk> _regenerationQueue = new BlockingCollection<PhysicalChunk>();
	private readonly ConcurrentQueue<(int, ChunkMeshData)> _finishedMeshData = new ConcurrentQueue<(int, ChunkMeshData)>();
	private Task _regenerationTask;
	private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	private void Start()
	{
		// start chunk regen task
		_regenerationTask = new Task(() =>
		{
			RegenerationWorker(_cancellationTokenSource.Token);
		}, _cancellationTokenSource.Token);
		_regenerationTask.Start();

		StartCoroutine(AssignChunkMeshCoroutine(_cancellationTokenSource.Token));
	}

	private void OnDestroy()
	{
		_cancellationTokenSource.Cancel();
	}

	private void Update()
	{
		// check regeneration worker
		if (_regenerationTask.IsFaulted)
			throw _regenerationTask.Exception;

		DebugCanvas.FinishedChunks = _finishedMeshData.Count;
	}

	private IEnumerator AssignChunkMeshCoroutine(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			// try to take the next finished mesh from the queue
			// if there are none, yield until the next frame
			if (!_finishedMeshData.TryDequeue(out (int, ChunkMeshData) finishedMesh))
				yield return null;

			var sectionIndex = finishedMesh.Item1;
			var meshData = finishedMesh.Item2;
			lock (_chunkMeshes)
			{
				// if the chunk has been unloaded the mesh data will become an orhpan, so ignore it
				if (!_chunkMeshes.Contains(meshData.PhysicalChunk))
					continue;

				Mesh mesh = new Mesh()
				{
					vertices = meshData.Vertices,
					triangles = meshData.Triangles,
					normals = meshData.Normals
				};

				// assign mesh and set generated
				var chunkSection = meshData.PhysicalChunk.Sections[sectionIndex];
				chunkSection.SetMesh(mesh);
				chunkSection.IsGenerated = true;

				// if we're over the frame budget, wait for the next frame
				if (Time.deltaTime > 1 / 60)
					yield return null;
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
	/// Adds a chunk to the list of chunks we are rendering, and marks it for mesh regeneration
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

			// (re)generate
			MarkChunkForRegeneration(chunkMesh);
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
	public void MarkChunkForRegeneration(PhysicalChunk mesh)
	{
		_regenerationQueue.Add(mesh);
	}

	/// <summary>
	/// Marks that we need to regenerate the mesh for a chunk
	/// </summary>
	/// <param name="chunk"></param>
	public void MarkChunkForRegeneration(Chunk chunk)
	{
		lock (_chunkMeshes)
		{
			MarkChunkForRegeneration(GetChunkMesh(chunk));
		}
	}

	/// <summary>
	/// Gets our ChunkMesh for a chun, or null if it doesn't exist
	/// </summary>
	/// <param name="chunk"></param>
	/// <returns></returns>
	private PhysicalChunk GetChunkMesh(Chunk chunk)
	{
		lock (_chunkMeshes)
		{
			return _chunkMeshes.Find(cm => cm.Chunk.Equals(chunk));
		}
	}

	private void RegenerationWorker(CancellationToken token)
	{
		var sw = new System.Diagnostics.Stopwatch();

		while (!token.IsCancellationRequested)
		{
			DebugCanvas.QueuedChunks = _regenerationQueue.Count;

			// generate mesh on another thread
			PhysicalChunk chunkMesh;
			try
			{
				chunkMesh = _regenerationQueue.Take(token);
			}
			catch (OperationCanceledException)
			{
				break;
			}

			// check that we are still keeping track of the chunk (i.e. it's not unloaded)
			if (!_chunkMeshes.Contains(chunkMesh))
				continue;

			// time how long it takes to generate mesh
			sw.Restart();
			var meshData = chunkMesh.GenerateMesh(0xffff);
			sw.Stop();

			// add chunk time to debug screen
			DebugCanvas.AverageChunkTime.Add(sw.Elapsed.Milliseconds / 1000f);

			// add finished mesh data to queue so it can be assigned to the mesh filter
			foreach (var data in meshData)
			{
				_finishedMeshData.Enqueue(data);
			}
		}
	}
}

/// <summary>
/// Represents raw vertices used to create a mesh of a chunk section in-game
/// </summary>
public struct ChunkMeshData
{
	public PhysicalChunk PhysicalChunk { get; set; }
	public Vector3[] Vertices { get; set; }
	public Vector3[] Normals { get; set; }
	public int[] Triangles { get; set; }
	public bool InitialGeneration { get; set; }
}
