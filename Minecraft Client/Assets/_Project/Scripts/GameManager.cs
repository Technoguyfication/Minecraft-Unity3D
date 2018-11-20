using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using System.Collections.Concurrent;

/// <summary>
/// Manages everything for a game, or connection to a server
/// </summary>
public class GameManager : MonoBehaviour
{

	[Header("Prefabs")]
	public GameObject PlayerPrefab;

	public string Username = "mcplayer";

	private PlayerController _player = null;
	private NetworkClient _client;
	private BlockingCollection<Packet> _packetSendQueue = new BlockingCollection<Packet>();
	private List<PacketData> _packetReceiveQueue = new List<PacketData>();
	private Task _netReadTask;
	private Task _netWriteTask;
	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
	private bool _initialized = false;
	private World _currentWorld;
	private string _playerUuid;
	private LoadingScreenController _loadingScreen;
	private int _lastTick = 0;

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
				foreach (var packet in _packetReceiveQueue)
				{
					HandlePacket(packet);
				}
				_packetReceiveQueue.Clear();
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
		_packetSendQueue.Add(packet);
	}

	/// <summary>
	/// Signals the game manager to connect to a server
	/// </summary>
	public IEnumerator ConnectToServerCoroutine(string hostname, int port)
	{
		// open loading screen
		AsyncOperation loadLoadingScreenTask = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);
		while (!loadLoadingScreenTask.isDone)
			yield return null;

		Debug.Log("Loading screen opened");

		SceneManager.SetActiveScene(SceneManager.GetSceneByName("LoadingScreen"));
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("LoadingScreen"));
		AsyncOperation unloadMainMenuTask = SceneManager.UnloadSceneAsync("MainMenu");

		while (!unloadMainMenuTask.isDone)
			yield return null;

		Debug.Log("Closed main menu");

		// find loading screen controller
		_loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen Controller").GetComponent<LoadingScreenController>();
		_loadingScreen.UpdateSubtitleText($"Attempting to connect to {hostname}:{port}");

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
			throw (Exception)connectTask.Exception ?? new UnityException("Connection failed: Unknown");

		_loadingScreen.UpdateSubtitleText("Connection established. Asking server nicely to let us play.");

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

		// open game scene
		AsyncOperation loadGameTask = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
		while (!loadGameTask.isDone)
			yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("Game"));

		// set up references in game scene
		_currentWorld.ChunkRenderer = GameObject.FindGameObjectWithTag("Chunk Renderer").GetComponent<ChunkRenderer>();

		_loadingScreen.UpdateSubtitleText("Downloading terrain...");

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

		_initialized = true;

		// start tick loop
		StartCoroutine(ClientTickLoopCoroutine(_cancellationTokenSource.Token));
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
				DispatchWritePacket(new ServerKeepAlivePacket() { KeepAliveID = new ClientKeepAlivePacket(data).KeepAliveID });
				Debug.Log($"Sending keep-alive: {BitConverter.ToInt64(data.Payload.ReverseIfLittleEndian(), 0)}");
				break;
			case ClientboundIDs.CHUNK_DATA:
				_currentWorld.AddChunkData(new ChunkDataPacket(data));
				break;
			case ClientboundIDs.PLAYER_POSITION_AND_LOOK:
				HandlePositionAndLook(new ClientPlayerPositionAndLookPacket(data));
				break;
		}
	}

	private void HandlePositionAndLook(ClientPlayerPositionAndLookPacket packet)
	{
		Vector3 pos = new Vector3((float)packet.Z, (float)packet.Y, (float)packet.X);
		Quaternion rot = Quaternion.Euler(packet.Pitch, packet.Yaw, 0);

		// check if we need to spawn the player for the first time
		if (_player == null)
		{
			_player = Instantiate(PlayerPrefab).GetComponent<PlayerController>();
			_loadingScreen.HideLoadingScreen();
		}

		// update player position
		_player.SetPosition(pos);
		_player.SetRotation(rot);

		Debug.Log($"Moved player to {pos}, {rot}");

		DispatchWritePacket(new TeleportConfirmPacket()
		{
			TeleportID = packet.TeleportID
		});
	}

	/// <summary>
	/// Runs 20 times per second for each tick
	/// </summary>
	/// <param name="ct"></param>
	private IEnumerator ClientTickLoopCoroutine(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			// wait for next tick
			while (_lastTick > (int)(Time.unscaledTime / 1000f) - 50)
				yield return null;

			// update player position
			// TODO: only update look / position if player has moved
			if (_player != null)
			{
				DispatchWritePacket(new ServerPlayerPositionAndLookPacket()
				{
					X = _player.X,
					FeetY = _player.FeetY,
					Z = _player.Z,
					Yaw = _player.Yaw,
					Pitch = _player.Pitch,
					OnGround = _player.OnGround
				});
			}

			// update tick time
			_lastTick = (int)(Time.unscaledTime / 1000f);
		}
	}

	private void NetworkReadWorker(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
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
			lock (_packetReceiveQueue)
			{
				_packetReceiveQueue.Add(data);
			}
		}
	}

	private void NetworkWriteWorker(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			try
			{
				// send all packets in queue
				foreach (Packet p in _packetSendQueue)
					_client.WritePacket(p);
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
