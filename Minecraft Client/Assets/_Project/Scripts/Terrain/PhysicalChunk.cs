using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The physical representation of a chunk column in-game
/// </summary>
public class PhysicalChunk : MonoBehaviour
{
	public GameObject SectionPrefab;
	public PhysicalChunkSection[] Sections { get; } = new PhysicalChunkSection[16];
	public Chunk Chunk { get; set; } = null;

	private readonly BlockPos[] _neighborPositions = new BlockPos[]
		{
			new BlockPos() { X = 1 },
			new BlockPos() { X = -1 },
			new BlockPos() { Y = 1 },
			new BlockPos() { Y = -1 },
			new BlockPos() { Z = 1 },
			new BlockPos() { Z = -1 },
		};

	// Use this for initialization
	public void Start()
	{
		name = Chunk.Position.ToString();

		// create chunk sections
		for (int i = 0; i < Sections.Length; i++)
		{
			var newSection = Instantiate(SectionPrefab, transform.position + new Vector3(0, i * 16, 0), Quaternion.identity, transform);
			Sections[i] = newSection.GetComponent<PhysicalChunkSection>();
		}
	}

	public PhysicalChunkSection GetSection(int index)
	{
		if (index < 0 || index > 15)
			return null;
		else
			return Sections[index];
	}

	/// <summary>
	/// Generates mesh data for this chunk section
	/// </summary>
	/// <param name="chunk"></param>
	public ChunkMeshData[] GenerateMesh(ushort sections)
	{
		var finishedMeshes = new List<ChunkMeshData>();

		// store neighbor chunks so we don't have to look them up through the World
		Chunk[] neighborChunks = new Chunk[4];
		var neighborChunkPositions = World.GetNeighbors(Chunk.Position);
		for (int i = 0; i < 4; i++)
		{
			neighborChunks[i] = Chunk.World.GetChunk(neighborChunkPositions[i]);
		}

		// iterate through chunk sections
		for (int s = 0; s < 16; s++)
		{
			// check that we are rendering this chunk section
			if (((0x01 << s) & sections) == 0)
				continue;

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			// set up vertices and triangles for this section
			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			List<Vector3> normals = new List<Vector3>();
			int triangleIndex = 0;

			// don't render any blocks if chunk section isn't populated
			// but don't continue through the loop, so we still add an empty mesh and mark this chunk section as populated
			if (s * 16 <= Chunk.MaxHeight)
			{
				// iterate through each block in chunk section
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						for (int x = 0; x < 16; x++)
						{
							BlockPos pos = new BlockPos() { X = x, Y = y + (s * 16), Z = z };
							int blockIndex = Chunk.GetBlockIndex(pos);

							// check if we need to render this block at all
							if (!Chunk.BlockArray[blockIndex].IsRendered)
								continue;

							// check if the block is a solid cube or more complicated block
							if (Chunk.BlockArray[blockIndex].IsSolid)
							{
								// check neighbors to find what faces we need to render
								bool[] neighbors = new bool[6];
								for (int i = 0; i < 6; i++)
								{
									var neighborPos = _neighborPositions[i] + pos;

									// check if we can use our "locally" cached chunk data to check this block
									if (Chunk.ExistsInside(neighborPos))
									{
										neighbors[i] = Chunk.BlockArray[Chunk.GetBlockIndex(neighborPos)].IsSolid;
									}
									else
									{
										Chunk neighborChunk;

										// find which neighbor chunk the block is in
										switch (i)
										{
											case 0:
												neighborChunk = neighborChunks[0];
												break;
											case 1:
												neighborChunk = neighborChunks[1];
												break;
											case 4:
												neighborChunk = neighborChunks[2];
												break;
											case 5:
												neighborChunk = neighborChunks[3];
												break;
											default:
												neighbors[i] = false;   // this block is outside 0 <= x <= 25
												continue;
										}

										neighbors[i] = neighborChunk?.GetBlockAt(neighborPos).IsSolid ?? true;  // unloaded neighbor chunks are null. if the chunk is unloaded, say it's empty
									}
								}

								// unity-style position of this block within the chunk to offset verts
								Vector3 blockPosUnity = new Vector3(z, y, x);

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
							else
							{
								// todo: code for rendering non-solid blocks goes here
								// ex. hoppers, grass, anything with a mesh defined in a resource pack

								Debug.LogWarning("Non solid blocks cannot be rendered yet");
							}
						}
					}
				}
			}

			sw.Stop();

			// add mesh data to the finished meshes
			finishedMeshes.Add(new ChunkMeshData()
			{
				Normals = normals.ToArray(),
				Triangles = triangles.ToArray(),
				Vertices = vertices.ToArray(),
				PhysicalChunk = this,
				ChunkSection = s,
				ElapsedTime = sw.ElapsedMilliseconds
			});
		}

		return finishedMeshes.ToArray();
	}

	/*
	private bool HasAllNeighbors(bool[] blocks)
	{
		foreach (var neighbor in blocks)
			if (!neighbor)
				return false;

		return true;
	}*/

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
		PhysicalChunk mesh = other as PhysicalChunk;
		if (other == null)
			return false;
		return mesh.Chunk.Position.Equals(Chunk.Position);
	}

	public override int GetHashCode()
	{
		return Chunk.GetHashCode();
	}
}
