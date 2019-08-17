using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class PacketReader
{
    /// <summary>
    /// Reads a VarInt from the given reader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The int representation of the VarInt</param>
    public static void ReadVarInt(in BinaryReader reader, out int result)
    {
        int value = 0, numRead = 0;
        result = 0;
        byte read;
        while (true)
        {
            read = reader.ReadByte();
            value = (read & 0x7F);
            result |= (value << (7 * numRead));

            numRead++;
            if (numRead > 5)
                throw new UnityException("VarInt too big!");
            if ((read & 0x80) != 128) break;
        }
    }

    /// <summary>
    /// Reads a string from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The read string</param>
    public static void ReadString(in BinaryReader reader, out string result)
    {
        result = reader.ReadString();
    }

    /// <summary>
    /// Reads a GUID from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The read GUID</param>
    public static void ReadGUID(in BinaryReader reader, out Guid result)
    {
        result = new Guid(reader.ReadBytes(16));
    }

    /// <summary>
    /// Reads a boolean from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The read boolean</param>
    public static void ReadBoolean(in BinaryReader reader, out bool result)
    {
        result = reader.ReadBoolean();
    }

    /// <summary>
    /// Reads a float from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The read float</param>
    public static void ReadFloat(in BinaryReader reader, out float result)
    {
        result = reader.ReadSingle();
    }

    /// <summary>
    /// Reads a byte from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The read byte</param>
    public static void ReadByte(in BinaryReader reader, out byte result)
    {
        result = reader.ReadByte();
    }

    /// <summary>
    /// Reads a Position from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="result">The read position (3 ints)</param>
    public static void ReadPosition(in BinaryReader reader, out Position result)
    {
        ulong val = reader.ReadUInt64();
        int x = (int)val >> 38;
        int y = (int)(val >> 26) & 0xFFF;
        int z = (int)val << 38 >> 38;

        result = new Position(x, y, z);
    }

    /// <summary>
    /// Reads a rotation from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="rotation">The read rotation (3 floats as Vector3)</param>
    public static void ReadRotation(in BinaryReader reader, out Vector3 rotation)
    {
        ReadFloat(reader, out float x);
        ReadFloat(reader, out float y);
        ReadFloat(reader, out float z);

        rotation = new Vector3(x, y, z);
    }

    /// <summary>
    /// Reads NBT data from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="nbtData">The read NBT data (complex)</param>
    /*
    public static void ReadNbtData(in BinaryReader reader, out NbtCompound nbtData)
    {
        throw new NotImplementedException("NBT Data is not yet supported");
    }*/

    /// <summary>
    /// Reads SlotData from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="slotData">The read slot data (inventory slot)</param>
    /*
    public static void ReadSlotData(in BinaryReader reader, out SlotData slotData)
    {
        ReadBoolean(reader, out bool present);
        if (!present)
            slotData = new SlotData(false, 0, 0, null);
        else
        {
            ReadVarInt(reader, out int itemID);
            ReadByte(reader, out byte count);
            ReadNbtData(reader, out NbtCompound nbt);
            slotData = new SlotData(true, itemID, count, nbt);
        }
    }*/
    
    /// <summary>
    /// Reads a Particle from the given BinaryReader
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <param name="particle">The read particle (type and extra data)</param>
    public static void ReadParticle(in BinaryReader reader, out Particle particle)
    {
        ReadVarInt(reader, out int particleType);
        Particle.ParticleType type = (Particle.ParticleType)particleType;

        int blockState = 0;

        float red = 0;
        float green = 0;
        float blue = 0;
        float scale = 0;

        SlotData slotData = new SlotData();

        switch (type)
        {
            case Particle.ParticleType.minecraft_block:
            case Particle.ParticleType.minecraft_falling_dust:
                ReadVarInt(reader, out blockState);
                break;
            case Particle.ParticleType.minecraft_dust:
                ReadFloat(reader, out red);
                ReadFloat(reader, out green);
                ReadFloat(reader, out blue);
                ReadFloat(reader, out scale);
                break;
            case Particle.ParticleType.minecraft_item:
                //ReadSlotData(reader, out slotData);
                break;
        }

        particle = new Particle(type, blockState, red, green, blue, scale, slotData);
    }
}
