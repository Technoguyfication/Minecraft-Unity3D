using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerPlayerPacket : Packet
{
	public bool OnGround { get; set; }

	public override byte[] Payload {
		get
		{
			List<byte> builder = new List<byte>();
			builder.AddRange(BitConverter.GetBytes(OnGround));
			return builder.ToArray();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public ServerPlayerPacket()
	{
		PacketID = (int)ServerboundIDs.PLAYER;
	}

	public static ServerPlayerPacket FromPlayer(PlayerController player)
	{
		return new ServerPlayerPacket()
		{
			OnGround = player.OnGround
		};
	}

	public ServerPlayerPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
