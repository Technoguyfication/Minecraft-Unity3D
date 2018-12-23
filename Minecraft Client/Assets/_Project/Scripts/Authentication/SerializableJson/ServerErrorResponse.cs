using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class ServerErrorResponse
{
	public ServerErrorResponse()
	{}

	public ServerErrorResponse(Exception ex)
	{
		Error = ex.GetType().Name;
		ErrorMessage = ex.Message;
	}

	public ServerErrorResponse(string message)
	{
		Error = "Unhandled Exception";
		ErrorMessage = message;
	}

	public string Error;
	public string ErrorMessage;
	public string Cause;

	new public string ToString()
	{
		// this mess returns the error + message and the cause, if it exists
		return $"{Error}: {ErrorMessage}" + ((Cause != null) ? $"({Cause})" : "");
	}
}
