using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The physical representation of a chunk in-game
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkMesh : MonoBehaviour
{
	public bool IsGenerated { get; set; } = false;
	public Chunk Chunk { get; set; } = null;

	// Use this for initialization
	void Start()
	{
		name = Chunk.Position.ToString();
	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// Generates a mesh from a chunk
	/// </summary>
	/// <param name="chunk"></param>
	public ChunkMeshData GenerateMesh()
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		int triangleIndex = 0;

		// iterate through each block in chunk
		for (int z = 0; z < 16; z++)
		{
			for (int y = 0; y < Chunk.MaxHeight; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					BlockPos pos = new BlockPos { X = x, Y = y, Z = z };
					BlockState block = Chunk.World.GetBlock(pos.GetWorldPos(Chunk));

					// check if we need to render this block
					if (!block.IsRendered)
						continue;

					// if this block is surrounded by solid blocks we dont't need to render it
					bool[] neighbors = Chunk.World.GetNeighbors(pos.GetWorldPos(Chunk));
					if (HasAllNeighbors(neighbors))
						continue;

					// unity-style position of this block within the chunk to offset verts
					Vector3 blockPosUnity = new Vector3(pos.Z, pos.Y, pos.X);

					// iterate through each face and add to mesh if it's visible
					for (int i = 0; i < 6; i++)
					{
						if (!neighbors[i])
						{
							Vector3[] newVertices = new Vector3[4];
							Vector3[] faceVertices = GetVertices(i);

							// translate vertices to relative block position so we can add them to the right place in the mesh
							// also add normals
							for (int j = 0; j < 4; j++)
							{
								newVertices[j] = faceVertices[j] + blockPosUnity;
								normals.Add(GetNormals(i));
							}

							// add mesh vertices
							vertices.AddRange(newVertices);

							// connect triangles
							triangles.AddRange(new int[] { triangleIndex, 1 + triangleIndex, 2 + triangleIndex, triangleIndex, 2 + triangleIndex, 3 + triangleIndex });
							triangleIndex += 4;
						}
					}
				}
			}
		}

		return new ChunkMeshData()
		{
			ChunkMesh = this,
			Vertices = vertices.ToArray(),
			Normals = normals.ToArray(),
			Triangles = triangles.ToArray()
		};
	}

	public void SetMesh(Mesh mesh)
	{
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	private bool HasAllNeighbors(bool[] blocks)
	{
		foreach (var neighbor in blocks)
			if (!neighbor)
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
					new Vector3(-0.5f, 0.5f, 0.5f),
					new Vector3(0.5f, 0.5f, 0.5f),
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

	private Vector3 GetNormals(int face)
	{
		switch (face)
		{
			case 0:
				return Vector3.forward;
			case 1:
				return Vector3.back;
			case 2:
				return Vector3.up;
			case 3:
				return Vector3.down;
			case 4:
				return Vector3.right;
			case 5:
				return Vector3.left;
			default:
				throw new ArgumentException($"Face {face} does not exist");
		}
	}

	public override bool Equals(object other)
	{
		ChunkMesh mesh = other as ChunkMesh;
		if (other == null)
			return false;
		return mesh.Chunk.Position.Equals(Chunk.Position);
	}

	public override int GetHashCode()
	{
		return Chunk.GetHashCode();
	}
}
