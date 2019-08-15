using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Chat message packet https://wiki.vg/Chat
/// </summary>
public class ChatMessagePacket : Packet
{
	public ChatMessagePacket()
	{
		PacketID = (int)ClientboundIDs.CHAT_MESSAGE;
	}

	public ChatMessagePacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			var buffer = new List<byte>(value);
			ChatMessage = PacketHelper.GetString(buffer);
			Position = (global::ChatMessage.Position)buffer.Read(1)[0];
		}
	}

	public string ChatMessage { get; set; }
	public ChatMessage.Position Position { get; set; }
}
