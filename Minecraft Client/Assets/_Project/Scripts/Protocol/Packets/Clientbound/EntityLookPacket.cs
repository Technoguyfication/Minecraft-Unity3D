using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLookPacket : Packet
{
	public int EntityID { get; set; }
	public sbyte Yaw { get; set; }
	public sbyte Pitch { get; set; }
	public bool OnGround { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			EntityID = VarInt.ReadNext(buffer);
			Yaw = (sbyte)buffer.Read(1)[0];
			Pitch = (sbyte)buffer.Read(1)[0];
			OnGround = PacketHelper.GetBoolean(buffer);
		}
		get => throw new NotImplementedException();
	}

	public EntityLookPacket()
	{
		PacketID = (int)ClientboundIDs.ENTITY_LOOK;
	}

	public EntityLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
