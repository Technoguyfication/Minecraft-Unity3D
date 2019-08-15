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
	[JsonProperty("bold")]
	public bool Bold { get; set; }

	[JsonProperty("italic")]
	public bool Italic { get; set; }

	[JsonProperty("underlined")]
	public bool Underlined { get; set; }

	[JsonProperty("obfuscated")]
	public bool Obfuscated { get; set; }

	[JsonProperty("color")]
	public string Color { get; set; }

	[JsonProperty("insertion")]
	public string Insertion { get; set; }

	[JsonProperty("clickEvent")]
	public ChatClickEvent ClickEvent { get; set; }
	// hoverEvent

	public static ChatComponent FromJson(string json)
	{
		return GetChatComponent(JObject.Parse(json));
	}

	public static ChatComponent GetChatComponent(JObject obj)
	{
		// check which type of ChatComponent this is
		if (obj["text"] != null)    // string component
			return obj.ToObject<StringComponent>();
		if (obj["translate"] != null) // translate component
			return obj.ToObject<TranslateComponent>();
		else if (obj["keybind"] != null)    // keybind component
			throw new NotImplementedException("Keybind chat components are not yet supported");
		else if (obj["score"] != null)  // score component
			throw new NotImplementedException("Score chat components are not yet supported");
		else    // default to normal ChatComponent as all components can have these values
			return obj.ToObject<ChatComponent>();
	}
}

public class ChatComponentJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType.Equals(typeof(ChatComponent));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (objectType.Equals(typeof(ChatComponent)))
		{
			var obj = JObject.Load(reader);
			return ChatComponent.GetChatComponent(obj);
		}
		else if (objectType.Equals(typeof(ChatComponent[])))
		{
			var objArr = JArray.Load(reader);
			var chatComponents = new ChatComponent[objArr.Count];
			for (int i = 0; i < chatComponents.Length; i++)
			{
				if (objArr[i].Type == JTokenType.Object)	// object type can be any component
					chatComponents[i] = ChatComponent.GetChatComponent((JObject)objArr[i]);
				else if (objArr[i].Type == JTokenType.String)	// string component
					chatComponents[i] = new StringComponent() { text = (string)objArr[i] };
			}

			return chatComponents;
		}

		throw new JsonReaderException($"ChatComponentJsonConverter cannot deserialize to {objectType.ToString()}");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override bool CanWrite => false;
}
