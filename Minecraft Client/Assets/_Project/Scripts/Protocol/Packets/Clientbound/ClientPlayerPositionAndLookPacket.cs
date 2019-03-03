using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientPlayerPositionAndLookPacket : Packet
{
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
	public float Yaw { get; set; }
	public float Pitch { get; set; }
	public byte Flags { get; set; }
	public int TeleportID { get; set; }


	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			X = PacketHelper.GetDouble(buffer);
			Y = PacketHelper.GetDouble(buffer);
			Z = PacketHelper.GetDouble(buffer);
			Yaw = PacketHelper.GetSingle(buffer);
			Pitch = PacketHelper.GetSingle(buffer);
			Flags = buffer.Read(1)[0];
			TeleportID = VarInt.ReadNext(buffer);
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public ClientPlayerPositionAndLookPacket()
	{
		PacketID = default(int);
	}

	public ClientPlayerPositionAndLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
