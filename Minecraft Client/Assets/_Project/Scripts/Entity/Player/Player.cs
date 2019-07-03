using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A player in game
/// </summary>
public class Player : LivingEntity
{
	public Texture Skin { get; set; }

	public Player()
	{
		Type = EntityType.PLAYER;
	}

	protected override void Update()
	{
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
