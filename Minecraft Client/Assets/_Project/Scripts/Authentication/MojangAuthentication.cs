using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Used for authenticating with mojang's servers.
/// All methods are blocking
/// </summary>
public static class MojangAuthentication
{
	public static string Username { get; private set; }
	public static Guid UUID { get; private set; }

	private const string LoginServer = @"https://authserver.mojang.com";
	private const string clientTokenPrefKey = "authClientToken";
	private const string accessTokenPrefKey = "authAccessToken";

	/// <summary>
	/// The access token to be used by the client
	/// </summary>
	public static string AccessToken
	{
		get => PlayerPrefs.GetString(accessTokenPrefKey, null);
		set
		{
			PlayerPrefs.SetString(accessTokenPrefKey, value);
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// Retreives the semi-permanent client token for this client
	/// </summary>
	/// <returns></returns>
	public static string GetClientToken()
	{
		// generate new client token if we need
		if (!PlayerPrefs.HasKey(clientTokenPrefKey))
		{
			string newToken = Guid.NewGuid().ToString();
			PlayerPrefs.SetString(clientTokenPrefKey, newToken);
			PlayerPrefs.Save();
			return newToken;
		}

		return PlayerPrefs.GetString(clientTokenPrefKey);
	}

	/// <summary>
	/// Logs client into a 
	/// </summary>
	/// <param name="username"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	public static IEnumerator Login(string username, string password, Action<AccountStatus> callback)
	{
		// send login request to server
		var task = MakeRequest<RefreshResponse>(LoginServer + "/authenticate", JsonUtility.ToJson(new AuthenticatePayload(username, password)), (responseData) =>
		{
			callback(HandleRefreshResponse(responseData));
		});

		while (task.MoveNext())
			yield return null;
	}

	/// <summary>
	/// Gets the stored login status of the user
	/// </summary>
	/// <returns></returns>
	public static IEnumerator GetLoginStatus(Action<AccountStatus> callback)
	{
		if (string.IsNullOrEmpty(AccessToken))
		{
			callback(AccountStatus.LOGGED_OUT);
			yield break;
		}

		// refresh access token
		var task = MakeRequest<RefreshResponse>(LoginServer + "/refresh", JsonUtility.ToJson(new RefreshPayload(AccessToken)), responseData =>
		{
			callback(HandleRefreshResponse(responseData));
		});

		// wait for web request to complete
		while (task.MoveNext())
			yield return null;
	}

	/// <summary>
	/// Invalidates a specified access token, or the stored one if no token is specified
	/// </summary>
	/// <param name="token"></param>
	public static IEnumerator InvalidateAccessToken(string token = null)
	{
		var task = MakeRequest<object>(LoginServer + "/invalidate", JsonUtility.ToJson(new InvalidatePayload(token ?? AccessToken)), responseData =>
		{
			// we don't need to do anything here
		});
		while (task.MoveNext())
			yield return null;
	}

	/// <summary>
	/// Sends a request to the joinserver and gets the response
	/// </summary>
	/// <param name="hash"></param>
	public static bool JoinServer(string hash)
	{
		var payload = new JoinServerPayload(AccessToken, UUID.ToString("N"), hash);
		bool success = false;

		var task = MakeRequest<object>("https://sessionserver.mojang.com/session/minecraft/join", JsonUtility.ToJson(payload), (data) =>
		{
			// expecting an error because server sends a 204 response
			if (data.ResponseCode != 204)
			{
				// an actual error occured
				Debug.LogError($"Invalid response from session server: {data.ErrorData}");
				success = false;
			}
			else
			{
				success = true;
			}
		});

		while (task.MoveNext())
		{ }

		return success;
	}

	/// <summary>
	/// Checks a refresh response and tells us the status of it
	/// </summary>
	/// <param name="responseData"></param>
	/// <returns></returns>
	private static AccountStatus HandleRefreshResponse(ResponseData responseData)
	{
		// check if response is errored
		if (responseData.ErrorData != null)
		{
			// check if token invalid
			if (responseData.ErrorData.error == "ForbiddenOperationException")
			{
				AccessToken = null; // clear access token because the one we have is invalid
				return AccountStatus.INVALID_CREDENTIALS;
			}

			// misc server error
			throw new Exception($"Error validating token: {responseData.ErrorData}");
		}

		// get body from response data
		var responseBody = (RefreshResponse)responseData.ResponseObject;
		Username = responseBody.selectedProfile.name;
		UUID = Guid.Parse(responseBody.selectedProfile.id);
		AccountStatus status;

		// check if user is premium
		if (responseBody.selectedProfile == null)
		{
			status = AccountStatus.NOT_PREMIUM;
		}
		else
		{
			status = AccountStatus.LOGGED_IN;
			AccessToken = responseBody.accessToken; // store access token
		}

		return status;
	}

	/// <summary>
	/// Posts the data and returns the response.
	/// </summary>
	/// <param name="endpoint">The endpoint to post to</param>
	/// <param name="jsonData"></param>
	/// <typeparam name="T">The type to deserialze the response into</typeparam>
	/// <returns></returns>
	private static IEnumerator MakeRequest<T>(string endpoint, string jsonData, Action<ResponseData> callback)
	{
		var request = new UnityWebRequest($"{endpoint}", "POST")
		{
			timeout = 30
		};

		request.SetRequestHeader("content-type", "application/json");
		request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
		request.downloadHandler = new DownloadHandlerBuffer();
		var operation = request.SendWebRequest();
		while (!operation.isDone)
			yield return null;

		// check if request failed
		if (request.responseCode != 200)
		{
			// try to parse server error json
			ServerErrorResponse error;
			try
			{
				error = JsonUtility.FromJson<ServerErrorResponse>(request.downloadHandler.text);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Exception deserializing auth server error:\n{ex}\nBody:\n{request.downloadHandler.text}");
				error = new ServerErrorResponse(ex);
			}

			callback(new ResponseData() { ErrorData = error, ResponseCode = request.responseCode });
		}
		else
		{
			// attempt to parse response
			try
			{
				T responseObject = JsonUtility.FromJson<T>(request.downloadHandler.text);
				callback(new ResponseData(responseObject));
			}
			catch (Exception ex)
			{
				// error parsing response
				Debug.LogWarning($"Exception deserializing server response:\n{ex}\nBody:\n{request.downloadHandler.text}");
				callback(new ResponseData() { ErrorData = new ServerErrorResponse(ex) });
			}
		}
	}

	/// <summary>
	/// Determines a user's login status
	/// </summary>
	public enum AccountStatus
	{
		LOGGED_IN,
		LOGGED_OUT,
		NOT_PREMIUM,
		INVALID_CREDENTIALS
	}
}

class ResponseData
{
	public ResponseData()
	{ }

	public ResponseData(object boxedResponse)
	{
		ResponseObject = boxedResponse;
	}

	/// <summary>
	/// The error this response contains
	/// </summary>
	public ServerErrorResponse ErrorData { get; set; } = null;

	/// <summary>
	/// A boxed version of the response object
	/// </summary>
	public object ResponseObject { get; set; }

	/// <summary>
	/// The response code sent by the server
	/// </summary>
	public long ResponseCode { get; set; }
}
