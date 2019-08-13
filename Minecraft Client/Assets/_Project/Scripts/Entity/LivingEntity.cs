using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an entity with a head
/// </summary>
public class LivingEntity : Entity
{
	public GameObject Head;
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

	private void SetHeadAndBodyAngles()
	{
		// subtract yaw from head yaw because head is parented to body
		Head.transform.localEulerAngles = new Vector3(HeadPitch, HeadYaw - Yaw, 0);
		Body.transform.localEulerAngles = new Vector3(0, Yaw, 0);
	}
}
