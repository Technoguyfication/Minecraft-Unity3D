using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#pragma warning disable CS0649 // field never assigned to

/// <summary>
/// Represents a stylized string from the server. Can be used in chat messages, dialog titles, action bars, etc.
/// </summary>
[Serializable]
public class ChatComponent
{
	[JsonProperty("bold")]
	public bool Bold { get; set; }

	[JsonProperty("italic")]
	public bool Italic { get; set; }

	[JsonProperty("underlined")]
	public bool Underlined { get; set; }

	[JsonProperty("strikethrough")]
	public bool Strikethrough { get; set; }

	[JsonProperty("obfuscated")]
	public bool Obfuscated { get; set; }

	[JsonProperty("color")]
	public string Color { get; set; }

	[JsonProperty("insertion")]
	public string Insertion { get; set; }

	[JsonProperty("clickEvent")]
	public ChatClickEvent ClickEvent { get; set; }

	[JsonProperty("extra")]
	[JsonConverter(typeof(ChatComponentJsonConverter))]
	public ChatComponent[] Extra;

	/// <summary>
	/// Gets a <see cref="ChatComponent"/> from a JSON string, parsing all it's children as well
	/// </summary>
	/// <param name="json"></param>
	/// <returns></returns>
	public static ChatComponent FromJson(string json)
	{
		return FromJson(JObject.Parse(json));
	}

	/// <summary>
	/// Gets a <see cref="ChatComponent"/> from a JSON Object, parsing all it's children as well
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static ChatComponent FromJson(JObject obj)
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

	/// <summary>
	/// Gets an HTML-formatted TextMeshPro string for a <see cref="ChatComponent"/>
	/// </summary>
	/// <param name="component">The chat component to convert into a formatted string</param>
	/// <param name="parent">The parent chat Component of <paramref name="component"/>, used for inheriting styles</param>
	/// <returns></returns>
	public static string GetFormattedString(ChatComponent component, ChatComponent parent)
	{
		var formattedString = new StringBuilder();
		var closeTags = new Stack<string>();

		// write formatting before the text.
		// if the style is not present in this component, check the parent's formatting
		if (component.Bold || (parent?.Bold ?? false))
			writeTag("b");
		if (component.Italic || (parent?.Italic ?? false))
			writeTag("i");
		if (component.Underlined || (parent?.Underlined ?? false))
			writeTag("u");
		if (component.Strikethrough || (parent?.Strikethrough ?? false))
			writeTag("s");
		if (component.Obfuscated || (parent?.Obfuscated ?? false))
			writeTag("s"); // strikethrough placeholder for magic text todo: magic text?
		if (!string.IsNullOrEmpty(component.Color) || !string.IsNullOrEmpty(parent?.Color))
			writeColor(component.Color ?? parent.Color);

		void writeTag(string tag)
		{
			formattedString.Append($"<{tag}>");
			closeTags.Push($"</{tag}>");
		}

		void writeColor(string color)
		{
			switch (color)
			{
				case "l":   // bold
					writeTag("b");
					break;
				case "o":   // italic
					writeTag("i");
					break;
				case "n":   // underlined
					writeTag("u");
					break;
				case "m":   // strikethrough
					writeTag("s");
					break;
				case "r":   // reset - don't write any color at all
					return;
				case "k":   // obfuscated or magic
					writeTag("s");  // placeholder for obfuscated text
					break;
				default:    // "color" isn't a formatting code so it must be an actual color
					string htmlColorCode = ColorUtility.ToHtmlStringRGB(GetChatForegroundColor(color));
					formattedString.Append($"<color=#{htmlColorCode}>");
					closeTags.Push("</color>");
					break;
			}
		}

		// write the text content of the component
		switch (component)
		{
			case StringComponent stringComponent:
				formattedString.Append(stringComponent.text);
				break;
			case TranslateComponent translateComponent:
				var formattedWith = translateComponent.with.Select(w => GetFormattedString(w, component));  // get formatted translation inserts
				formattedString.Append(ChatTranslation.TranslateString(translateComponent.translate, formattedWith.ToArray())); // translate string and append
				break;
			default:
				throw new NotImplementedException($"{component.GetType().ToString()} is not supported yet");
		}

		// append the close tags
		formattedString.Append(string.Join(string.Empty, closeTags));

		// add extra ChatComponents
		if (component.Extra != null)
		{
			foreach (var extraComp in component.Extra)
			{
				formattedString.Append(GetFormattedString(extraComp, component));
			}
		}

		return formattedString.ToString();
	}

