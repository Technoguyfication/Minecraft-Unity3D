using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0649 // field never assigned to

[Serializable]
public class ChatClickEvent
{
	public string open_url;
	public string run_command;
	public string suggest_command;
	public int change_page;
}
