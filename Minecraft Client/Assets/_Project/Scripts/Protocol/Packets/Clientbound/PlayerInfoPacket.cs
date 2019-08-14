using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlayerInfoPacket : Packet
{
	public override byte[] Payload { get; set; }

	public PlayerInfoPacket()
	{
		PacketID = (int)ClientboundIDs.PLAYER_INFO;
	}

	public PlayerInfoPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public struct AddPlayerAction
	{
		public int Name { get; }
		public Property[] Properties { get; }
		public GameMode GameMode { get; }
		public int Ping { get; }
		public bool HasDisplayName { get; }
		public string DisplayName { get; }
	}

	public struct Property
	{

	}

	public struct UpdateGamemodeAction
	{

	}
}
