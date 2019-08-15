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
	public string translation;

	[JsonConverter(typeof(ChatComponentJsonConverter))]
	public ChatComponent[] with;

	public override string GetComponentText(bool useFormatting, LinkedList<string> endBuilder)
	{
		return AppendFormatting(this, endBuilder) + ChatTranslation.TranslateString(translation, with.Select(c => c.GetComponentText(useFormatting, endBuilder)).ToArray());
	}
}
