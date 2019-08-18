using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ChatTranslation
{
	private static readonly Dictionary<string, string> _translationTable = new Dictionary<string, string>();

	static ChatTranslation()
	{
		_translationTable.Add("chat.type.text", "<{0}> {1}");
		_translationTable.Add("gameMode.spectator", "Spectator Mode");
	}

	/// <summary>
	/// Translates message and adds plaintext insertions
	/// </summary>
	/// <param name="key"></param>
	/// <param name="insertions"></param>
	/// <returns></returns>
	public static string TranslateString(string key, string[] insertions)
	{
		if (_translationTable.ContainsKey(key))
		{
			return string.Format(_translationTable[key], insertions);
		}
		else
		{
			// if there are insertions, add them after the raw key name
			if (insertions != null && insertions.Length > 0)
				return $"{key}: [\"{string.Join("\", \"", insertions)}\"]"; // chat.type.text: "PlayerName", "message here"
			else
				return $"{key}: []";
		}
	}
}
