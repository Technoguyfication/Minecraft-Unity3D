using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a Minecraft Entity
/// https://minecraft.gamepedia.com/Entity
/// </summary>
public abstract class Entity : MonoBehaviour
{
	public GameObject Physical;
	public int EntityID { get; set; }
	public virtual Guid UUID { get; set; }
	public EntityType Type { get; set; }
	public World World { get; set; }

	/// <summary>
	/// Force gravity to be enabled for this entity even if the world has not loaded yet
	/// </summary>
	public bool ForceGravityOn { get; set; } = false;

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
	/// The rotation of the entity. For living entities, this is the rotation of the body.
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
	/// The block this entity is in
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

	/// <summary>
	/// All types of entities in Minecraft
	/// </summary>
	public enum EntityType
	{
		// the following are spawned using spawn mob packet
		Bat = 3,
		Blaze = 4,
		CaveSpider = 6,
		Chicken = 7,
		Cod = 8,
		Cow = 9,
		Creeper = 10,
		Donkey = 11,
		Dolphin = 12,
		Drowned = 14,
		ElderGuardian = 15,
		EnderDragon = 17,
		Enderman = 18,
		Endermite = 19,
		EvocationVillager = 21,
		Ghast = 26,
		Giant = 27,
		Guardian = 28,
		Horse = 29,
		Husk = 30,
		IllusionVillager = 31,
		Llama = 36,
		MagmaSlime = 38,
		Mule = 46,
		Mooshroom = 47,
		Ocelot = 48,
		Parrot = 50,
		Pig = 51,
		Pufferfish = 52,
		Pigman = 53,
		PolarBear = 54,
		Rabbit = 56,
		Salmon = 57,
		Sheep = 58,
		Shulker = 59,
		Silverfish = 61,
		Skeleton = 62,
		SkeletonHorse = 63,
		Slime = 64,
		Snowman = 66,
		Spider = 69,
		Squid = 70,
		Stray = 71,
		TropicalFish = 72,
		Turtle = 73,
		Vex = 78,
		Villager = 79,
		IronGolem = 80,
		VindicationVillager = 81,
		Witch = 82,
		Wither = 83,
		WitherSkeleton = 84,
		Wolf = 86,
		Zombie = 87,
		ZombieHorse = 88,
		ZombieVillager = 89,
		Phantom = 90,

		// The following are spawned using spawn object
		AreaEffectCloud = 0,
		ArmorStand = 1,
		Arrow = 3,
		Boat = 5,
		DragonFireball = 13,   // dragon
		EndCrystal = 16,
		EvocationFangs = 20,
		EyeOfEnderSignal = 23,
		FallingSand = 24,
		FireworkRocket = 25,
		Item = 32,
		ItemFrame = 33,
		Fireball = 34,  // ghast
		LeashKnot = 35,
		LlamaSpit = 37,
		MinecartRideable = 39,
		MinecartChest = 40,
		MinecartCommandBlock = 41,
		MinecartFurnace = 42,
		MinecartHopper = 43,
		MinecartSpawner = 44,
		MinecartTnt = 45,
		PrimedTnt = 55,
		ShulkerBullet = 60,
		SmallFireball = 65,    // blaze
		Snowball = 67,
		SpectralArrow = 68,
		ThrownEgg = 74,
		ThrownEnderPearl = 75,
		ThrownExpBottle = 76,
		ThrownPotion = 77,
		WitherSkull = 85,
		FishingBobber = 93,
		Trident = 94,

		// the following have special packet to be spawned
		ExpOrb = 22,
		Painting = 49,
		LightingBolt = 91,
		Player = 92
	}
}
