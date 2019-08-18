using System;
using System.Collections.Generic;
using System.IO;
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

	public ServerPlayerPositionAndLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public ServerPlayerPositionAndLookPacket()
	{
		PacketID = (int)ServerboundIDs.PlayerPositionAndLook;
	}

	public override byte[] Payload
	{
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteDouble(writer, X);
					PacketWriter.WriteDouble(writer, FeetY);
					PacketWriter.WriteDouble(writer, Z);

					PacketWriter.WriteFloat(writer, Yaw);
					PacketWriter.WriteFloat(writer, Pitch);

					PacketWriter.WriteBoolean(writer, OnGround);

					return stream.ToArray();
				}
			}
		}
		set => throw new NotImplementedException();
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
}
