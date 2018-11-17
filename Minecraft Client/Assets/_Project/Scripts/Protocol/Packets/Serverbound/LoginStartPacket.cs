using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LoginStartPacket : Packet
{
	public string Username { get; set; }

	public override byte[] Payload
	{
		get
		{
			return PacketStructureUtility.GetBytes(Username);
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public LoginStartPacket()
	{
		PacketID = 0x00;
	}

	public LoginStartPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
