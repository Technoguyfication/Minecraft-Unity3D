﻿using System;
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
	public void EntityRelativeMove(int entityId, Vector3 deltaPos, bool onGround)
	{
		var entity = GetEntityByID(entityId);
		var newPos = entity.MinecraftPosition + deltaPos;
		entity.MinecraftPosition = newPos;
		entity.OnGround = onGround;
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
	public void EntityLook(int entityId, float pitch, float yaw)
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
		if (GetEntityByID(pkt.EntityID) != null)
		{
			Debug.LogWarning($"Server tried to spawn {pkt.Type.ToString()} with entity ID {pkt.EntityID} but it is already loaded on client.\nExisting entity: {GetEntityByID(pkt.EntityID)}");
			return;
		}

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
		var deltaPos = ConvertRelativeMoveToVector(pkt.DeltaX, pkt.DeltaY, pkt.DeltaZ);
		try
		{
			EntityRelativeMove(pkt.EntityID, deltaPos, pkt.OnGround);
		}
		catch (NullReferenceException)
		{
			Debug.LogWarning($"Server tried to send relative move for unloaded entity ID {pkt.EntityID}");
			return;
		}
	}

	public void HandleEntityLookAndRelativeMovePacket(EntityLookAndRelativeMovePacket pkt)
	{
		var deltaPos = ConvertRelativeMoveToVector(pkt.DeltaZ, pkt.DeltaY, pkt.DeltaX);

		try
		{
			EntityRelativeMove(pkt.EntityID, deltaPos, pkt.OnGround);
			EntityLook(pkt.EntityID, pkt.Pitch / 256f, pkt.Yaw / 256f);
		}
		catch (NullReferenceException)
		{
			Debug.LogWarning($"Server tried to send look and relative move packet for unloaded entity ID {pkt.EntityID}");
			return;
		}
	}

	public void HandleEntityLook(EntityLookPacket pkt)
	{
		try
		{
			EntityLook(pkt.EntityID, pkt.Pitch / 256f, pkt.Yaw / 256f);
		}
		catch (NullReferenceException)
		{
			Debug.LogWarning($"Server tried to send look packet for unloaded entity ID {pkt.EntityID}");
		}
	}

	private Vector3 ConvertRelativeMoveToVector(short deltaX, short deltaY, short deltaZ)
	{
		Vector3 delta = new Vector3(deltaX, deltaY, deltaZ) / 4096;
		return delta;
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