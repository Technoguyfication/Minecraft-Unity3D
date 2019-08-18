using System;
using System.Collections.Generic;
using System.IO;
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

	public ClientPlayerPositionAndLookPacket()
	{
		PacketID = (int)ClientboundIDs.PlayerPositionAndLook;
	}

	public ClientPlayerPositionAndLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					X = PacketReader.ReadDouble(reader);
					Y = PacketReader.ReadDouble(reader);
					Z = PacketReader.ReadDouble(reader);

					Yaw = PacketReader.ReadSingle(reader);
					Pitch = PacketReader.ReadSingle(reader);

					Flags = PacketReader.ReadByte(reader);

					TeleportID = PacketReader.ReadVarInt(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}

}
