using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class ServerErrorResponse
{
	public ServerErrorResponse()
	{ }

	public ServerErrorResponse(Exception ex)
	{
		error = ex.GetType().Name;
		errorMessage = ex.Message;
	}

	public ServerErrorResponse(string message)
	{
		error = "Unhandled Exception";
		errorMessage = message;
	}

	public string error;
	public string errorMessage;
	public string cause;

	public override string ToString()
	{
		// this mess returns the error + message and the cause, if it exists
		return $"{error}: {errorMessage}" + ((cause != null) ? $"({cause})" : "");
	}
}
