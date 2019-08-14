using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class RefreshResponse
{
	// there's a bunch more crap we could put here but this is all we care about
	// see: https://wiki.vg/Authentication#Response
	public string accessToken;
	public AuthenticationPayloads.Profile selectedProfile;
}


namespace AuthenticationPayloads
{
	[Serializable]
	public class Profile
	{
		public string id;
		public string name;
		public bool legacy;
	}
}