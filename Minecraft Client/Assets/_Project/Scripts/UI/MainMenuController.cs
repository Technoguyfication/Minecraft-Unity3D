using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;

public class MainMenuController : MonoBehaviour
{
	[Header("Connect")]
	public InputField AddressInput;
	public InputField PortInput;
	[Header("Authentication")]
	public Image AuthStatusImage;
	public Sprite AuthStatusImageGood;
	public Color AuthStatusImageColorGood = Color.green;
	public Sprite AuthStatusImageBad;
	public Color AuthStatusImageColorBad = Color.red;
	public Sprite AuthStatusImageLoading;
	public Text AuthStatusText;
	public Button LoginButton;
	public Button LogoutButton;

	[Header("Other")]
	public GameManager GameManager;

	private bool _spinningLoginImage = false;
	private readonly float _loginImageSpinSpeed = 400f;

	// Use this for initialization
	void Start()
	{

	}

	private void Awake()
	{
		RefreshLoginStatus();
	}

	// Update is called once per frame
	void Update()
	{
		if (_spinningLoginImage)
		{
			AuthStatusImage.rectTransform.Rotate(new Vector3(0, 0, _loginImageSpinSpeed * Time.deltaTime));
		}
	}

	/// <summary>
	/// Refreshes the client's login status
	/// </summary>
	public void RefreshLoginStatus()
	{
		Debug.Log("Checking user login..");
		Debug.Log($"Client auth server token: {MojangAuthentication.GetClientToken()}");
		StartCoroutine(MojangAuthentication.GetLoginStatus((status) =>
		{
			// change ui based on status
			switch (status)
			{
				case MojangAuthentication.AccountStatus.LOGGED_IN:
					SetAuthImage(AuthImageStatus.VALID);
					AuthStatusText.text = $"Logged in as {MojangAuthentication.Username}";
					break;
				case MojangAuthentication.AccountStatus.LOGGED_OUT:
				case MojangAuthentication.AccountStatus.INVALID_CREDENTIALS:
					SetAuthImage(AuthImageStatus.INVALID);
					AuthStatusText.text = "Not Logged In";
					break;
				case MojangAuthentication.AccountStatus.NOT_PREMIUM:
					SetAuthImage(AuthImageStatus.INVALID);
					AuthStatusText.text = $"Logged in as {MojangAuthentication.Username}\nAccount not premium.";
					break;
			}
		}));
	}

	private void SetAuthImage(AuthImageStatus status)
	{
		switch (status)
		{
			case AuthImageStatus.VALID:
				_spinningLoginImage = false;
				AuthStatusImage.rectTransform.rotation = Quaternion.identity;
				AuthStatusImage.sprite = AuthStatusImageGood;
				AuthStatusImage.color = AuthStatusImageColorGood;
				break;
			case AuthImageStatus.INVALID:
				_spinningLoginImage = false;
				AuthStatusImage.rectTransform.rotation = Quaternion.identity;
				AuthStatusImage.sprite = AuthStatusImageBad;
				AuthStatusImage.color = AuthStatusImageColorBad;
				break;
			case AuthImageStatus.LOADING:
				_spinningLoginImage = true;
				AuthStatusImage.sprite = AuthStatusImageLoading;
				AuthStatusImage.color = Color.black;
				break;
		}
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

	public void ConnectToServer()
	{
		string hostname = AddressInput.text;
		int port = int.Parse(PortInput.text);
		Debug.Log("Starting game load coroutine");
		StartCoroutine(GameManager.ConnectToServerCoroutine(hostname, port));
	}

	private IEnumerator QueryServerCoroutine(string hostname, int port)
	{
		using (var client = new NetworkClient())
		{
			var connectTask = new Task(() =>
			{
				client.Connect(hostname, port);
			});

			connectTask.Start();

			// wait until server is connected
			while (!connectTask.IsCompleted)
				yield return null;

			// make sure connection is successful
			if (connectTask.IsFaulted)
			{
				if (connectTask.IsFaulted || !client.Connected)
				{
					throw connectTask.Exception;
				}
			}

			// create handshake to send to server
			Packet[] handshakePackets = new Packet[]
				{
					new HandshakePacket()
					{
						 Address = hostname,
						 Port = port,
						 NextState = NetworkClient.ProtocolState.STATUS
					},
					new RequestPacket()
				};

			ServerStatus status = new ServerStatus();

			var retreiveStatusTask = new Task(() =>
			{
				// write handshake packets to server
				client.WritePackets(handshakePackets);

				// get server data
				ResponsePacket responsePacket;
				while (true)
				{
					PacketData responsePacketData = client.ReadNextPacket();

					// check if it's a response packet
					if (responsePacketData.ID == 0x00)
					{
						responsePacket = new ResponsePacket(responsePacketData);
						break;
					}
				}

				// send ping
				var sw = new Stopwatch();
				sw.Start();
				client.WritePacket(new PingPongPacket());

				// wait for pong packet
				while (true)
					if (client.ReadNextPacket().ID == 0x01) break;

				sw.Stop();

				// set server status so the task can end
				status = new ServerStatus()
				{
					Packet = responsePacket,
					PingTime = (int)sw.ElapsedMilliseconds
				};
			});

			retreiveStatusTask.Start();

			while (!retreiveStatusTask.IsCompleted)
				yield return null;

			if (retreiveStatusTask.IsFaulted)
				throw retreiveStatusTask.Exception;

			Debug.Log($"Response: {status.Packet.JSONResponse}\nPing: {status.PingTime}ms");
		}
	}

	private struct ServerStatus
	{
		/// <summary>
		/// The response from the server
		/// </summary>
		public ResponsePacket Packet;

		/// <summary>
		/// The time in ms it took to ping the server
		/// </summary>
		public int PingTime;
	}

	enum AuthImageStatus
	{
		VALID,
		INVALID,
		LOADING
	}
}
