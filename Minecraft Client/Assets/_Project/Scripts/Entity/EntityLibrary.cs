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

	// todo: use reflection to generate this as a dictionary dynamically at runtime
	public GameObject GetEntity(EntityType type)
	{
		switch (type)
		{
			case EntityType.Creeper:
				return Creeper;
			case EntityType.Cow:
				return Cow;
			case EntityType.Pig:
				return Pig;
			default:
				return DefaultEntity;
		}
	}
}
