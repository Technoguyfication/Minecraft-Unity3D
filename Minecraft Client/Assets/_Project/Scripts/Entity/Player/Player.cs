using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A player in game
/// </summary>
public class Player : Entity
{
	public Texture Skin { get; set; }
	public GameObject Head;
	
	public Player()
	{
		Type = EntityType.PLAYER;
	}

	protected override void Update()
	{
		// set angle of player head
		Head.transform.localEulerAngles = new Vector3(Pitch, Yaw, 0);

		// simplify pitch degrees
		if (Pitch > 90f)
		{
			Pitch %= 90f;
			Pitch -= 90f;
		}
		else
		{
			Pitch %= 90f;
		}

		// simplify yaw degrees
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

		base.Update();
	}
}
