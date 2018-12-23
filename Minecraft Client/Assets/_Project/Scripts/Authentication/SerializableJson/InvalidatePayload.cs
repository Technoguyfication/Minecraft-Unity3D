using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class InvalidatePayload
{
	public InvalidatePayload()
	{ }

	public InvalidatePayload(string accessToken)
	{
		AccessToken = accessToken;
	}

	public string AccessToken;
	public string ClientToken = MojangAuthentication.GetClientToken();
}
