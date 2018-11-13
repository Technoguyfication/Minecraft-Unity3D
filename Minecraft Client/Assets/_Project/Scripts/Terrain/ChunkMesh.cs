using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkMesh : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// Generates a mesh from a chunk
	/// </summary>
	/// <param name="chunk"></param>
	public void GenerateMesh(Chunk chunk)
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		int triangleIndex = 0;

		// iterate through each block in chunk
		for (int x = 4; x < 16; x++)
		{
			for (int y = 65; y < 256; y++)
			{
				for (int z = 4; z < 16; z++)
				{
					BlockPos pos = new BlockPos { X = x, Y = y, Z = z };
					Block[] neighbors = chunk.World.GetNeighbors(pos.GetWorldPos(chunk));
					Block block = chunk.World.GetBlock(pos.GetWorldPos(chunk));
					Vector3 blockPosUnity = new Vector3(pos.Z, pos.Y, pos.X);

					// check if we need to render this block
					if (HasAllNeighbors(neighbors) || !(block?.IsSolid ?? false))
						continue;

					// iterate through each face and add to mesh if it's visible
					for (int i = 0; i < 6; i++)
					{
						if (!(neighbors[i]?.IsSolid ?? false))
						{
							Vector3[] newVertices = new Vector3[4];
							Vector3[] faceVertices = GetVertices(i);

							// translate vertices to relative block position
							for (int j = 0; j < 4; j++)
							{
								newVertices[j] = faceVertices[j] + blockPosUnity;
							}

							vertices.AddRange(newVertices); // add vertices to mesh

							// connect triangles
							triangles.AddRange(new int[] { triangleIndex, 1 + triangleIndex, 2 + triangleIndex, triangleIndex, 2 + triangleIndex, 3 + triangleIndex });
							triangleIndex += 4;
						}
					}
				}
			}
		}

		Mesh newMesh = new Mesh
		{
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray()
		};
		newMesh.RecalculateNormals();
		GetComponent<MeshFilter>().mesh = newMesh;

	}

	private bool HasAllNeighbors(Block[] blocks)
	{
		foreach (var neighbor in blocks)
			if (neighbor?.Type == Block.BlockType.AIR)
				return false;

		return true;
	}

	/// <summary>
	/// Get vertices for face inx: (minecraft: +X -X +Y -Y +Z -Z) (unity: +Z -Z +Y -Y +X -X)
	/// </summary>
	/// <param name="face"></param>
	/// <returns></returns>
	private Vector3[] GetVertices(int face)
	{
		switch (face)
		{
			case 0:
				return new Vector3[]
				{
					new Vector3(0.5f, 0.5f, 0.5f),
					new Vector3(-0.5f, 0.5f, 0.5f),
					new Vector3(-0.5f, -0.5f, 0.5f),
					new Vector3(0.5f, -0.5f, 0.5f),
				};
			case 1:
				return new Vector3[]
				{
					new Vector3(-0.5f, 0.5f, -0.5f),
					new Vector3(0.5f, 0.5f, -0.5f),
					new Vector3(0.5f, -0.5f, -0.5f),
					new Vector3(-0.5f, -0.5f, -0.5f),
				};
			case 2:
				return new Vector3[]
				{
					new Vector3(0.5f, 0.5f, -0.5f),
					new Vector3(-0.5f, 0.5f, -0.5f),
					new Vector3(0.5f, 0.5f, -0.5f),
					new Vector3(-0.5f, 0.5f, 0.5f),
				};
			case 3:
				return new Vector3[]
				{
					new Vector3(0.5f, -0.5f, 0.5f),
					new Vector3(-0.5f, -0.5f, 0.5f),
					new Vector3(-0.5f, -0.5f, -0.5f),
					new Vector3(0.5f, -0.5f, -0.5f),
				};
			case 4:
				return new Vector3[]
				{
					new Vector3(0.5f, 0.5f, -0.5f),
					new Vector3(0.5f, 0.5f, 0.5f),
					new Vector3(0.5f, -0.5f, 0.5f),
					new Vector3(0.5f, -0.5f, -0.5f),
				};
			case 5:
				return new Vector3[]
				{
					new Vector3(-0.5f, 0.5f, 0.5f),
					new Vector3(-0.5f, 0.5f, -0.5f),
					new Vector3(-0.5f, -0.5f, -0.5f),
					new Vector3(-0.5f, -0.5f, 0.5f),
				};

			default:
				throw new ArgumentException($"Face {face} does not exist on cube!");
		}
	}
}
