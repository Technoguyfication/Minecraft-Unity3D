using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Position
{
    public int X;
    public int Y;
    public int Z;

    public Position(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Position(in BinaryReader reader)
    {
        ulong val = reader.ReadUInt64();

        X = (int)val >> 38;
        Y = (int)(val >> 26) & 0xFFF;
        Z = (int)val << 38 >> 38;
    }
}
