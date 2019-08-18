using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Represents a chat message from the server. Uses <see cref="ChatComponent"/> for formatting and styling
/// </summary>
public class ChatMessage
{
	/// <summary>
	/// Gets the message with no styling information
	/// </summary>
	public string PlaintextMessage { get; }

	/// <summary>
	/// For use in TextMeshPro dialogs
	/// </summary>
	public string HtmlFormattedMessage { get; }

	/// <summary>
	/// The position of the message on the screen
	/// </summary>
	public Position ChatPosition { get; set; }

	public ChatMessage(ChatMessagePacket pkt)
	{
		var chatComponent = ChatComponent.FromJson(pkt.Json);

		// use .ToString() for plaintext and components will output text with no formatting
		PlaintextMessage = chatComponent.ToString();

		HtmlFormattedMessage = ChatComponent.GetFormattedString(chatComponent, null);
	}

	public enum Position : byte
	{
		CHAT = 0x00,
		SYSTEM_MESSAGE = 0x01,
		ABOVE_HOTBAR = 0x02
	}
}
