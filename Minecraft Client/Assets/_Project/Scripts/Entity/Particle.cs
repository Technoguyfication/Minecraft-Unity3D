using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Particle
{
	public ParticleType UsedParticleType { get; }

	public int BlockState { get; }

	public float Red { get; }
	public float Green { get; }
	public float Blue { get; }
	public float Scale { get; }

	public SlotData slotData;

	public Particle(BinaryReader reader)
	{
		int particleType = PacketReader.ReadVarInt(reader);
		UsedParticleType = (ParticleType)particleType;

		BlockState = 0;

		Red = 0;
		Green = 0;
		Blue = 0;
		Scale = 0;

		slotData = new SlotData();

		switch (UsedParticleType)
		{
			case ParticleType.MinecraftBlock:
			case ParticleType.MinecraftFallingDust:
				BlockState = PacketReader.ReadVarInt(reader);
				break;
			case ParticleType.MinecraftDust:
				Red = PacketReader.ReadSingle(reader);
				Green = PacketReader.ReadSingle(reader);
				Blue = PacketReader.ReadSingle(reader);
				Scale = PacketReader.ReadSingle(reader);
				break;
			case ParticleType.MinecraftItem:
				//ReadSlotData(reader, out slotData);
				throw new NotImplementedException("Particle: SlotData is not yet handled");
				//break;
		}
	}

	public enum ParticleType
	{
		MinecraftAmbientEntityEffect = 0,
		MinecraftAngryVillager = 1,
		MinecraftBarrier = 2,
		MinecraftBlock = 3, // Uses blockState
		MinecraftBubble = 4,
		MinecraftCloud = 5,
		MinecraftCrit = 6,
		MinecraftDamageIndicator = 7,
		MinecraftDragonBreath = 8,
		MinecraftDrippingLava = 9,
		MinecraftDrippingWater = 10,
		MinecraftDust = 11, // Uses red, green, blue, and scale
		MinecraftEffect = 12,
		MinecraftElderGuardian = 13,
		MinecraftEnchantedHit = 14,
		MinecraftEnchant = 15,
		MinecraftEndRod = 16,
		MinecraftEntityEffect = 17,
		MinecraftExplosionEmitter = 18,
		MinecraftExplosion = 19,
		MinecraftFallingDust = 20, // Uses blockState
		MinecraftFirework = 21,
		MinecraftFishing = 22,
		MinecraftFlame = 23,
		MinecraftHappyVillager = 24,
		MinecraftHeart = 25,
		MinecraftInstantEffect = 26,
		MinecraftItem = 27, // Uses slotData
		MinecraftItemSlime = 28,
		MinecraftItemSnowball = 29,
		MinecraftLargeSmoke = 30,
		MinecraftLava = 31,
		MinecraftMycelium = 32,
		MinecraftNote = 33,
		MinecraftPoof = 34,
		MinecraftPortal = 35,
		MinecraftRain = 36,
		MinecraftSmoke = 37,
		MinecraftSpit = 38,
		MinecraftSquidInk = 39,
		MinecraftSweepAttack = 40,
		MinecraftTotemOfUndying = 41,
		MinecraftUnderwater = 42,
		MinecraftSplash = 43,
		MinecraftWitch = 44,
		MinecraftBubblePop = 45,
		MinecraftCurrentDown = 46,
		MinecraftBubbleColumnUp = 47,
		MinecraftNautilus = 48,
		MinecraftDolphin = 49
	}
}
