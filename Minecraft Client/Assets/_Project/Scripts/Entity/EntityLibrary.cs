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

	public GameObject GetEntity(EntityType type)
	{
		switch (type)
		{
			case EntityType.CREEPER:
				return Creeper;
			case EntityType.COW:
				return Cow;
			case EntityType.PIG:
				return Pig;
			default:
				return DefaultEntity;
		}
	}
}
