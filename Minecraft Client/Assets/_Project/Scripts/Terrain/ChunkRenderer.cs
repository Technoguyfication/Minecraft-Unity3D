using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages chunk meshes and rendering
/// </summary>
public class ChunkRenderer : MonoBehaviour
{
	public GameObject ChunkMeshPrefab;
	public DebugCanvas DebugCanvas;

	private List<ChunkMesh> _chunkMeshes = new List<ChunkMesh>();
	private BlockingCollection<ChunkMesh> _regenerationQueue = new BlockingCollection<ChunkMesh>();
	private List<ChunkMeshData> _finishedMeshData = new List<ChunkMeshData>();
	private Task _regenerationTask;
	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	private void Start()
	{
		// start chunk regen task
		_regenerationTask = new Task(() =>
		{
			RegenerationWorker(_cancellationTokenSource.Token);
		}, _cancellationTokenSource.Token);
		_regenerationTask.Start();
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

		// add generated meshes to chunks
		lock (_finishedMeshData)
		{
			lock (_chunkMeshes)
			{
				foreach (var meshData in _finishedMeshData)
				{
					// if the chunk has been unloaded the mesh data will become an orhpan, so ignore it
					if (!_chunkMeshes.Contains(meshData.ChunkMesh))
						continue;

					Mesh mesh = new Mesh()
					{
						vertices = meshData.Vertices,
						triangles = meshData.Triangles,
						normals = meshData.Normals
					};

					meshData.ChunkMesh.SetMesh(mesh);
					meshData.ChunkMesh.IsGenerated = true;

					// add chunk time to debug screen
					DebugCanvas.AverageChunkTime.Add(meshData.Time);
				}
				_finishedMeshData.Clear();
			}
		}
	}

	/// <summary>
	/// Returns whether a chunk is generated
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public bool IsChunkGenerated(ChunkPos pos)
	{
		lock (_chunkMeshes)
		{
			return _chunkMeshes.Exists(c => c.Chunk.Position.Equals(pos) && c.IsGenerated);
		}
	}

	/// <summary>
	/// Adds a chunk to the list of chunks we are rendering, and marks it for mesh regeneration
	/// </summary>
	/// <param name="chunk"></param>
	public void AddChunk(Chunk chunk)
	{
		var chunkMeshObject = Instantiate(ChunkMeshPrefab, new Vector3((chunk.Position.Z * 16) + 0.5f, 0.5f, (chunk.Position.X * 16) + 0.5f), Quaternion.identity);
		chunkMeshObject.transform.parent = this.transform;
		var chunkMesh = chunkMeshObject.GetComponent<ChunkMesh>();
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
	public void UnloadChunkMesh(ChunkMesh chunkMesh)
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
	public void UnloadChunk(ChunkPos pos)
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
	public void MarkChunkForRegeneration(ChunkMesh mesh)
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
	private ChunkMesh GetChunkMesh(Chunk chunk)
	{
		lock (_chunkMeshes)
		{
			return _chunkMeshes.Find(cm => cm.Chunk.Equals(chunk));
		}
	}

	private void RegenerationWorker(CancellationToken token)
	{
		var sw = new System.Diagnostics.Stopwatch();

		UnityEngine.Profiling.Profiler.BeginThreadProfiling("rendering", "chunk mesh generation");

		while (!token.IsCancellationRequested)
		{
			// generate mesh on another thread
			ChunkMesh chunkMesh;
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
			var meshData = chunkMesh.GenerateMesh();
			sw.Stop();

			meshData.Time = sw.Elapsed.Milliseconds / 1000f;

			// add finished mesh to queue so we can quickly add it to our chunk object
			lock (_finishedMeshData)
			{
				_finishedMeshData.Add(meshData);
			}
		}

		UnityEngine.Profiling.Profiler.EndThreadProfiling();
	}
}

public struct ChunkMeshData
{
	public ChunkMesh ChunkMesh { get; set; }
	public Vector3[] Vertices { get; set; }
	public Vector3[] Normals { get; set; }
	public int[] Triangles { get; set; }
	public float Time { get; set; }
}
