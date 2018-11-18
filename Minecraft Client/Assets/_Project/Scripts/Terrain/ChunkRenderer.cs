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

	private List<ChunkMesh> _chunkMeshes = new List<ChunkMesh>();
	private BlockingCollection<ChunkMesh> _regenerationQueue = new BlockingCollection<ChunkMesh>();
	private BlockingCollection<ChunkMeshData> _finishedMeshes = new BlockingCollection<ChunkMeshData>();
	private Task _regenerationTask;
	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public ChunkRenderer()
	{

	}

	private void Start()
	{
		// start chunk regen task
		_regenerationTask = new Task(() =>
		{
			RegenerationWorker(_cancellationTokenSource.Token);
		}, _cancellationTokenSource.Token);
		_regenerationTask.Start();
	}

	private void Update()
	{
		// check regeneration worker
		if (_regenerationTask.IsFaulted)
			throw _regenerationTask.Exception;

		// add generated meshes to chunks
		if (_finishedMeshes.Count > 0)
		{
			foreach (var meshData in _finishedMeshes)
			{
				Mesh mesh = new Mesh()
				{
					vertices = meshData.Vertices,
					triangles = meshData.Triangles
				};

				meshData.Chunk.SetMesh(mesh);
			}
		}
	}

	/// <summary>
	/// Adds a chunk to the list of chunks we are rendering, and marks it for mesh regeneration
	/// </summary>
	/// <param name="chunk"></param>
	public void AddChunk(Chunk chunk)
	{
		var chunkMeshObject = Instantiate(ChunkMeshPrefab, new Vector3(chunk.Position.Z * 16, 0, chunk.Position.X * 16), Quaternion.identity);
		var chunkMesh = chunkMeshObject.GetComponent<ChunkMesh>();
		chunkMesh.Chunk = chunk;
		chunkMesh.name = chunk.Position.ToString();

		// add chunkmesh to list
		_chunkMeshes.Add(chunkMesh);

		// (re)generate
		MarkChunkForRegeneration(chunkMesh);
	}

	/// <summary>
	/// Unloads a chunk and destroys it's mesh in the world
	/// </summary>
	/// <param name="chunk"></param>
	public void UnloadChunk(Chunk chunk)
	{
		var chunkMesh = _chunkMeshes.Find(cm => cm.Chunk.Equals(chunk));
		UnloadChunk(chunkMesh);
	}

	public void UnloadChunk(ChunkMesh chunkMesh)
	{
		// check if chunk exists
		if (!_chunkMeshes.Contains(chunkMesh) || chunkMesh == null)
			return;

		_chunkMeshes.Remove(chunkMesh);
		Destroy(chunkMesh.gameObject);
	}

	public void UnloadChunk(ChunkPos pos)
	{
		UnloadChunk(_chunkMeshes.Find(cm => 
		cm.Chunk.Position.Equals(pos)));
	}

	public void UnloadAllChunks()
	{
		foreach (var chunkMesh in _chunkMeshes)
		{
			UnloadChunk(chunkMesh);
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
		MarkChunkForRegeneration(GetChunkMesh(chunk));
	}

	/// <summary>
	/// Gets our ChunkMesh for a chun, or null if it doesn't exist
	/// </summary>
	/// <param name="chunk"></param>
	/// <returns></returns>
	private ChunkMesh GetChunkMesh(Chunk chunk)
	{
		return _chunkMeshes.Find(cm => cm.Chunk.Equals(chunk));
	}

	private void RegenerationWorker(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			// generate mesh on another thread
			ChunkMesh chunkMesh = _regenerationQueue.Take(token);

			// check that we are still keeping track of the chunk (i.e. it's not unloaded)
			if (!_chunkMeshes.Contains(chunkMesh))
				continue;

			var meshData = chunkMesh.GenerateMesh();

			// add finished mesh to queue so we can quickly add it to our chunk object
			_finishedMeshes.Add(meshData);
		}
	}
}

public struct ChunkMeshData
{
	public ChunkMesh Chunk { get; set; }
	public Vector3[] Vertices { get; set; }
	public int[] Triangles { get; set; }
}

