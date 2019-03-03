using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Initializes an entity on the client
/// </summary>
public class EntityPacket : Packet
{
	public EntityPacket()
	{
		PacketID = 0x27;
	}

	public override byte[] Payload
	{
		set
		{
			EntityID = VarInt.ReadNext(value);
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public int EntityID { get; set; }
}
