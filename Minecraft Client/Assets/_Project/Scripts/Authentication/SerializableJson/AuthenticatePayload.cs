using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class AuthenticatePayload
{
	public AuthenticatePayload()
	{ }

	public AuthenticatePayload(string username, string password)
	{
		Username = username;
		Password = password;
	}

	public AuthenticationPayloads.Agent Agent { get; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string ClientToken
	{
		get
		{
			return MojangAuthentication.GetClientToken();
		}
	}
	public bool RequestUser { get; set; } = false;
}

// create a new namespace so we aren't cluttering the default namespace with json data
namespace AuthenticationPayloads
{
	[Serializable]
	public class Agent
	{
		public string Name { get; set; } = "Minecraft";
		public int Version { get; set; } = 1;
	}
}