	public static Color GetChatForegroundColor(string colorCode)
	{
		switch (colorCode)
		{
			case "0":
			case "black":
				return UnityEngine.Color.black;
			case "1":
			case "dark_blue":
				return new Color(0f, 0f, .66f);
			case "2":
			case "dark_green":
				return new Color(0f, .66f, 0f);
			case "3":
			case "dark_aqua":
				return new Color(0f, .66f, .66f);
			case "4":
			case "dark_red":
				return new Color(.66f, 0f, 0f);
			case "5":
			case "dark_purple":
				return new Color(.66f, 0f, .66f);
			case "6":
			case "gold":
				return new Color(1f, .66f, 0f);
			case "7":
			case "gray":
				return new Color(.66f, .66f, .66f);
			case "8":
			case "dark_gray":
				return new Color(.33f, .33f, .33f);
			case "9":
			case "blue":
				return new Color(.33f, .33f, 1f);
			case "a":
			case "green":
				return new Color(.33f, 1f, .33f);
			case "b":
			case "aqua":
				return new Color(.33f, 1f, 1f);
			case "c":
			case "red":
				return new Color(1f, .33f, .33f);
			case "d":
			case "light_purple":
				return new Color(1f, .33f, 1f);
			case "e":
			case "yellow":
				return new Color(1f, 1f, .33f);
			case "f":
			case "white":
				return UnityEngine.Color.white;
			default:
				throw new NotImplementedException($"Specified color code {colorCode} does not exist");
		}
	}

#pragma warning disable IDE0060 // Remove unused parameter
	public Color GetChatBackgroundColor(string colorCode)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		throw new NotImplementedException();
	}

	public enum CHAT_COLOR
	{
		BLACK,
		DARK_BLUE,
		DARK_GREEN,
		DARK_CYAN,
		DARK_RED,
		PURPLE,
		GOLD,
		GRAY,
		DARK_GRAY,
		BLUE,
		BRIGHT_GREEN,
		CYAN,
		RED,
		PINK,
		YELLOW,
		WHITE
	}

	public override string ToString()
	{
		if (Extra != null)
		{
			var sb = new StringBuilder();

			foreach (var extra in Extra)
			{
				sb.Append(extra.ToString());
			}

			return sb.ToString();
		}
		else
		{
			return string.Empty;
		}
	}
}

/// <summary>
/// Utility class for converting JSON Data into <see cref="ChatComponent"/>s
/// </summary>
public class ChatComponentJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType.Equals(typeof(ChatComponent)) || objectType.Equals(typeof(ChatComponent[]));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (objectType.Equals(typeof(ChatComponent)))
		{
			var obj = JObject.Load(reader);
			return ChatComponent.FromJson(obj);
		}
		else if (objectType.Equals(typeof(ChatComponent[])))
		{
			var objArr = JArray.Load(reader);
			var chatComponents = new ChatComponent[objArr.Count];
			for (int i = 0; i < chatComponents.Length; i++)
			{
				if (objArr[i].Type == JTokenType.Object)    // object type can be any component
					chatComponents[i] = ChatComponent.FromJson((JObject)objArr[i]);
				else if (objArr[i].Type == JTokenType.String)   // string components can be single json primitives
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
