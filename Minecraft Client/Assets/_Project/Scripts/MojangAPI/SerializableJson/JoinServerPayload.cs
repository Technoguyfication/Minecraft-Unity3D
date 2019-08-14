using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE1006

[Serializable]
public class JoinServerPayload
{
	public JoinServerPayload()
	{ }

	public JoinServerPayload(string accessToken, string playerUuid, string serverHash)
	{
		this.accessToken = accessToken;
		selectedProfile = playerUuid;
		serverId = serverHash;
	}

	public string accessToken;
	public string selectedProfile;
	public string serverId;
}
