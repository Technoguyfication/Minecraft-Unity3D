using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
	public InputField AddressInput;
	public InputField PortInput;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// Queries the server in the connection box
	/// </summary>
	public void QueryServer()
	{
		string hostname = AddressInput.text;
		int port = int.Parse(PortInput.text);

		StartCoroutine(QueryServerCoroutine(hostname, port));

	}

	private IEnumerator QueryServerCoroutine(string hostname, int port)
	{
		using (var client = new NetworkClient())
		{
			client.StartConnect(hostname, port);

			// wait until server is connected
			while (!client.Connected)
				yield return null;


		}
	}
}
