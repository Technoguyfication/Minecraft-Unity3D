using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for mobs
/// </summary>
public abstract class Mob : LivingEntity
{
	// I'm assuming head pitch means pitch as far as we are concerned
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
}
