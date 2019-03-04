using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A player in game
/// </summary>
public class Player : Entity
{
	public Texture Skin { get; set; }

	public Player()
	{
		Type = EntityType.PLAYER;
	}
}
