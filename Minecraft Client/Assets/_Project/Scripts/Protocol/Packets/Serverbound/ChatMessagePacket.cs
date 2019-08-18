using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CSChatMessagePacket : Packet
{
	public string Message { get; set; }

	public CSChatMessagePacket()
	{
		PacketID = (int)ServerboundIDs.ChatMessage;
	}

	public CSChatMessagePacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteString(writer, Message);

					return stream.ToArray();
				}
			}
		}
		set => throw new NotImplementedException();
	}
}
