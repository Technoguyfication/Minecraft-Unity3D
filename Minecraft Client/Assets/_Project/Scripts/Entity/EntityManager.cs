using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
	public GameObject TestEntity;
	public World World;

	private readonly List<Entity> _entities = new List<Entity>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	/// <summary>
	/// Moves an entity relative to it's current position in Minecraft coordinate space
	/// </summary>
	/// <param name="entityId"></param>
	/// <param name="deltaPos"></param>
	public void EntityRelativeMove(int entityId, Vector3 deltaPos)
	{
		var entity = GetEntityByID(entityId);
		var newPos = entity.MinecraftPosition + deltaPos;
		entity.MinecraftPosition = newPos;
	}

	/// <summary>
	/// Moves an entity absolutely in minecraft coordinate space
	/// </summary>
	/// <param name="entityId"></param>
	/// <param name="absolutePos"></param>
	public void EntityAbsoluteMove(int entityId, Vector3 absolutePos)
	{
		GetEntityByID(entityId).MinecraftPosition = absolutePos;
	}

	/// <summary>
	/// Sets an entity's look angles
	/// </summary>
	/// <param name="entityId"></param>
	/// <param name="yaw"></param>
	/// <param name="pitch"></param>
	public void SetEntityLook(int entityId, float pitch, float yaw)
	{
		var entity = GetEntityByID(entityId);
		entity.SetRotation(pitch, yaw);
	}

	/// <summary>
	/// Spawns a mob in the server
	/// </summary>
	/// <param name="pkt"></param>
	public void HandleSpawnMobPacket(SpawnMobPacket pkt)
	{
		var mob = Instantiate(TestEntity, transform).GetComponent<Mob>();
		mob.Type = pkt.Type;
		mob.World = World;
		mob.UUID = pkt.UUID;
		mob.EntityID = pkt.EntityID;
		Vector3 pos = new Vector3((float)pkt.X, (float)pkt.Y, (float)pkt.Z); ;
		mob.MinecraftPosition = pos;
		mob.SetRotation(pkt.Pitch, pkt.Yaw);
		mob.HeadPitch = pkt.HeadPitch;
		mob.name = $"{mob.Type.ToString()} ID:{mob.EntityID} UUID:{mob.UUID.ToString()}";

		_entities.Add(mob);

#if DEBUG
		Debug.Log($"Spawned entity at {pos}");
#endif
	}

	public void HandleEntityRelativeMovePacket(EntityRelativeMovePacket pkt)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Destroys entities on the client
	/// </summary>
	public void DestroyEntities(int[] entityIds)
	{
		foreach (int id in entityIds)
		{
			DestroyEntity(id);
		}
	}

	/// <summary>
	/// Destroys an entity on the client
	/// </summary>
	/// <param name="entityId"></param>
	public void DestroyEntity(int entityId)
	{
		var entity = GetEntityByID(entityId);
		if (entity == null)
			return;

		_entities.Remove(entity);
		Destroy(entity.gameObject);

#if DEBUG
		Debug.Log($"Destroyed entity ID {entityId}");
#endif
	}

	/// <summary>
	/// Gets an entity from it's entity ID
	/// </summary>
	/// <param name="entityId"></param>
	/// <returns></returns>
	public Entity GetEntityByID(int entityId)
	{
		return _entities.Find(e => e.EntityID == entityId);
	}

	/// <summary>
	/// Destroys all client entities
	/// </summary>
	public void DestroyAllEntities()
	{
		_entities.ForEach(e => Destroy(e));
		_entities.Clear();
	}
}
