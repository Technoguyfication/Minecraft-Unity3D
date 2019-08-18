using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a minecraft with physics and stuff
/// </summary>
public abstract class Entity : MonoBehaviour
{
	public GameObject Physical;
	public int EntityID { get; set; }
	public Guid UUID { get; set; }
	public EntityType Type { get; set; }
	public World World { get; set; }

	public bool ForceGravityOn = false;

	protected readonly float HeadMinX = -89.9f;
	protected readonly float HeadMaxX = 89.9f;

	protected BoxCollider Collider;
	protected Rigidbody Rigidbody;

	/// <summary>
	/// Whether the entity is touching the ground or not.
	/// </summary>
	public virtual bool OnGround { get; set; }

	/// <summary>
	/// Gets a Vector3 representing the minecraft position of the entity.
	/// </summary>
	public Vector3 MinecraftPosition
	{
		get => new Vector3((float)X, (float)Y, (float)Z);
		set => transform.position = new Vector3(value.z, value.y, value.x);
	}

	/// <summary>
	/// The Unity-style position of the entity
	/// </summary>
	public Vector3 UnityPosition
	{
		get => transform.position;
		set => transform.position = value;
	}

	/// <summary>
	/// X coordinate in Minecraft space
	/// </summary>
	public double X => transform.position.z;
	/// <summary>
	/// Y coordinate in Minecraft space
	/// </summary>
	public double Y => transform.position.y;
	/// <summary>
	/// Z coordinate in Minecraft space
	/// </summary>
	public double Z => transform.position.x;
	public float Pitch { get; set; } = 0f;
	public float Yaw { get; set; } = 0f;
	/// <summary>
	/// The rotation of the entity. (Usually the head rotation)
	/// </summary>
	public Quaternion EntityRotation
	{
		get => Quaternion.Euler(Pitch, Yaw, 0);
		set
		{
			Pitch = value.eulerAngles.x;
			Yaw = value.eulerAngles.y;
		}
	}
	public short VelocityX { get; set; }
	public short VelocityY { get; set; }
	public short VelocityZ { get; set; }

	/// <summary>
	/// The block this player is in
	/// </summary>
	public BlockPos BlockPos => new BlockPos()
	{
		X = (int)X - ((X < 0) ? 1 : 0),
		Y = (int)Y,
		Z = (int)Z - ((Z < 0) ? 1 : 0)
	};

	// Use this for initialization
	protected virtual void Start()
	{
		Collider = Physical.GetComponent<BoxCollider>();
		Rigidbody = GetComponent<Rigidbody>();

		Rigidbody.useGravity = ForceGravityOn;   // don't use gravity by default for entities
	}

	public void SetRotation(float pitch, float yaw)
	{
		Pitch = pitch;
		Yaw = yaw;
	}

	public override int GetHashCode()
	{
		return EntityID;
	}

	public override bool Equals(object other)
	{
		Entity _other = other as Entity;
		if (_other == null)
			return false;
		else
			return _other.GetHashCode().Equals(GetHashCode());
	}

	public override string ToString()
	{
		return $"{Type.ToString()}:{EntityID}";
	}


	public enum EntityType
	{
		// the following are spawned using spawn mob packet
		BAT = 3,
		BLAZE = 4,
		CAVE_SPIDER = 6,
		CHICKEN = 7,
		COD = 8,
		COW = 9,
		CREEPER = 10,
		DONKEY = 11,
		DOLPHIN = 12,
		DROWNED = 14,
		ELDER_GUARDIAN = 15,
		ENDER_DRAGON = 17,
		ENDERMAN = 18,
		ENDERMITE = 19,
		EVOCATION_VILLAGER = 21,
		GHAST = 26,
		GIANT = 27,
		GUARDIAN = 28,
		HORSE = 29,
		HUSK = 30,
		ILLUSION_ILLAGER = 31,
		LLAMA = 36,
		LAVA_SLIME = 38,
		MULE = 46,
		MUSHROOM_COW = 47,
		OZELOT = 48,
		PARROT = 50,
		PIG = 51,
		PUFFERFISH = 52,
		PIG_ZOMBIE = 53,
		POLAR_BEAR = 54,
		RABBIT = 56,
		SALMON = 57,
		SHEEP = 58,
		SHULKER = 59,
		SILVERFISH = 61,
		SKELETON = 62,
		SKELETON_HORSE = 63,
		SLIME = 64,
		SNOWMAN = 66,
		SPIDER = 69,
		SQUID = 70,
		STRAY = 71,
		TROPICAL_FISH = 72,
		TURTLE = 73,
		VEX = 78,
		VILLAGER = 79,
		IRON_GOLEM = 80,
		VINDICATION_ILLAGER = 81,
		WITCH = 82,
		WITHER = 83,
		WITHER_SKELETON = 84,
		WOLF = 86,
		ZOMBIE = 87,
		ZOMBIE_HORSE = 88,
		ZOMBIE_VILLAGER = 89,
		PHANTOM = 90,

		// The following are spawned using spawn object
		AREA_EFFECT_CLOUD = 0,
		ARMOR_STAND = 1,
		ARROW = 3,
		BOAT = 5,
		DRAGON_FIREBALL = 13,   // dragon
		END_CRYSTAL = 16,
		EVOCATION_FANGS = 20,
		EYE_OF_ENDER_SIGNAL = 23,
		FALLING_SAND = 24,
		FIREWORK_ROCKET = 25,
		ITEM = 32,
		ITEM_FRAME = 33,
		FIREBALL = 34,  // ghast
		LEASH_KNOT = 35,
		LLAMA_SPIT = 37,
		MINECART_RIDEABLE = 39,
		MINECART_CHEST = 40,
		MINECART_COMMAND_BLOCK = 41,
		MINECART_FURNACE = 42,
		MINECART_HOPPER = 43,
		MINECART_SPAWNER = 44,
		MINECART_TNT = 45,
		PRIMED_TNT = 55,
		SHULKER_BULLET = 60,
		SMALL_FIREBALL = 65,    // blaze
		SNOWBALL = 67,
		SPECTRAL_ARROW = 68,
		THROWN_EGG = 74,
		THROWN_ENDER_PEARL = 75,
		THROWN_EXP_BOTTLE = 76,
		THROWN_POTION = 77,
		WITHER_SKULL = 85,
		FISHING_BOBBER = 93,
		TRIDENT = 94,

		// the following have special packet to be spawned
		EXP_ORB = 22,
		PAINTING = 49,
		LIGHTNING_BOLT = 91,
		PLAYER = 92
	}
}
