using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ChunkPos
{
	public int X { get; set; }
	public int Z { get; set; }

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
		ChunkPos? pos = obj as ChunkPos?;
		if (pos == null)
			return false;
		else
			return (((ChunkPos)pos).X == X) && (((ChunkPos)pos).Z == Z);
	}

	public static ChunkPos operator +(ChunkPos left, ChunkPos right)
	{
		return new ChunkPos()
		{
			X = left.X + right.X,
			Z = left.Z + right.Z
		};
	}
}
