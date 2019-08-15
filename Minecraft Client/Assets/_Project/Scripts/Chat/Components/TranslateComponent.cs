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
	public ChatComponent[] with;

	public override string ToString()
	{
		return ChatTranslation.TranslateString(translate, with.Select(c => c.ToString()).ToArray());
	}
}
