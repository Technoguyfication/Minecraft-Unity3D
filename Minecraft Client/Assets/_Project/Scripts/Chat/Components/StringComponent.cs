using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0649 // field never assigned to

[Serializable]
class StringComponent : ChatComponent
{
	public string text;

	public override string ToString()
	{
		return text + base.ToString();
	}
}
