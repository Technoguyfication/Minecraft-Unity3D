using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityHeadLookPacket : Packet
{
	public int EntityID { get; set; }
	public sbyte HeadYaw { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			EntityID = VarInt.ReadNext(buffer);
			HeadYaw = (sbyte)buffer.Read(1)[0];
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public EntityHeadLookPacket()
	{
		PacketID = (int)ClientboundIDs.ENTITY_HEAD_LOOK;
	}

	public EntityHeadLookPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
