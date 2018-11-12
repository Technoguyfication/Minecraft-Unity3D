using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IChunkPalette
{
	uint GetBlockState(uint id);
	int Length { get; }
	void Read(List<byte> buffer);
}
