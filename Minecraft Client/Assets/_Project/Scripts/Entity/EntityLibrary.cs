using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityType = Entity.EntityType;

public class EntityLibrary : MonoBehaviour
{
	public GameObject DefaultEntity;
	[Space(10)]
	public GameObject Creeper;
	public GameObject Cow;
	public GameObject Pig;
	public GameObject Player;

	// todo: use reflection to generate this as a dictionary dynamically at runtime
	public GameObject GetEntityPrefab(EntityType type)
	{
		switch (type)
		{
			case EntityType.Creeper:
				return Creeper;
			case EntityType.Cow:
				return Cow;
			case EntityType.Pig:
				return Pig;
			case EntityType.Player:
				return Player;
			default:
				return DefaultEntity;
		}
	}
}
