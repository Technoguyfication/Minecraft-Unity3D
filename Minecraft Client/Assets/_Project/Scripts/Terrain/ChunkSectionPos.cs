using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ChunkSectionPos
{
	public ChunkColumnPos ChunkColumnPos { get; set; }
	public int X => ChunkColumnPos.X;
	public int Y { get; set; }
	public int Z => ChunkColumnPos.Z;

	public override string ToString()
	{
		return $"{X}, {Y}, {Z}";
	}

	public override int GetHashCode()
	{
		return $"{X} + {Y} + {Z}".GetHashCode();
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
		return new ChunkSectionPos()
		{
			ChunkColumnPos = left.ChunkColumnPos + right.ChunkColumnPos,
			Y = left.Y + right.Y
		};
	}
}
