using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerPlayerPositionAndLookPacket : Packet
{
	public double X { get; set; }
	public double FeetY { get; set; }
	public double Z { get; set; }
	public float Yaw { get; set; }
	public float Pitch { get; set; }
	public bool OnGround { get; set; }

	public override byte[] Payload {
		get
		{
			List<byte> builder = new List<byte>();
			builder.AddRange(BitConverter.GetBytes(X).ReverseIfLittleEndian());
			builder.AddRange(BitConverter.GetBytes(FeetY).ReverseIfLittleEndian());
			builder.AddRange(BitConverter.GetBytes(Z).ReverseIfLittleEndian());
			builder.AddRange(BitConverter.GetBytes(Yaw).ReverseIfLittleEndian());
			builder.AddRange(BitConverter.GetBytes(Pitch).ReverseIfLittleEndian());
			builder.AddRange(BitConverter.GetBytes(OnGround));
			return builder.ToArray();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public ServerPlayerPositionAndLookPacket()
	{
		PacketID = (int)ServerboundIDs.PLAYER_POSITION_AND_LOOK;
	}

	public static ServerPlayerPositionAndLookPacket FromPlayer(PlayerController player)
	{
		return new ServerPlayerPositionAndLookPacket()
		{
			X = player.X,
			FeetY = player.Y,
			Z = player.Z,
			Yaw = player.Yaw - 90,
			Pitch = player.Pitch,
			OnGround = player.OnGround
		};
	}

	public ServerPlayerPositionAndLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
