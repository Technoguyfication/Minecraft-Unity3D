using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CSChatMessagePacket : Packet
{
	public string Message { get; set; }

	public override byte[] Payload
	{
		get => PacketHelper.GetBytes(Message);
		set => throw new NotImplementedException();
	}

	public CSChatMessagePacket()
	{
		PacketID = (int)ServerboundIDs.CHAT_MESSAGE;
	}

	public CSChatMessagePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used
}
