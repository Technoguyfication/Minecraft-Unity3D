using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The local player
/// </summary>
public class PlayerController : EntityPlayer
{
	public const float DEFAULT_JUMP_HEIGHT = 1.25f;

	[Range(0, 100f)]
	public float MouseSensitivity = 15f;
	public float SneakSpeed = 1.31f;
	public float WalkSpeed = 4.17f;
	public float SprintSpeed = 5.612f;
	public float JumpHeight = DEFAULT_JUMP_HEIGHT;    // sometimes changes (potions, etc.)

	public delegate void OnGroundEventHandler(object sender, OnGroundEventArgs e);
	public event OnGroundEventHandler OnGroundChanged;
	private bool _wasOnGround = true;

	public override bool OnGround
	{
		get
		{
			// raycast from all four corners of hitbox to determine if player is on ground
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
		set => throw new InvalidOperationException("Cannot set OnGround for PlayerController!");
	}

	// Update is called once per frame
	new protected void Update()
	{
		Pitch -= Input.GetAxis("Mouse Y") * MouseSensitivity;
		Pitch = Mathf.Clamp(Pitch, HeadMinX, HeadMaxX);

		HeadYaw = Yaw += Input.GetAxis("Mouse X") * MouseSensitivity;

		// jumping
		if (Input.GetKey(KeyCode.Space) && OnGround)
		{
			Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * JumpHeight), Rigidbody.velocity.y);
		}

		base.Update();
	}

	protected void FixedUpdate()
	{
		// only simulate gravity on the player if the chunk we are in is loaded
		// stops player from falling through world when loading
		Rigidbody.useGravity = World?.ChunkRenderer.IsChunkSectionGenerated(BlockPos.GetChunkSectionPos()) ?? false || ForceGravityOn;

		Vector3 inputVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;    // get raw input from user

		// adjust velocity based on walking or sprinting
		if (Input.GetKey(KeyCode.LeftShift))
			inputVelocity *= SneakSpeed;
		else if (Input.GetKey(KeyCode.LeftControl))
			inputVelocity *= SprintSpeed;
		else
			inputVelocity *= WalkSpeed;

		// add in falling velocity
		inputVelocity.y = Rigidbody.velocity.y;

		// we need to adjust the input velocity by which direction the player is looking, so they walk in the direction they are looking
		Vector3 rotatedVelocity = Quaternion.Euler(0, Yaw, 0) * inputVelocity;

		// check if player ground status changed
		if (OnGround != _wasOnGround)
		{
			_wasOnGround = OnGround;
			OnGroundChanged?.Invoke(this, new OnGroundEventArgs() { OnGround = OnGround });
		}

		Rigidbody.velocity = rotatedVelocity;
	}
}

public class OnGroundEventArgs : EventArgs
{
	public bool OnGround { get; set; }
}
