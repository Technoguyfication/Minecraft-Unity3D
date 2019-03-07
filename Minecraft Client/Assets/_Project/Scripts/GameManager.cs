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
	public GameObject ChunkRendererPrefab;

	public string Username = "mcplayer";
	public DebugCanvas DebugCanvas;
	public EntityManager EntityManager;

	private PlayerController _player = null;
	private NetworkClient _client;
	private BlockingCollection<Packet> _packetSendQueue = new BlockingCollection<Packet>();
	private List<PacketData> _packetReceiveQueue = new List<PacketData>();
	private Task _netReadTask;
	private Task _netWriteTask;
	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
	private bool _initialized = false;
	private World _currentWorld
	{
		get
		{
			return _currentWorldVar;
		}
		set
		{
			_currentWorldVar = value;
			EntityManager.World = _currentWorld;
		}
	}
	private World _currentWorldVar;
	private Guid _playerUuid;
	private LoadingScreenController _loadingScreen;
	private float _lastTick = 0f;
	private bool _disconnecting = false;

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

	private void OnDestroy()
	{
		Disconnect("Game stopped");
		_client?.Dispose();
	}

	public void Disconnect(string reason)
	{
		if (_disconnecting)
			return;

		_disconnecting = true;
		_cancellationTokenSource.Cancel();
		_client?.Disconnect(reason);
		_initialized = false;

		Debug.Log($"Disconnected: {reason}");
	}

	/// <summary>
	/// Queues up a packet to write to the server
	/// </summary>
	private void DispatchWritePacket(Packet packet)
	{
		DebugCanvas.TxPackets++;
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
		_disconnecting = false;
		_client = new NetworkClient();
		_client.Disconnected += ClientDisconnectedEventHandler;
		Task connectTask = new Task(() =>
		{
			_client.Connect(hostname, port);
		});
		connectTask.Start();

		while (!connectTask.IsCompleted)
			yield return null;

		if (connectTask.IsFaulted || !_client.Connected)
			throw (Exception)connectTask.Exception ?? new UnityException("Connection failed: Unknown");

		_loadingScreen.UpdateSubtitleText("Connection established. Logging in...");

		// perform handshake/login
		LoginSuccessPacket loginSuccess = null;
		JoinGamePacket joinGame = null;
		// TODO: put the rest of the login packets here
		// player abilities, item slot change, etc.

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

		_client.State = NetworkClient.ProtocolState.LOGIN;

		// get login response
		while (true)
		{
			try
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
					case ClientboundIDs.LOGIN_DISCONNECT:
						Disconnect($"Disconnected from server: {new DisconnectPacket(packet).JSONResponse}");
						yield break;
				}
			}
			catch (Exception e)
			{
				Disconnect($"Failed to login: {e.Message}");
				yield break;
			}

			if (loginSuccess != null && joinGame != null)
			{
				_client.State = NetworkClient.ProtocolState.PLAY;
				break;
			}
		}

		_loadingScreen.UpdateSubtitleText("Downloading world...");

		// set settings from server
		_playerUuid = loginSuccess.UUID;
		_currentWorld = new World(DebugCanvas)
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
		_currentWorld.ChunkRenderer = Instantiate(ChunkRendererPrefab, Vector3.zero, Quaternion.identity).GetComponent<ChunkRenderer>();
		_currentWorld.ChunkRenderer.DebugCanvas = DebugCanvas;

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

	private void ClientDisconnectedEventHandler(object sender, DisconnectedEventArgs e)
	{
		Disconnect(e.Reason);
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
				break;
			case ClientboundIDs.CHUNK_DATA:
				_currentWorld.AddChunkData(new ChunkDataPacket(data));
				break;
			case ClientboundIDs.PLAYER_POSITION_AND_LOOK:
				HandlePositionAndLook(new ClientPlayerPositionAndLookPacket(data));
				break;
			case ClientboundIDs.UNLOAD_CHUNK:
				_currentWorld.UnloadChunk(new UnloadChunkPacket(data).Position);
				break;
			case ClientboundIDs.SPAWN_MOB:
				EntityManager.HandleSpawnMobPacket(new SpawnMobPacket(data));
				break;
			case ClientboundIDs.DESTROY_ENTITIES:
				EntityManager.DestroyEntities(new DestroyEntitiesPacket(data).EntityIDs);
				break;
			case ClientboundIDs.ENTITY_RELATIVE_MOVE:
				EntityManager.HandleEntityRelativeMovePacket(new EntityRelativeMovePacket(data));
				break;
			case ClientboundIDs.ENTITY_LOOK:
				EntityManager.HandleEntityLook(new EntityLookPacket(data));
				break;
			case ClientboundIDs.ENTITY_LOOK_AND_RELATIVE_MOVE:
				EntityManager.HandleEntityLookAndRelativeMovePacket(new EntityLookAndRelativeMovePacket(data));
				break;
			case ClientboundIDs.ENTITY_TELEPORT:
				EntityManager.HandleEntityTeleport(new EntityTeleportPacket(data));
				break;
			case ClientboundIDs.ENTITY_HEAD_LOOK:
				EntityManager.HandleEntityHeadLook(new EntityHeadLookPacket(data));
				break;
			default:
				break;
		}
	}

	private void HandlePositionAndLook(ClientPlayerPositionAndLookPacket packet)
	{
		// calculate absolute or relative position for player
		// https://wiki.vg/Protocol#Player_Position_And_Look_.28clientbound.29
		// ^ see flag table
		Vector3 pos = new Vector3(
			(float)packet.X + (((packet.Flags & 0x01) != 0) ? _player.MinecraftPosition.x : 0f),
			(float)packet.Y + (((packet.Flags & 0x02) != 0) ? _player.MinecraftPosition.y : 0f),
			(float)packet.Z + (((packet.Flags & 0x04) != 0) ? _player.MinecraftPosition.z : 0f));

		// check if we need to spawn the player for the first time
		if (_player == null)
		{
			_player = Instantiate(PlayerPrefab).GetComponent<PlayerController>();
			_player.World = _currentWorld;
			_player.UUID = _playerUuid;
			_player.OnGroundChanged += PlayerOnGroundChanged;
			_loadingScreen.HideLoadingScreen();
			DebugCanvas.Player = _player;
		}

		// update player position
		_player.MinecraftPosition = pos;
		_player.SetRotation(packet.Pitch + (((packet.Flags & 0x10) != 0) ? _player.Pitch : 0), packet.Yaw + (((packet.Flags & 0x08) != 0) ? _player.Yaw : 0) + 90);

		Debug.Log($"Moved player to {pos}, {_player.transform.rotation.eulerAngles}");

		DispatchWritePacket(new TeleportConfirmPacket()
		{
			TeleportID = packet.TeleportID
		});
	}

	private void PlayerOnGroundChanged(object sender, OnGroundEventArgs e)
	{
		DispatchWritePacket(new ServerPlayerPacket() { OnGround = e.OnGround });
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
			while (_lastTick > (Time.unscaledTime - 0.05f))
				yield return null;

			DebugCanvas.TickCount++;

			// update player position
			// TODO: only update look / position if player has moved
			if (_player != null)
			{
				DispatchWritePacket(ServerPlayerPositionAndLookPacket.FromPlayer(_player));
			}

			// update tick time
			_lastTick = Time.unscaledTime;
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
			DebugCanvas.RxPackets++;
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
				foreach (Packet p in _packetSendQueue.GetConsumingEnumerable())
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
