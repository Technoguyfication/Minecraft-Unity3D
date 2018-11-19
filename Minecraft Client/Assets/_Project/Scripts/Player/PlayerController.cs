using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[Range(0, 100f)]
	public float MouseSensitivity = 15f;
	public float SneakSpeed = 1.31f;
	public float WalkSpeed = 4.17f;
	public float SprintSpeed = 5.612f;
	public float JumpHeight = 1.25f;    // sometimes changes (potions, etc.)
	public GameObject Camera;
	private float _cameraMinX = -90f;
	private float _cameraMaxX = 90f;

	/// <summary>
	/// Whether the player is touching the ground or not
	/// </summary>
	public bool OnGround
	{
		get
		{
			// perform raycast to check if we are grounded
			return Physics.Raycast(transform.position, -Vector3.up, GetComponent<BoxCollider>().bounds.extents.y + 0.01f);
		}
	}

	public double X { get { return transform.position.z; } }
	public double FeetY { get { return transform.position.y; } }
	public double Z { get { return transform.position.x; } }
	public float Yaw { get; set; } = 0f;
	public float Pitch { get; set; } = 0f;

	// Use this for initialization
	void Start() {

	}

	public void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	public void SetRotation(Quaternion rotation)
	{
		Yaw = rotation.eulerAngles.x;
		Pitch = rotation.eulerAngles.y;
	}

	// Update is called once per frame
	void Update () {
		Yaw -= Input.GetAxis("Mouse Y") * MouseSensitivity;
		Yaw = Mathf.Clamp(Yaw, _cameraMinX, _cameraMaxX);

		Pitch += Input.GetAxis("Mouse X") * MouseSensitivity;

		Camera.transform.localEulerAngles = new Vector3(Yaw, Pitch, 0);

		// jumping
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Rigidbody rb = GetComponent<Rigidbody>();
			rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * 1.25f), rb.velocity.y);
		}
	}

	private void FixedUpdate()
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		Vector3 inputVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;    // get raw input from user

		// adjust velocity based on walking or sprinting
		if (Input.GetKey(KeyCode.LeftShift))
			inputVelocity *= SneakSpeed;
		else if (Input.GetKey(KeyCode.LeftControl))
			inputVelocity *= SprintSpeed;
		else
			inputVelocity *= WalkSpeed;

		// add in falling velocity
		inputVelocity.y = rb.velocity.y;

		// since the camera is the only part of the player that turns, we need to rotate the velocity vectors to the camera's looking position
		// so the user moves in the direction they are looking
		Vector3 rotatedVelocity = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0) * inputVelocity;

		rb.velocity = rotatedVelocity;
	}
}
