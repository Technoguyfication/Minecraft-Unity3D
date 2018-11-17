using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Manages chunk meshes and rendering
/// </summary>
public class ChunkRenderer
{
	private List<ChunkMesh> _meshes = new List<ChunkMesh>();
	private List<ChunkMesh> _regenerationQueue = new List<ChunkMesh>();

	public void AddChunk(Chunk chunk)
	{
		ChunkMesh mesh = new ChunkMesh()
		{
			Chunk = chunk
		};

	}
}

