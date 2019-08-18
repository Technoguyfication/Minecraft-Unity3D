using System;
using System.Collections.Generic;
using System.IO;
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
		PacketID = (int)ClientboundIDs.ChatMessage;
	}

	public ChatMessagePacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					Json = PacketReader.ReadString(reader);
					Position = (ChatMessage.Position)PacketReader.ReadByte(reader);
				}
			}
		}
	}

	public string Json { get; protected set; }
	public ChatMessage.Position Position { get; protected set; }
}
