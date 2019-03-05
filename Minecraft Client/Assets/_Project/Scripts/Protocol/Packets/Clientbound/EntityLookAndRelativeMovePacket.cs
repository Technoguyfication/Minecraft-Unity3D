using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLookAndRelativeMovePacket : Packet
{
	public int EntityID { get; set; }
	public short DeltaX { get; set; }
	public short DeltaY { get; set; }
	public short DeltaZ { get; set; }
	public byte Yaw { get; set; }
	public byte Pitch { get; set; }
	public bool OnGround { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			EntityID = VarInt.ReadNext(buffer);
			DeltaX = PacketHelper.GetInt16(buffer);
			DeltaY = PacketHelper.GetInt16(buffer);
			DeltaZ = PacketHelper.GetInt16(buffer);
			Yaw = buffer.Read(1)[0];
			Pitch = buffer.Read(1)[0];
			OnGround = PacketHelper.GetBoolean(buffer);
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public EntityLookAndRelativeMovePacket()
	{
		PacketID = (int)ClientboundIDs.ENTITY_LOOK_AND_RELATIVE_MOVE;
	}

	public EntityLookAndRelativeMovePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
