using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a Chunk column
/// </summary>
public class Chunk
{
	/// <summary>
	/// The position of the chunk within the world
	/// </summary>
	public ChunkPos Position { get; }

	public World World { get; set; }

	/// <summary>
	/// Gets whether or not the chunk has been completely loaded, and can be rendered
	/// </summary>
	public bool IsLoaded { get; private set; }

	/// <summary>
	/// Blocks stored in chunks as WXYZ where W = chunk index in column and XYZ are block coords relative to chunk
	/// </summary>
	private readonly BlockState[,,] _blocks = new BlockState[16, 256, 16];
	private readonly int[,,] _blockLights = new int[16, 256, 16];
	private readonly int[,,] _skylights = new int[16, 256, 16];

	/// <summary>
	/// Biome map for chunk
	/// </summary>
	private readonly Biome[,] _biomeMap = new Biome[16, 16];

	/// <summary>
	/// Creates a new chunk and assigns it to chunk coordinates in the world
	/// </summary>
	/// <param name="x"></param>
	/// <param name="z"></param>
	public Chunk(int x, int z, World world)
	{
		Position = new ChunkPos()
		{
			X = x,
			Z = z
		};
		World = world;
	}

	public Chunk(ChunkDataPacket packet, World world)
	{
		Position = packet.Position;
		World = world;
		AddChunkData(packet);
	}

	/// <summary>
	/// Gets the block at the specified position
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public BlockState GetBlockAt(BlockPos pos)
	{
		if (pos.Y > 255 || pos.Y < 0)
			return new BlockState(BlockType.VOID_AIR);

		var chunkPos = pos.GetPosWithinChunk();
		return _blocks[chunkPos.X, chunkPos.Y, chunkPos.Z];
	}

	public int GetBlockLightLevel(BlockPos pos)
	{
		var chunkPos = pos.GetPosWithinChunk();
		return _blockLights[pos.X, pos.Y, pos.Z];
	}

	public int GetSkyLightLevel(BlockPos pos)
	{
		if (World.Dimension != World.DimensionType.OVERWORLD)
			throw new Exception("Sky lights do not exist in this dimension!");

		var chunkPos = pos.GetPosWithinChunk();
		return _skylights[pos.X, pos.Y, pos.Z];
	}

	public void AddChunkData(ChunkDataPacket packet)
	{
		//check if data applies to this chunk
		if (!packet.Position.Equals(Position))
		{
			Debug.LogWarning($"Chunk at {Position} tried to add data for chunk at {packet.Position}??");
			return;
		}

		var data = new List<byte>(packet.Data);

		// chunk data
		for (int w = 0; w < 16; w++)
		{
			// check bitmask to see if we're reading data for this section
			if ((packet.PrimaryBitmask & (1) << w) != 0)
			{
				// read bits per block
				byte bitsPerBlock = data.Read(1)[0];
				if (bitsPerBlock < 4)
					bitsPerBlock = 4;

				// choose palette type
				IChunkPalette palette;
				if (bitsPerBlock <= 8)
					palette = new IndirectChunkPalette();
				else
					palette = new DirectChunkPalette();

				palette.Read(data); // read palette data, bringing chunk data into front of buffer

				// bitmask that contains bitsperblock set bits
				uint individualValueMask = (uint)((1 << bitsPerBlock) - 1);

				// read data into array of longs
				int dataArrayLength = VarInt.ReadNext(data);
				ulong[] dataArray = new ulong[dataArrayLength];
				for (int i = 0; i < dataArrayLength; i++)
				{
					dataArray[i] = PacketStructureUtility.GetUInt64(data);
				}

				// parse block data
				for (int y = 0; y < 16; y++)    // for section height
				{
					for (int z = 0; z < 16; z++)    // section width
					{
						for (int x = 0; x < 16; x++)    // section width
						{
							int blockNumber = (((y * 16) + z) * 16) + x;
							int startLong = (blockNumber * bitsPerBlock) / 64;
							int startOffset = (blockNumber * bitsPerBlock) % 64;
							int endLong = ((blockNumber + 1) * bitsPerBlock - 1) / 64;

							uint blockData;
							if (startLong == endLong)
							{
								blockData = (uint)(dataArray[startLong] >> startOffset);
							}
							else
							{
								int endOffset = 64 - startOffset;
								blockData = (uint)(dataArray[startLong] >> startOffset | dataArray[endLong] << endOffset);
							}

							blockData &= individualValueMask;

							uint blockState = palette.GetBlockState(blockData);
							BlockState blk = new BlockState((BlockType)blockState);

							_blocks[x, y + (16 * w), z] = blk;
						}
					}
				}

				// parse block light data
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						for (int x = 0; x < 16; x += 2)
						{
							// light data takes 4 bits. because of this, we read two light values per byte
							byte value = data.Read(1)[0];

							_blockLights[x, y + (16 * w), z] = value & 0xf;
							_blockLights[x + 1, y + (16 * w), z] = (value >> 4) & 0xf;
						}
					}
				}

				// parse sky lights
				if (World.Dimension == World.DimensionType.OVERWORLD)
				{
					// parse block light data
					for (int y = 0; y < 16; y++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int x = 0; x < 16; x += 2)
							{
								// light data takes 4 bits. because of this, we read two light values per byte
								byte value = data.Read(1)[0];

								_skylights[x, y + (16 * w), z] = value & 0xf;
								_skylights[x + 1, y + (16 * w), z] = (value >> 4) & 0xf;
							}
						}
					}
				}
			}
			else
			{
				// fill chunk section with air if full chunk
				if (packet.GroundUpContinuous)
				{
					for (int x = 0; x < 16; x++)
						for (int y = 0; y < 16; y++)
							for (int z = 0; z < 16; z++)
								_blocks[x, y + (16 * w), z] = new BlockState(BlockType.AIR);
				}
			}
		}

		// parse biomes
		if (packet.GroundUpContinuous)
		{
			//throw new NotImplementedException();
		}
	}
}
