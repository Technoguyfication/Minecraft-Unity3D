using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ChunkColumnPos
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
		ChunkColumnPos? pos = obj as ChunkColumnPos?;
		if (pos == null)
			return false;
		else
			return (((ChunkColumnPos)pos).X == X) && (((ChunkColumnPos)pos).Z == Z);
	}

	public static ChunkColumnPos operator +(ChunkColumnPos left, ChunkColumnPos right)
	{
		return new ChunkColumnPos()
		{
			X = left.X + right.X,
			Z = left.Z + right.Z
		};
	}
}
