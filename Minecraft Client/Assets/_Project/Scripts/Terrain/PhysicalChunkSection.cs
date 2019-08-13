using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The physical representation of a chunk column in-game
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class PhysicalChunkSection : MonoBehaviour
{
	/// <summary>
	/// Whether the initial generation of the chunk is complete
	/// </summary>
	public bool IsGenerated = false;

	public void SetMesh(Mesh mesh)
	{
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}
}
