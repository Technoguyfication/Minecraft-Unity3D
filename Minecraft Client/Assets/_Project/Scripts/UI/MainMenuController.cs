using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class MainMenuController : MonoBehaviour
{
	private const string ADDRESS_PREF_KEY = "lastAddressInput";
	private const string PORT_PREF_KEY = "lastPortInput";

	[Header("Connect")]
	public InputField AddressInput;
	public InputField PortInput;
	[Header("Authentication Window")]
	public GameObject LoginWindow;
	public InputField EmailInput;
	public InputField PasswordInput;
	public Button WindowLoginButton;
	[Header("Authentication Widget")]
	public Image AuthStatusImage;
	public Sprite AuthStatusImageGood;
	public Color AuthStatusImageColorGood = Color.green;
	public Sprite AuthStatusImageBad;
	public Color AuthStatusImageColorBad = Color.red;
	public Sprite AuthStatusImageLoading;
	public Text AuthStatusText;
	public Button WidgetLoginButton;
	public Button LogoutButton;

	[Header("Other")]
	public GameManager GameManager;
	public GameObject ModalDimmer;

	private bool _spinningLoginImage = false;
	private readonly float _loginImageSpinSpeed = 200f;

	// Use this for initialization
	void Start()
	{
		// set hostname and port textbox to last used
		// todo: server list page
		AddressInput.text = PlayerPrefs.GetString(ADDRESS_PREF_KEY, AddressInput.text);
		PortInput.text = PlayerPrefs.GetString(PORT_PREF_KEY, PortInput.text);
	}

	private void Awake()
	{
		RefreshLoginStatus();
		RefreshLoginButton();
	}

	// Update is called once per frame
	void Update()
	{
		// tab skip implementation
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
			Selectable next;

			if (Input.GetKey(KeyCode.LeftShift))
			{
				// inverted direction
				next = current.FindSelectableOnUp();
			}
			else
			{
				next = current.FindSelectableOnDown();
			}

			if (next != null)
			{
				// check if next object is an inputfield
				var inputField = next.GetComponent<InputField>();
				if (inputField != null)
				{
					// select text caret
					inputField.OnPointerClick(new PointerEventData(EventSystem.current));
				}

				EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
			}
		}

		// spin spinner on login widget
		if (_spinningLoginImage)
		{
			AuthStatusImage.rectTransform.Rotate(new Vector3(0, 0, -_loginImageSpinSpeed * Time.deltaTime));
		}
	}

	/// <summary>
	/// Opens the login window
	/// </summary>
	public void OpenModalLoginWindow()
	{
		LoginWindow.SetActive(true);
		ModalDimmer.SetActive(true);
	}

	/// <summary>
	/// Closes the login window
	/// </summary>
	public void CloseModalLoginWindow()
	{
		PasswordInput.text = string.Empty;  // clear login password
		LoginWindow.SetActive(false);
		ModalDimmer.SetActive(false);
	}

	/// <summary>
	/// Checks if username and password are filled, and activates login button
	/// </summary>
	public void RefreshLoginButton()
	{
		WindowLoginButton.interactable = !string.IsNullOrEmpty(EmailInput.text) && !string.IsNullOrEmpty(PasswordInput.text);
	}

	/// <summary>
	/// Logs the user in using the filled-in username and password
	/// </summary>
	public void LoginUser()
	{
		string username = EmailInput.text;
		string password = PasswordInput.text;

		// check that username and password are filled out
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			return;
		}

		SetAuthImage(AuthImageStatus.LOADING);
		StartCoroutine(MojangAPI.Login(username, password, HandleLoginResponse));
	}

	/// <summary>
	/// Refreshes the client's login status
	/// </summary>
	public void RefreshLoginStatus()
	{
		Debug.Log("Checking user login..");
		Debug.Log($"Client auth server token: {MojangAPI.GetClientToken()}");

		SetAuthImage(AuthImageStatus.LOADING);
		StartCoroutine(MojangAPI.GetLoginStatus(HandleLoginResponse));
	}

	private void HandleLoginResponse(MojangAPI.AccountStatus status)
	{
		switch (status)
		{
			case MojangAPI.AccountStatus.LOGGED_IN:
				SetAuthImage(AuthImageStatus.VALID);
				AuthStatusText.text = $"Logged in as {MojangAPI.Username}";
				SetLoginLogoutButtons(true);
				GameManager.Username = MojangAPI.Username;
				break;
			case MojangAPI.AccountStatus.LOGGED_OUT:
			case MojangAPI.AccountStatus.INVALID_CREDENTIALS:
				SetAuthImage(AuthImageStatus.INVALID);
				AuthStatusText.text = "Not Logged In";
				SetLoginLogoutButtons(false);
				break;
			case MojangAPI.AccountStatus.NOT_PREMIUM:
				SetAuthImage(AuthImageStatus.INVALID);
				AuthStatusText.text = $"Logged in as {MojangAPI.Username}\nAccount not premium.";
				SetLoginLogoutButtons(false);
				break;
		}
	}

	/// <summary>
	/// Sets the state of the login/logout buttons in the auth widget depending on whether the user is logged in
	/// </summary>
	/// <param name="loggedIn">Whether the user is logged in</param>
	private void SetLoginLogoutButtons(bool loggedIn)
	{
		WidgetLoginButton.interactable = !loggedIn;
		LogoutButton.interactable = loggedIn;
	}

	/// <summary>
	/// Sets the image displayed on the auth widget
	/// </summary>
	/// <param name="status"></param>
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

	/// <summary>
	/// Connects to the server specified in the connect box
	/// </summary>
	public void ConnectToServer()
	{
		string hostname = AddressInput.text;
		int port = int.Parse(PortInput.text);
		PlayerPrefs.SetString(ADDRESS_PREF_KEY, AddressInput.text);
		PlayerPrefs.SetString(PORT_PREF_KEY, PortInput.text);
		Debug.Log("Starting game load coroutine");
		StartCoroutine(GameManager.ConnectToServerCoroutine(hostname, port));

		enabled = false;    // close main menu
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

	private enum AuthImageStatus
	{
		VALID,
		INVALID,
		LOADING
	}
}
