using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0649 // field never assigned to

[Serializable]
class TranslateComponent : ChatComponent
{
	public string translation;
	public ChatComponent[] with;
}
