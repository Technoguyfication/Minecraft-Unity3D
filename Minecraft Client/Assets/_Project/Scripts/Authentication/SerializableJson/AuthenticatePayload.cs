using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationPayloads;

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

	public Agent Agent = new Agent();
	public string Username;
	public string Password;
	public string ClientToken = MojangAuthentication.GetClientToken();
	public bool RequestUser = false;
}

// create a new namespace so we aren't cluttering the default namespace with json data
namespace AuthenticationPayloads
{
	[Serializable]
	public class Agent
	{
		public string Name = "Minecraft";
		public int Version = 1;
	}
}
