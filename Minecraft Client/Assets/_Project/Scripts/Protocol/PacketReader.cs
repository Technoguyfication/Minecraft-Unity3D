using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class PacketReader
{
    /// <summary>
    /// Reads from the given binary reader, and returns an int using the VarInt implementation.
    /// </summary>
    /// <param name="reader">The BinaryReader to use</param>
    /// <returns>The int version of the read VarInt</returns>
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

    public static void ReadString(in BinaryReader reader, out string result)
    {
        result = reader.ReadString();
    }

    public static void ReadGUID(in BinaryReader reader, out Guid result)
    {
        result = new Guid(reader.ReadBytes(16));
    }

    public static void ReadBoolean(in BinaryReader reader, out bool result)
    {
        result = reader.ReadBoolean();
    }
}
