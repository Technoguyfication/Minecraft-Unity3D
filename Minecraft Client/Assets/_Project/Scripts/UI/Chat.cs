using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
	const string CHATBOX_PREFIX = "<font=\"Minecraft Regular SDF\"><mark=#00000065>";

	public TextMeshProUGUI ChatBox;
	public int MaxMessages = 100;
	public InputField ChatInput;

	private readonly List<string> _formattedMessages = new List<string>();

	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		// todo: add chat sending
	}

	/// <summary>
	/// Handles a chat packet from the server
	/// </summary>
	/// <param name="pkt"></param>
	public void HandleChatPacket(ChatMessagePacket pkt)
	{
		ChatMessage chatMessage;

		try
		{
			chatMessage = new ChatMessage(pkt);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Error parsing chat message: {ex}");
			return;
		}

		AddChatMessage(chatMessage);
	}

	/// <summary>
	/// Adds a <see cref="ChatMessage"/> to the UI chat box
	/// </summary>
	/// <param name="message"></param>
	public void AddChatMessage(ChatMessage message)
	{
		Debug.Log(message.PlaintextMessage);
		_formattedMessages.Add(message.HtmlFormattedMessage);

		RepopulateText();
	}

	private void RepopulateText()
	{
		// prune max messages
		if (_formattedMessages.Count > MaxMessages)
			_formattedMessages.RemoveRange(0, _formattedMessages.Count - MaxMessages);

		var chatText = new StringBuilder(CHATBOX_PREFIX);
		chatText.Append(string.Join("\n", _formattedMessages));

		ChatBox.text = chatText.ToString();
	}
}
