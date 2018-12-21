using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	[Range(0, 100f)]
	public float MouseSensitivity = 15f;
	public float SneakSpeed = 1.31f;
	public float WalkSpeed = 4.17f;
	public float SprintSpeed = 5.612f;
	public float JumpHeight = 1.25f;    // sometimes changes (potions, etc.)
	public GameObject Camera;
	public GameObject Physical;
	public bool UseGravity = true;

	public delegate void OnGroundEventHandler(object sender, OnGroundEventArgs e);
	public event OnGroundEventHandler OnGroundChanged;

	private readonly float _cameraMinX = -89.9f;
	private readonly float _cameraMaxX = 89.9f;

	private BoxCollider _collider;
	private Rigidbody _rigidbody;
	private bool _wasOnGround = true;

	/// <summary>
	/// Whether the player is touching the ground or not
	/// </summary>
	public bool OnGround
	{
		get
		{
			// raycast from all four corners of hitbox to determine if player on on ground
			// it's assumed the Physical object has the same X and Z scale
			float physicalWidth = Physical.transform.localScale.x;
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
				if (Physics.Raycast(Physical.transform.position + castPoint, -Vector3.up, _collider.bounds.extents.y + 0.01f))
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
		_collider = Physical.GetComponent<BoxCollider>();
		_rigidbody = GetComponent<Rigidbody>();
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

	// Update is called once per frame
	void Update()
	{
		if (Pitch > 90f)
		{
			Pitch %= 90f;
			Pitch -= 90f;
		}
		else
		{
			Pitch %= 90f;
		}

		Pitch -= Input.GetAxis("Mouse Y") * MouseSensitivity;
		Pitch = Mathf.Clamp(Pitch, _cameraMinX, _cameraMaxX);

		Yaw += Input.GetAxis("Mouse X") * MouseSensitivity;

		Camera.transform.localEulerAngles = new Vector3(Pitch, Yaw, 0);

		// jumping
		if (Input.GetKey(KeyCode.Space) && OnGround)
		{
			_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * JumpHeight), _rigidbody.velocity.y);
		}
	}

	private void FixedUpdate()
	{
		Vector3 inputVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;    // get raw input from user

		// adjust velocity based on walking or sprinting
		if (Input.GetKey(KeyCode.LeftShift))
			inputVelocity *= SneakSpeed;
		else if (Input.GetKey(KeyCode.LeftControl))
			inputVelocity *= SprintSpeed;
		else
			inputVelocity *= WalkSpeed;

		// add in falling velocity
		inputVelocity.y = _rigidbody.velocity.y;

		// since the camera is the only part of the player that turns, we need to rotate the velocity vectors to the camera's looking position
		// so the user moves in the direction they are looking
		Vector3 rotatedVelocity = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0) * inputVelocity;

		// check if player ground status changed
		if (OnGround != _wasOnGround)
		{
			_wasOnGround = OnGround;
			OnGroundChanged?.Invoke(this, new OnGroundEventArgs() { OnGround = OnGround });
		}

		_rigidbody.useGravity = UseGravity;
		_rigidbody.velocity = rotatedVelocity;
	}
}

public class OnGroundEventArgs : EventArgs
{
	public bool OnGround { get; set; }
}
