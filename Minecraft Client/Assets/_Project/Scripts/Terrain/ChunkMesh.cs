using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkMesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Generates a mesh from a chunk
	/// </summary>
	/// <param name="chunk"></param>
	public void GenerateMesh(Chunk chunk)
	{
		List<int> vertices = new List<int>();
		List<int> triangles = new List<int>();

		// iterate through each block in chunk
		for (int x = 0; x < 16; x++)
		{
			for (int y = 0; y < 256; y++)
			{
				for (int z = 0; z < 16; z++)
				{
					BlockPos pos = new BlockPos { X = x, Y = y, Z = z };
					Block block = chunk.World.GetBlock(pos);
					Block[] neighbors = chunk.World.GetNeighbors(
				}
			}
		}
	}

	private bool HasAllNeighbors(Block[] blocks)
	{
		foreach (var neighbor in blocks)
			if (neighbor?.Type == Block.BlockType.AIR)
				return false;

		return true;
	}
}
