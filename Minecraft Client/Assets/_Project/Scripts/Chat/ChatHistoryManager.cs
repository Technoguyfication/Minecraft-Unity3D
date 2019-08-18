using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatHistoryManager : MonoBehaviour
{
	private static TMP_Text _chatText;
	private static readonly Queue<string> _messages = new Queue<string>();
	public const int MaxMessages = 8;
	public void Awake()
	{
		_chatText = GetComponent<TMP_Text>();
		ClearHistory();
	}

	/// <summary>
	/// Use this method to add to the chat history window, text can be in rich text format.
	/// </summary>
	/// <param name="message">The message to add, can be rich text.</param>
	public static void AddMessage(string message)
	{
		_messages.Enqueue(message);
		if (_messages.Count > MaxMessages)
			_messages.Dequeue();
		RewriteToUI();
	}

	/// <summary>
	/// Clears the chat history, and adds "Chat History Ready"
	/// </summary>
	public static void ClearHistory()
	{
		_messages.Clear();
		AddMessage("Chat History Ready");
		RewriteToUI();
	}

	private static void RewriteToUI()
	{
		_chatText.text = "";
		foreach (string entry in _messages)
		{
			_chatText.text += (_chatText.text.Length != 0 ? "\n" : "");
			_chatText.text += entry;
		}
	}
}
