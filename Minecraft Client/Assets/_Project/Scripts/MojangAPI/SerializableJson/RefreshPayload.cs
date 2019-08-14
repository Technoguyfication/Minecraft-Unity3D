using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE1006

[Serializable]
public class RefreshPayload
{
	public RefreshPayload()
	{ }

	public RefreshPayload(string accessToken)
	{
		this.accessToken = accessToken;
	}

	public string accessToken;
	public string clientToken = MojangAPI.GetClientToken();
	public bool requestUser = false;
}
