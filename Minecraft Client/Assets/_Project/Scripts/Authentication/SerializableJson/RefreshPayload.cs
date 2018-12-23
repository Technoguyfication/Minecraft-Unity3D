using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class RefreshPayload
{
	public RefreshPayload()
	{ }

	public RefreshPayload(string accessToken)
	{
		AccessToken = accessToken;
	}

	public string AccessToken;
	public string ClientToken = MojangAuthentication.GetClientToken();
	public bool RequestUser = false;
}
