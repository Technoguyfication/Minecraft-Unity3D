using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LoginSuccessPacket : Packet
{
	public Guid UUID { get; set; }
	public string Username { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			UUID = Guid.Parse(PacketHelper.GetString(buffer));
			Username = PacketHelper.GetString(buffer);
		}
		get => throw new NotImplementedException();
	}

	public LoginSuccessPacket()
	{
		PacketID = (int)ClientboundIDs.LOGIN_SUCCESS;
	}

	public LoginSuccessPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
