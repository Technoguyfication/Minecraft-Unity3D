using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class JoinGamePacket : Packet
{
	public int PlayerEntityId { get; set; }
	public GameMode GameMode { get; set; }
	public World.DimensionType Dimension { get; set; }
	public Difficulty Difficulty { get; set; }
	public int MaxPlayers { get; set; }
	public LevelType LevelType { get; set; }
	public bool ReducedDebug { get; set; }

	public JoinGamePacket()
	{
		PacketID = (int)ClientboundIDs.JoinGame;
	}

	public JoinGamePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					PlayerEntityId = PacketReader.ReadInt32(reader);
					GameMode = (GameMode)PacketReader.ReadByte(reader);
					Dimension = (World.DimensionType)PacketReader.ReadInt32(reader);
					Difficulty = (Difficulty)PacketReader.ReadByte(reader);
					MaxPlayers = PacketReader.ReadByte(reader);

					string levelTypeString = PacketReader.ReadString(reader);
					switch (levelTypeString.ToLower())
					{
						case "default":
							LevelType = LevelType.DEFAULT;
							break;
						case "flat":
							LevelType = LevelType.FLAT;
							break;
						case "largebiomes":
							LevelType = LevelType.LARGE_BIOMES;
							break;
						case "amplified":
							LevelType = LevelType.AMPLIFIED;
							break;
						case "default_1_1":
							LevelType = LevelType.DEFAULT_1_1;
							break;
						default:
							throw new MalformedPacketException($"Level type \"{levelTypeString}\" does not exist");

					}

					ReducedDebug = PacketReader.ReadBoolean(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
