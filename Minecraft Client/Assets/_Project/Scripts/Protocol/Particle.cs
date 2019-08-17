using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Particle
{
    public ParticleType type;

    public int blockState;

    public float red;
    public float green;
    public float blue;
    public float scale;

    public SlotData slotData;

    public Particle(ParticleType type, int blockState, float red, float green, float blue, float scale, SlotData slotData)
    {
        this.type = type;
        this.blockState = blockState;

        this.red = red;
        this.green = green;
        this.blue = blue;
        this.scale = scale;

        this.slotData = slotData;
    }

    public enum ParticleType
    {
        minecraft_ambient_entity_effect = 0,
        minecraft_angry_villager = 1,
        minecraft_barrier = 2,
        minecraft_block = 3, // Uses blockState
        minecraft_bubble = 4,
        minecraft_cloud = 5,
        minecraft_crit = 6,
        minecraft_damage_indicator = 7,
        minecraft_dragon_breath = 8,
        minecraft_dripping_lava = 9,
        minecraft_dripping_water = 10,
        minecraft_dust = 11, // Uses red, green, blue, and scale
        minecraft_effect = 12,
        minecraft_elder_guardian = 13,
        minecraft_enchanted_hit = 14,
        minecraft_enchant = 15,
        minecraft_end_rod = 16,
        minecraft_entity_effect = 17,
        minecraft_explosion_emitter = 18,
        minecraft_explosion = 19,
        minecraft_falling_dust = 20, // Uses blockState
        minecraft_firework = 21,
        minecraft_fishing = 22,
        minecraft_flame = 23,
        minecraft_happy_villager = 24,
        minecraft_heart = 25,
        minecraft_instant_effect = 26,
        minecraft_item = 27, // Uses slotData
        minecraft_item_slime = 28,
        minecraft_item_snowball = 29,
        minecraft_large_smoke = 30,
        minecraft_lava = 31,
        minecraft_mycelium = 32,
        minecraft_note = 33,
        minecraft_poof = 34,
        minecraft_portal = 35,
        minecraft_rain = 36,
        minecraft_smoke = 37,
        minecraft_spit = 38,
        minecraft_squid_ink = 39,
        minecraft_sweep_attack = 40,
        minecraft_totem_of_undying = 41,
        minecraft_underwater = 42,
        minecraft_splash = 43,
        minecraft_witch = 44,
        minecraft_bubble_pop = 45,
        minecraft_current_down = 46,
        minecraft_bubble_column_up = 47,
        minecraft_nautilus = 48,
        minecraft_dolphin = 49
    }
}
