using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Used for communicating with mojang's servers.
/// All methods are blocking
/// </summary>
public static class MojangAPI
{
	/* in this class we do some nasty stuff with IEnumerable and coroutines
	 * essentially, i didn't want to make this a MonoBehaviour, but I still
	 * want to use nested coroutines. I ended up using task.MoveNext() manually
	 * a lot, and it doesn't make a lot of sense until you realize this
	 * */

	public static string Username { get; private set; }

	private static Guid _uuid;
	private const string _loginServer = @"https://authserver.mojang.com";
	private const string _clientTokenPrefKey = "authClientToken";
	private const string _accessTokenPrefKey = "authAccessToken";

	/// <summary>
	/// The access token to be used by the client
	/// </summary>
	public static string AccessToken
	{
		get => PlayerPrefs.GetString(_accessTokenPrefKey, null);
		set
		{
			PlayerPrefs.SetString(_accessTokenPrefKey, value);
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// Retreives the semi-permanent client token for this client
	/// </summary>
	/// <returns></returns>
	public static string GetClientToken()
	{
		if (PlayerPrefs.HasKey(_clientTokenPrefKey))
		{
			return PlayerPrefs.GetString(_clientTokenPrefKey);
		}
		else
		{
			// generate a new access token
			string newToken = Guid.NewGuid().ToString();
			PlayerPrefs.SetString(_clientTokenPrefKey, newToken);
			PlayerPrefs.Save();
			return newToken;
		}
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
		var task = MakeRequest<RefreshResponse>(_loginServer + "/authenticate", JsonUtility.ToJson(new AuthenticatePayload(username, password)), (responseData) =>
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
		var task = MakeRequest<RefreshResponse>(_loginServer + "/refresh", JsonUtility.ToJson(new RefreshPayload(AccessToken)), responseData =>
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
		var task = MakeRequest<object>(_loginServer + "/invalidate", JsonUtility.ToJson(new InvalidatePayload(token ?? AccessToken)), responseData =>
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
		var payload = new JoinServerPayload(AccessToken, _uuid.ToString("N"), hash);
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
		_uuid = Guid.Parse(responseBody.selectedProfile.id);
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
	/// Gets the skin data for a player from a profile skin and cape response object
	/// https://wiki.vg/Mojang_API#UUID_-.3E_Profile_.2B_Skin.2FCape
	/// </summary>
	/// <param name="textureData"></param>
	/// <returns></returns>
	public static async Task<PlayerSkinData> GetPlayerSkin(JObject textureData)
	{
		string bodyUrl = textureData["BODY"]["url"].ToString();
		string capeUrl = string.Empty;

		// try and load cape url if it is included in JSON
		if (textureData.TryGetValue("CAPE", out JToken capeToken))
		{
			try
			{
				capeUrl = capeToken["url"].ToString();
			}
			catch (JsonException)
			{
				// malformed cape JSON can be ignored
				Debug.LogWarning($"Malformed cape JSON from server: {capeToken.ToString()}");
			}
		}

		var skinData = new PlayerSkinData();

		// get skin data
		using (var httpClient = new HttpClient())
		{
			// get body data
			var bodyResponse = await httpClient.GetAsync(bodyUrl);
			bodyResponse.EnsureSuccessStatusCode();
			byte[] bodyRawData = await bodyResponse.Content.ReadAsByteArrayAsync();
			skinData.Body = new Texture2D(0, 0);    // size doesn't matter since loading image will replace size
			skinData.Body.LoadImage(bodyRawData);

			// load cape if it exists
			if (!string.IsNullOrEmpty(capeUrl))
			{
				var capeResponse = await httpClient.GetAsync(capeUrl);
				capeResponse.EnsureSuccessStatusCode();
				byte[] capeRawData = await capeResponse.Content.ReadAsByteArrayAsync();
				skinData.Cape = new Texture2D(0, 0);
				skinData.Cape.LoadImage(capeRawData);
			}
		}

		return skinData;
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

			callback(new ResponseData(error, request.responseCode));
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
				callback(new ResponseData(new ServerErrorResponse(ex)));
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

/// <summary>
/// A really simple struct to return a boxed object and http response code 
/// </summary>
struct ResponseData
{
	public ResponseData(ServerErrorResponse errorData, long? responseCode = null)
	{
		ErrorData = errorData;
		ResponseObject = null;
		ResponseCode = responseCode;
	}

	public ResponseData(object boxedResponse)
	{
		ErrorData = null;
		ResponseObject = boxedResponse;
		ResponseCode = null;
	}

	/// <summary>
	/// The error this response contains
	/// </summary>
	public ServerErrorResponse ErrorData { get; }

	/// <summary>
	/// A boxed version of the response object
	/// </summary>
	public object ResponseObject { get; }

	/// <summary>
	/// The response code sent by the server
	/// </summary>
	public long? ResponseCode { get; }
}
