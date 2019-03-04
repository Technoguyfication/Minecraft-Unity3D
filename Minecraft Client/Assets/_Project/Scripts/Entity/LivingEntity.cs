using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an entity with a head
/// </summary>
public class LivingEntity : Entity
{
	public GameObject Head;

    // Update is called once per frame
    protected override void Update()
    {
		// set angle of player head
		Head.transform.localEulerAngles = new Vector3(Pitch, Yaw, 0);

		base.Update();
	}
}
