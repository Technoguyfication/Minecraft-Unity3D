using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ChunkColumnPos
{
	public int X { get; }
	public int Z { get; }

	public ChunkColumnPos(int x, int z)
	{
		X = x;
		Z = z;
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
		ChunkColumnPos? pos = obj as ChunkColumnPos?;
		if (pos == null)
			return false;
		else
			return (((ChunkColumnPos)pos).X == X) && (((ChunkColumnPos)pos).Z == Z);
	}

	public static ChunkColumnPos operator +(ChunkColumnPos left, ChunkColumnPos right)
	{
		return new ChunkColumnPos(
			left.X + right.X,
			left.Z + right.Z);
	}
}
