using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

#pragma warning disable CS0649 // field never assigned to

[Serializable]
class TranslateComponent : ChatComponent
{
	public string translate;

	[JsonConverter(typeof(ChatComponentJsonConverter))]
	public ChatComponent[] with = new ChatComponent[0];

	public override string ToString()
	{
		string translatedString = ChatTranslation.TranslateString(translate, with?.Select((c) =>
		{
			return c.ToString();
		})?.ToArray());

		return translatedString + base.ToString();
	}
}
