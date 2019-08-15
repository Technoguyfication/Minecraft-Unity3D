using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0649 // field never assigned to

[Serializable]
class ChatComponent
{
	public string text;
	public bool bold;
	public bool italic;
	public bool underlined;
	public bool obfuscated;
	public string color;
	public string insertion;
	public ChatClickEvent clickEvent;
	// hoverEvent

	public static ChatComponent GetChatComponent(string json)
	{
		var obj = JObject.Parse(json);

		// check which type of ChatComponent this is
		if (obj["translation"] != null)
			return JsonConvert.DeserializeObject<TranslateComponent>(json);

		return JsonConvert.DeserializeObject<ChatComponent>(json);
	}

	public override string ToString()
	{
		return text;
	}
}
