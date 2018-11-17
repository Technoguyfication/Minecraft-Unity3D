using System;
using System.Collections.Generic;
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

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			PlayerEntityId = PacketStructureUtility.GetInt32(buffer);
			GameMode = (GameMode)buffer.Read(1)[0];
			Difficulty = (Difficulty)buffer.Read(1)[0];
			MaxPlayers = buffer.Read(1)[0];

			string levelTypeString = PacketStructureUtility.GetString(buffer);
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

			ReducedDebug = PacketStructureUtility.GetBoolean(buffer);
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public JoinGamePacket()
	{
		PacketID = 0x25;
	}

	public JoinGamePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
