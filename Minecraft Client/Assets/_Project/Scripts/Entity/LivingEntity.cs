using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an entity with a head
/// </summary>
public class LivingEntity : Entity
{
	public GameObject Head;

	protected readonly float HeadMinX = -89.9f;
	protected readonly float HeadMaxX = 89.9f;

	public float HeadPitch
	{
		get => Pitch;
		set
		{
			SetHeadAndBodyAngles();
			Pitch = value;
		}
	}

	public float HeadYaw
	{
		get => headYaw;
		set
		{
			SetHeadAndBodyAngles();
			headYaw = value;
		}
	}

	new public float Yaw
	{
		get => base.Yaw;
		set
		{
			SetHeadAndBodyAngles();
			base.Yaw = value;
		}
	}

	public GameObject Body;
	private float headYaw;

	/// <summary>
	/// Sets the angles of the head and body of the entity
	/// </summary>
	private void SetHeadAndBodyAngles()
	{
		// subtract yaw from head yaw because head is parented to body
		Head.transform.localEulerAngles = new Vector3(HeadPitch, 0, HeadYaw - Yaw);
		Body.transform.localEulerAngles = new Vector3(0, Yaw, 0);
	}
}
