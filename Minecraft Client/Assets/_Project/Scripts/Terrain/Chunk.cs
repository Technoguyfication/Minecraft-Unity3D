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
	/// The height of the heighest block in this chunk, rounded up to nearest 16
	/// </summary>
	public int MaxHeight { get; private set; }

	/// <summary>
	/// Blocks stored as (((y * 16) + z) * 16) + x
	/// This should not be used for normal block lookups, use <see cref="GetBlockAt(BlockPos)"/> instead
	/// </summary>
	public readonly BlockState[] BlockArray = new BlockState[256 * 16 * 16];
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

	/// <summary>
	/// Creates a chunk from a <see cref="ChunkDataPacket"/> in the specified world
	/// </summary>
	/// <param name="packet"></param>
	/// <param name="world"></param>
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
		return BlockArray[GetBlockIndex(chunkPos)];
	}

	/// <summary>
	/// Gets the block at an index
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public BlockState GetBlockAt(int index)
	{
		return BlockArray[index];
	}

	/// <summary>
	/// Gets the block light level of a block
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public int GetBlockLightLevel(BlockPos pos)
	{
		var chunkPos = pos.GetPosWithinChunk();
		return _blockLights[chunkPos.X, chunkPos.Y, chunkPos.Z];
	}

	/// <summary>
	/// Gets the sky light level of a block
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public int GetSkyLightLevel(BlockPos pos)
	{
		if (World.Dimension != World.DimensionType.OVERWORLD)
			throw new Exception("Sky lights do not exist in this dimension!");

		var chunkPos = pos.GetPosWithinChunk();
		return _blockLights[chunkPos.X, chunkPos.Y, chunkPos.Z];
	}

	/// <summary>
	/// Gets the index of a block inside a chunk section.
	/// To find the chunk section, use <see cref="GetChunkSection(BlockPos)"/>
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public static int GetBlockIndex(BlockPos pos)
	{
		return GetBlockIndex(pos.X, pos.Y, pos.Z);
	}

	public static int GetBlockIndex(int x, int y, int z)
	{
		return (((y * 16) + z) * 16) + x;
	}

	/// <summary>
	/// Returns whether the specified position exists inside a chunk.
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public static bool ExistsInside(BlockPos pos)
	{
		return pos.X >= 0 && pos.X < 16 && pos.Y >= 0 && pos.Y < 256 && pos.Z >= 0 && pos.Z < 16;
	}

	/// <summary>
	/// Populates this chunk with data from a <see cref="ChunkDataPacket"/>
	/// </summary>
	/// <param name="packet"></param>
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
		for (int s = 0; s < 16; s++)
		{
			// check bitmask to see if we're reading data for this section
			if ((packet.PrimaryBitmask & (1) << s) != 0)
			{
				// set max block height to the height of this chunk
				int maxBlockHeight = (s + 1) * 16;
				if (maxBlockHeight > MaxHeight)
					MaxHeight = maxBlockHeight;

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
				for (int b = 0; b < 4096; b++)    // for section height
				{
					int startLong = (b * bitsPerBlock) / 64;
					int startOffset = (b * bitsPerBlock) % 64;
					int endLong = ((b + 1) * bitsPerBlock - 1) / 64;

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

					// add block to block array
					BlockArray[b + (4096 * s)] = blk;
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

							_blockLights[x, y + (16 * s), z] = value & 0xf;
							_blockLights[x + 1, y + (16 * s), z] = (value >> 4) & 0xf;
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

								_skylights[x, y + (16 * s), z] = value & 0xf;
								_skylights[x + 1, y + (16 * s), z] = (value >> 4) & 0xf;
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
					for (int i = 0; i < 4096; i++)
					{
						BlockArray[s + (4096 * s)] = new BlockState(BlockType.AIR);
					}
				}
			}
		}

		// parse biomes
		if (packet.GroundUpContinuous)
		{
			//throw new NotImplementedException();
		}
	}

	// equality is determined by chunk position
	// we should never have duplicate chunks in a world
	// empahsis on "should"
	public override bool Equals(object obj)
	{
		Chunk c = obj as Chunk;
		return c?.Position.Equals(Position) ?? false;
	}

	public override int GetHashCode()
	{
		return Position.GetHashCode();
	}

	public override string ToString()
	{
		return $"({Position.X}, {Position.Z})";
	}
}
