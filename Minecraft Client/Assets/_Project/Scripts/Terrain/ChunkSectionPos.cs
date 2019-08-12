using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ChunkSectionPos
{
	public ChunkColumnPos ChunkColumnPos { get; set; }
	public int X => ChunkColumnPos.X;
	public int Y { get; }
	public int Z => ChunkColumnPos.Z;

	public ChunkSectionPos(int x, int y, int z)
	{
		ChunkColumnPos = new ChunkColumnPos(x, z);
		Y = y;
	}

	public ChunkSectionPos(ChunkColumnPos cPos, int y)
	{
		ChunkColumnPos = cPos;
		Y = y;
	}

	public override string ToString()
	{
		return $"{X}, {Z}";
	}

	public override int GetHashCode()
	{
		return $"{X} + {Z}".GetHashCode();
	}

	public override bool Equals(object obj)
	{
		ChunkSectionPos? pos = obj as ChunkSectionPos?;
		if (pos == null)
			return false;
		else
			return (((ChunkSectionPos)pos).X == X) && (((ChunkSectionPos)pos).Z == Z);
	}

	public static ChunkSectionPos operator +(ChunkSectionPos left, ChunkSectionPos right)
	{
		return new ChunkSectionPos(
			left.X + right.X,
			left.Y + right.Y,
			left.Z + right.Z);
	}
}
