using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a minecraft with physics and stuff
/// </summary>
public abstract class Entity : MonoBehaviour
{
	public GameObject Physical;
	public abstract int ID { get; set; }

	protected readonly float CameraMinX = -89.9f;
	protected readonly float CameraMaxX = 89.9f;

	protected BoxCollider Collider;
	protected Rigidbody Rigidbody;

	/// <summary>
	/// Whether the player is touching the ground or not
	/// </summary>
	public bool OnGround
	{
		get
		{
			// raycast from all four corners of hitbox to determine if player on on ground
			// it's assumed the Physical object has the same X and Z scale
			float physicalWidth = Physical.transform.localScale.x / 2;
			Vector3[] castPoints = new Vector3[]
				{
					new Vector3(physicalWidth, 0, physicalWidth),
					new Vector3(physicalWidth, 0, -physicalWidth),
					new Vector3(-physicalWidth, 0, -physicalWidth),
					new Vector3(-physicalWidth, 0, physicalWidth),
				};

			// if any rays hit, we're on the ground
			foreach (var castPoint in castPoints)
			{
				if (Physics.Raycast(Physical.transform.position + castPoint, -Vector3.up, Collider.bounds.extents.y + 0.00001f))
					return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Gets a Vector3 representing the minecraft position of the player.
	/// </summary>
	public Vector3 MinecraftPosition
	{
		get
		{
			return new Vector3((float)Z, (float)FeetY, (float)X);
		}
		set
		{
			SetPosition(new Vector3(value.z, value.y, value.x));
		}
	}

	/// <summary>
	/// The Unity-style position of the player
	/// </summary>
	public Vector3 UnityPosition
	{
		get
		{
			return transform.position;
		}
		set
		{
			SetPosition(value);
		}
	}

	public double X { get { return transform.position.z; } }
	public double FeetY { get { return transform.position.y; } }
	public double Z { get { return transform.position.x; } }
	public float Pitch { get; set; } = 0f;
	public float Yaw { get; set; } = 0f;

	/// <summary>
	/// The block this player is in
	/// </summary>
	public BlockPos BlockPos
	{
		get
		{
			return new BlockPos()
			{
				X = (int)X - ((X < 0) ? 1 : 0),
				Y = (int)FeetY,
				Z = (int)Z - ((Z < 0) ? 1 : 0)
			};
		}
	}

	// Use this for initialization
	void Start()
	{
		Collider = Physical.GetComponent<BoxCollider>();
		Rigidbody = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
    {
		// simplify pitch
		if (Pitch > 90f)
		{
			Pitch %= 90f;
			Pitch -= 90f;
		}
		else
		{
			Pitch %= 90f;
		}

		// simplify yaw
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
	}

	/// <summary>
	/// Sets the Unity position of the object
	/// </summary>
	/// <param name="position"></param>
	private void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	public void SetRotation(Quaternion rotation)
	{
		Pitch = rotation.eulerAngles.x;
		Yaw = rotation.eulerAngles.y;
	}

	public override int GetHashCode()
	{
		return ID;
	}

	public override bool Equals(object other)
	{
		Entity _other = other as Entity;
		if (_other == null)
			return false;
		else
			return _other.GetHashCode().Equals(GetHashCode());
	}

	public enum Type
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
		SALMON = 58,
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
