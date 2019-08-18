using System;
using System.Collections.Generic;
using System.IO;
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
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteString(writer, Username);

					return stream.ToArray();
				}
			}
			//PacketHelper.GetBytes(Username);
		}
		set => throw new NotImplementedException();
	}

	public LoginStartPacket()
	{
		PacketID = 0x00;
	}

	public LoginStartPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
