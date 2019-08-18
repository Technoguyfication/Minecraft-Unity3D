using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationPayloads;

#pragma warning disable IDE1006

[Serializable]
public class AuthenticatePayload
{
	public AuthenticatePayload()
	{ }

	public AuthenticatePayload(string username, string password)
	{
		this.username = username;
		this.password = password;
	}

	public Agent agent = new Agent();
	public string username;
	public string password;
	public string clientToken = MojangAPI.GetClientToken();
	public bool requestUser = false;
}

// create a new namespace so we aren't cluttering the default namespace with json data
namespace AuthenticationPayloads
{
	[Serializable]
	public class Agent
	{
		public string name = "Minecraft";
		public int version = 1;
	}
}
