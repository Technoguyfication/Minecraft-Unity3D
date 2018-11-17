using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using System.Threading;

public class GameManager : MonoBehaviour
{

	[Header("Prefabs")]
	public GameObject PlayerPrefab;

	public string Username = "mcplayer";

	private NetworkClient _client;
	private List<Packet> _packetSendQueue = new List<Packet>();
	private List<PacketData> _packetReceiveQueue = new List<PacketData>();
	private Task _netReadTask;
	private Task _netWriteTask;
	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
	private bool _initialized = false;
	private World _currentWorld;
	private string _playerUuid;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		// this only happens if we are actively connected to a server
		if (_initialized)
		{
			// make sure we aren't disconnected
			if (_netReadTask.IsFaulted)
			{
				Disconnect(_netReadTask.Exception.Message);
				return;
			}
			if (_netWriteTask.IsFaulted)
			{
				Disconnect(_netWriteTask.Exception.Message);
				return;
			}
			if (!_client.Connected)
			{
				Disconnect("Network disconnected");
				return;
			}

			// handle packets in queue
			lock (_packetReceiveQueue)
			{
				foreach (PacketData data in _packetReceiveQueue)
				{
					HandlePacket(data);
				}
			}
		}
	}

	public void Disconnect(string reason)
	{
		_cancellationTokenSource.Cancel();
		_client.Disconnect();
		_initialized = false;

		Debug.Log($"Disconnected: {reason}");
	}

	/// <summary>
	/// Queues up a packet to write to the server
	/// </summary>
	private void DispatchWritePacket(Packet packet)
	{
		lock (_packetSendQueue)
		{
			_packetSendQueue.Add(packet);
		}
	}

	/// <summary>
	/// Signals the game manager to connect to a server
	/// </summary>
	public IEnumerator ConnectToServerCoroutine(string hostname, int port)
	{
		// open loading screen
		AsyncOperation loadTask = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);
		while (!loadTask.isDone)
			yield return null;

		SceneManager.SetActiveScene(SceneManager.GetSceneByName("LoadingScreen"));
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("LoadingScreen"));
		AsyncOperation unloadTask = SceneManager.UnloadSceneAsync("MainMenu");

		while (!unloadTask.isDone)
			yield return null;

		// find loading screen controller
		LoadingScreenController controller = GameObject.FindGameObjectWithTag("Loading Screen Controller").GetComponent<LoadingScreenController>();
		controller.UpdateSubtitleText($"Attempting to connect to {hostname}:{port}");

		// set up network client
		_client = new NetworkClient();
		Task connectTask = new Task(() =>
		{
			_client.Connect(hostname, port);
		});
		connectTask.Start();

		while (!connectTask.IsCompleted)
			yield return null;

		if (connectTask.IsFaulted || !_client.Connected)
			throw (Exception)connectTask.Exception ?? new UnityException("Disconnected from server");

		controller.UpdateSubtitleText("Connection established. Asking server nicely to let us play.");

		// perform handshake/login
		LoginSuccessPacket loginSuccess = null;
		JoinGamePacket joinGame = null;
		// TODO: put the rest of the login packets here
		// player abilities, item slot change, etc.

		Task loginTask = new Task(() =>
		{
			// send off packets
			_client.WritePackets(new Packet[] {
				new HandshakePacket()
				{
					Address = hostname,
					Port = port,
					NextState = NetworkClient.ProtocolState.LOGIN
				},
				new LoginStartPacket()
				{
					Username = Username
				}
			});

			// get response
			while (true)
			{
				var packet = _client.ReadNextPacket();
				switch ((ClientboundIDs)packet.ID)
				{
					case ClientboundIDs.LOGIN_SUCCESS:
						loginSuccess = new LoginSuccessPacket(packet);
						break;
					case ClientboundIDs.JOIN_GAME:
						joinGame = new JoinGamePacket(packet);
						break;
					case ClientboundIDs.DISCONNECT:
						throw new Exception($"Disconnected from server: {new DisconnectPacket(packet).JSONResponse}");
				}

				if (loginSuccess != null && joinGame != null)
					break;
			}
		});
		loginTask.Start();

		// wait till completion
		while (!loginTask.IsCompleted)
			yield return null;

		if (loginTask.IsFaulted)
			throw loginTask.Exception;

		// set settings from server
		_playerUuid = loginSuccess.UUID;
		_currentWorld = new World()
		{
			Dimension = joinGame.Dimension,
		};

		controller.UpdateSubtitleText("Downloading terrain...");

		// start network worker tasks
		_netReadTask = new Task(() =>
		{
			NetworkReadWorker(_cancellationTokenSource.Token);
		}, _cancellationTokenSource.Token);
		_netReadTask.Start();

		_netWriteTask = new Task(() =>
		{
			NetworkWriteWorker(_cancellationTokenSource.Token);
		}, _cancellationTokenSource.Token);
		_netWriteTask.Start();
	}

	/// <summary>
	/// Handles a packet from the server
	/// </summary>
	/// <param name="data"></param>
	private void HandlePacket(PacketData data)
	{
		switch ((ClientboundIDs)data.ID)
		{
			case ClientboundIDs.DISCONNECT:
				Disconnect(new DisconnectPacket(data).JSONResponse);
				break;
			case ClientboundIDs.KEEP_ALIVE:
				DispatchWritePacket(new ServerKeepAlivePacket() { Payload = data.Payload });
				break;
		}
	}

	private void NetworkReadWorker(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			lock (_packetReceiveQueue)
			{
				PacketData data;
				try
				{
					data = _client.ReadNextPacket();
				}
				catch (Exception)
				{
					// if the thread is supposed to be cancelled, regard all errors as by-products of the socket closing
					if (ct.IsCancellationRequested)
						return;
					else
						throw;
				}
				_packetReceiveQueue.Add(data);
			}
		}
	}

	private void NetworkWriteWorker(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			lock (_packetSendQueue)
			{
				try
				{
					// send all packets in queue
					_client.WritePackets(_packetSendQueue);
					_packetSendQueue.Clear();
				}
				catch (Exception)
				{
					// if the thread is supposed to be cancelled, regard all errors as by-products of the socket closing
					if (ct.IsCancellationRequested)
						return;
					else
						throw;
				}
			}
		}
	}
}

public enum GameMode : int
{
	SURVIVAL = 0,
	CREATIVE = 1,
	ADVENTURE = 2,
	SPECTATOR = 3
}

public enum Difficulty : int
{
	PEACEFUL = 0,
	EASY = 1,
	MEDIUM = 2,
	HARD = 3
}

public enum LevelType
{
	DEFAULT,
	FLAT,
	LARGE_BIOMES,
	AMPLIFIED,
	DEFAULT_1_1
}
