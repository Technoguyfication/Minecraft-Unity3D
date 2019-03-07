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
		get
		{
			return Pitch;
		}
		set
		{
			Pitch = value;
		}
	}
	public float HeadYaw { get; set; }
	public GameObject Body;

    // Update is called once per frame
    protected override void Update()
    {
		Head.transform.localEulerAngles = new Vector3(HeadPitch, HeadYaw, 0);
		Body.transform.localEulerAngles = new Vector3(0, Yaw, 0);

		base.Update();
	}
}
