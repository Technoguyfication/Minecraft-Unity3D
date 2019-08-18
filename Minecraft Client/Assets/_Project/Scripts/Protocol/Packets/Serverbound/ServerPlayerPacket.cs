using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerPlayerPacket : Packet
{
	public bool OnGround { get; set; }

	public ServerPlayerPacket()
	{
		PacketID = (int)ServerboundIDs.Player;
	}

	public ServerPlayerPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteBoolean(writer, OnGround);

					return stream.ToArray();
				}
			}
		}
		set => throw new NotImplementedException();
	}

	public static ServerPlayerPacket FromPlayer(PlayerController player)
	{
		return new ServerPlayerPacket()
		{
			OnGround = player.OnGround
		};
	}
}
