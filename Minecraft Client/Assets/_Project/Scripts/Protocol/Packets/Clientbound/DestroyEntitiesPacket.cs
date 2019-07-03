using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DestroyEntitiesPacket : Packet
{
	public int EntitiesCount { get; set; }
	public int[] EntityIDs { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			EntityIDs = new int[VarInt.ReadNext(buffer)];
			for (int i = 0; i < EntityIDs.Length; i++)
			{
				EntityIDs[i] = VarInt.ReadNext(buffer);
			}
		}
		get => throw new NotImplementedException();
	}

	public DestroyEntitiesPacket()
	{
		PacketID = (int)ClientboundIDs.DESTROY_ENTITIES;
	}

	public DestroyEntitiesPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
