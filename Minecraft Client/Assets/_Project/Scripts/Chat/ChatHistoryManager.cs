using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatHistoryManager : MonoBehaviour
{
    private static Text text;
    private static Queue<string> messages = new Queue<string>();
    private const int maxMessage = 8;
    void Awake()
    {
        text = GetComponent<Text>();
        ClearHistory();
    }

    /// <summary>
    /// Use this method to add to the chat history window, text can be in rich text format.
    /// </summary>
    /// <param name="message">The message to add, can be rich text.</param>
    public static void AddMessage(string message)
    {
        messages.Enqueue(message);
        if (messages.Count > maxMessage)
            messages.Dequeue();
        RewriteToUI();
    }

    /// <summary>
    /// Clears the chat history, and adds "Chat History Ready"
    /// </summary>
    public static void ClearHistory()
    {
        messages.Clear();
        AddMessage("Chat History Ready");
        RewriteToUI();
    }

    private static void RewriteToUI()
    {
        text.text = "";
        foreach (string entry in messages)
        {
            text.text += (text.text.Length != 0 ? "\n" : "");
            text.text += entry;
        }
    }
}
