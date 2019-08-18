using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DestroyEntitiesPacket : Packet
{
	public int EntitiesCount { get; set; }
	public int[] EntityIDs { get; set; }

	public DestroyEntitiesPacket()
	{
		PacketID = (int)ClientboundIDs.DestroyEntities;
	}

	public DestroyEntitiesPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					EntityIDs = new int[PacketReader.ReadVarInt(reader)];
					for (int i = 0; i < EntityIDs.Length; i++)
					{
						EntityIDs[i] = PacketReader.ReadVarInt(reader);
					}
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
