using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum BlockType : uint
{
	GENERIC_SOLID,

	AIR = 0,
	VOID_AIR = 8591,
	CAVE_AIR = 8592,
	STONE = 1,
	WATER = 34,
}
