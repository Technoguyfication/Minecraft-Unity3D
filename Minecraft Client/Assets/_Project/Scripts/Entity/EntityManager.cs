using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
	public EntityLibrary EntityLibrary;
	public World World;

	private readonly List<Entity> _entities = new List<Entity>();
	private const float ENTITY_ANGLE_COEFFICIENT = 360 / 256f;
	private const float ENTITY_ANGLE_OFFSET = 90f;

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
	public void EntityAbsoluteMove(int entityId, Vector3 absolutePos, bool onGround)
	{
		var entity = GetEntityByID(entityId);
		entity.MinecraftPosition = absolutePos;
		entity.OnGround = onGround;
	}

	/// <summary>
	/// Sets an entity's look angles for the body and head pitch only. For head yaw, use <see cref="EntityHeadYaw(int, float)"/>
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
	/// Sets the head yaw angle of the entity
	/// </summary>
	/// <param name="entityId"></param>
	/// <param name="headYaw"></param>
	public void EntityHeadYaw(int entityId, float headYaw)
	{
		LivingEntity entity = (LivingEntity)GetEntityByID(entityId);
		entity.HeadYaw = headYaw;
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

		var mob = Instantiate(EntityLibrary.GetEntity(pkt.Type), transform).GetComponent<Mob>();
		mob.Type = pkt.Type;
		mob.World = World;
		mob.UUID = pkt.UUID;
		mob.EntityID = pkt.EntityID;
		Vector3 pos = new Vector3((float)pkt.X, (float)pkt.Y, (float)pkt.Z); ;
		mob.MinecraftPosition = pos;
		mob.SetRotation(pkt.Pitch * ENTITY_ANGLE_COEFFICIENT, pkt.Yaw * ENTITY_ANGLE_COEFFICIENT + ENTITY_ANGLE_OFFSET);
		//mob.HeadPitch = pkt.HeadPitch * ENTITY_ANGLE_COEFFICIENT;
		mob.name = $"{mob.Type.ToString()} ID:{mob.EntityID} UUID:{mob.UUID.ToString()}";

		_entities.Add(mob);
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
			//Debug.LogWarning($"Server tried to send relative move for unloaded entity ID {pkt.EntityID}");
			return;
		}
	}

	public void HandleEntityLookAndRelativeMovePacket(EntityLookAndRelativeMovePacket pkt)
	{
		var deltaPos = ConvertRelativeMoveToVector(pkt.DeltaX, pkt.DeltaY, pkt.DeltaZ);

		try
		{
			EntityRelativeMove(pkt.EntityID, deltaPos, pkt.OnGround);
			EntityLook(pkt.EntityID, pkt.Pitch * ENTITY_ANGLE_COEFFICIENT, pkt.Yaw * ENTITY_ANGLE_COEFFICIENT + ENTITY_ANGLE_OFFSET);
		}
		catch (NullReferenceException)
		{
			//Debug.LogWarning($"Server tried to send look and relative move packet for unloaded entity ID {pkt.EntityID}");
			return;
		}
	}

	public void HandleEntityLook(EntityLookPacket pkt)
	{
		try
		{
			EntityLook(pkt.EntityID, pkt.Pitch * ENTITY_ANGLE_COEFFICIENT, pkt.Yaw * ENTITY_ANGLE_COEFFICIENT + ENTITY_ANGLE_OFFSET);
		}
		catch (NullReferenceException)
		{
			//Debug.LogWarning($"Server tried to send look packet for unloaded entity ID {pkt.EntityID}");
			return;
		}
	}

	public void HandleEntityHeadLook(EntityHeadLookPacket pkt)
	{
		try
		{
			EntityHeadYaw(pkt.EntityID, pkt.HeadYaw * ENTITY_ANGLE_COEFFICIENT + ENTITY_ANGLE_OFFSET);
		}
		catch (NullReferenceException)
		{
			//Debug.LogWarning($"Server tried to send head look packet for unloaded entity ID {pkt.EntityID}");
			return;
		}
	}

	private Vector3 ConvertRelativeMoveToVector(short deltaX, short deltaY, short deltaZ)
	{
		Vector3 delta = new Vector3(deltaX, deltaY, deltaZ) / 4096;
		return delta;
	}

	public void HandleEntityTeleport(EntityTeleportPacket pkt)
	{
		try
		{
			EntityLook(pkt.EntityID, pkt.Pitch * ENTITY_ANGLE_COEFFICIENT, pkt.Yaw * ENTITY_ANGLE_COEFFICIENT + ENTITY_ANGLE_OFFSET);
			EntityAbsoluteMove(pkt.EntityID, new Vector3((float)pkt.X, (float)pkt.Y, (float)pkt.Z), pkt.OnGround);
		}
		catch (NullReferenceException)
		{
			//Debug.LogWarning($"Server tried to send teleport packet for unloaded entity ID {pkt.EntityID}");
			return;
		}
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

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Notimplemented")]
	public void HandleEntityMetadataPacket(EntityMetadataPacket packet)
	{
		throw new NotImplementedException("Not yet implemented");
	}
}
