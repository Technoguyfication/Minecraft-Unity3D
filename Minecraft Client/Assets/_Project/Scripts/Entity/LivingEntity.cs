using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an entity with a head
/// </summary>
public class LivingEntity : Entity
{
	public GameObject Head;
	public GameObject Body;

    // Update is called once per frame
    protected override void Update()
    {
		// set angle of player head
		// todo: make the head have some degree of freedom before body moves like vanilla game
		Head.transform.localEulerAngles = new Vector3(Pitch, Yaw, 0);
		Body.transform.localEulerAngles = new Vector3(0, Yaw, 0);

		base.Update();
	}
}
