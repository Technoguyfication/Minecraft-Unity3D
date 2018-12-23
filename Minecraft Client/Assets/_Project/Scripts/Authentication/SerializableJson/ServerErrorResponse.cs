using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class ServerErrorResponse
{
	public string Error { get; set; }
	public string ErrorMessage { get; set; }
	public string Cause { get; set; }

	new public string ToString()
	{
		// this mess returns the error + message and the cause, if it exists
		return $"{Error}: {ErrorMessage}" + ((Cause != null) ? $"({Cause})" : "");
	}
}
