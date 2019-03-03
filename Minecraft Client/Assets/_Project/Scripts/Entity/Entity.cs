using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a minecraft with physics and stuff
/// </summary>
public abstract class Entity : MonoBehaviour
{
	public GameObject Physical;

	protected readonly float CameraMinX = -89.9f;
	protected readonly float CameraMaxX = 89.9f;

	protected BoxCollider Collider;
	protected Rigidbody Rigidbody;

	/// <summary>
	/// Whether the player is touching the ground or not
	/// </summary>
	public bool OnGround
	{
		get
		{
			// raycast from all four corners of hitbox to determine if player on on ground
			// it's assumed the Physical object has the same X and Z scale
			float physicalWidth = Physical.transform.localScale.x / 2;
			Vector3[] castPoints = new Vector3[]
				{
					new Vector3(physicalWidth, 0, physicalWidth),
					new Vector3(physicalWidth, 0, -physicalWidth),
					new Vector3(-physicalWidth, 0, -physicalWidth),
					new Vector3(-physicalWidth, 0, physicalWidth),
				};

			// if any rays hit, we're on the ground
			foreach (var castPoint in castPoints)
			{
				if (Physics.Raycast(Physical.transform.position + castPoint, -Vector3.up, Collider.bounds.extents.y + 0.00001f))
					return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Gets a Vector3 representing the minecraft position of the player.
	/// </summary>
	public Vector3 MinecraftPosition
	{
		get
		{
			return new Vector3((float)Z, (float)FeetY, (float)X);
		}
		set
		{
			SetPosition(new Vector3(value.z, value.y, value.x));
		}
	}

	/// <summary>
	/// The Unity-style position of the player
	/// </summary>
	public Vector3 UnityPosition
	{
		get
		{
			return transform.position;
		}
		set
		{
			SetPosition(value);
		}
	}

	public double X { get { return transform.position.z; } }
	public double FeetY { get { return transform.position.y; } }
	public double Z { get { return transform.position.x; } }
	public float Pitch { get; set; } = 0f;
	public float Yaw { get; set; } = 0f;

	/// <summary>
	/// The block this player is in
	/// </summary>
	public BlockPos BlockPos
	{
		get
		{
			return new BlockPos()
			{
				X = (int)X - ((X < 0) ? 1 : 0),
				Y = (int)FeetY,
				Z = (int)Z - ((Z < 0) ? 1 : 0)
			};
		}
	}

	// Use this for initialization
	void Start()
	{
		Collider = Physical.GetComponent<BoxCollider>();
		Rigidbody = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
    {
		// simplify pitch
		if (Pitch > 90f)
		{
			Pitch %= 90f;
			Pitch -= 90f;
		}
		else
		{
			Pitch %= 90f;
		}

		// simplify yaw
		if (Yaw > 180f)
		{
			Yaw %= 360f;
			Yaw -= 360f;
		}
		else if (Yaw < -180f)
		{
			Yaw %= 360f;
			Yaw += 360f;
		}
		else
		{
			Yaw %= 360f;
		}
	}

	/// <summary>
	/// Sets the Unity position of the object
	/// </summary>
	/// <param name="position"></param>
	private void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	public void SetRotation(Quaternion rotation)
	{
		Pitch = rotation.eulerAngles.x;
		Yaw = rotation.eulerAngles.y;
	}
}
