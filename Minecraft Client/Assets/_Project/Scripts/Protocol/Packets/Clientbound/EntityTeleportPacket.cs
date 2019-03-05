using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityTeleportPacket : Packet
{
	public int EntityID { get; set; }
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
	public byte Yaw { get; set; }
	public byte Pitch { get; set; }
	public bool OnGround { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			EntityID = VarInt.ReadNext(buffer);
			X = PacketHelper.GetDouble(buffer);
			Y = PacketHelper.GetDouble(buffer);
			Z = PacketHelper.GetDouble(buffer);
			Yaw = buffer.Read(1)[0];
			Pitch = buffer.Read(1)[0];
			OnGround = PacketHelper.GetBoolean(buffer);
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public EntityTeleportPacket()
	{
		PacketID = (int)ClientboundIDs.ENTITY_TELEPORT;
	}

	public EntityTeleportPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
