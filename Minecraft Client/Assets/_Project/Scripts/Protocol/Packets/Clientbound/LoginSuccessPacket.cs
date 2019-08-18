using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LoginSuccessPacket : Packet
{
	public Guid UUID { get; set; }
	public string Username { get; set; }

	public LoginSuccessPacket()
	{
		PacketID = (int)ClientboundIDs.LogIn_Success;
	}

	public LoginSuccessPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					UUID = Guid.Parse(PacketReader.ReadString(reader));
					Username = PacketReader.ReadString(reader);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
