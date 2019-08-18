using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
	public Chat Chat;
	public PlayerLibrary PlayerLibrary;

	private PlayerController _player = null;
	private NetworkClient _client;
	private readonly BlockingCollection<Packet> _packetSendQueue = new BlockingCollection<Packet>();
	private readonly List<PacketData> _packetReceiveQueue = new List<PacketData>();
	private Task _netReadTask;
	private Task _netWriteTask;
	private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
	private bool _initialized = false;
	private World CurrentWorld
	{
		get => _currentWorld;
		set
		{
			_currentWorld = value;
			EntityManager.World = CurrentWorld;
		}
	}
	private World _currentWorld;
	private Guid _playerUuid;
	private LoadingScreenController _loadingScreen;
	private float _lastTick = 0f;
	private bool _disconnecting = false;

	public void Awake()
	{
#if UNITY_EDITOR
		Debug.Log("Running in editor");
#endif
#if UNITY_STANDALONE
		Debug.Log("Running in standalone mode");
#endif
#if ENABLE_PROFILER
		Debug.Log("Unity profiler enabled");
#endif
	}

	// Update is called once per frame
	public void Update()
	{
		// this only happens if we are actively connected to a server
		if (_initialized)
		{
			// make sure we aren't disconnected
			if (_netReadTask?.IsFaulted ?? false)
			{
				Disconnect(_netReadTask.Exception.InnerExceptions[0].Message);
				return;
			}
			if (_netWriteTask?.IsFaulted ?? false)
			{
				Disconnect(_netWriteTask.Exception.InnerExceptions[0].Message);
				return;
			}
			if (!_client?.Connected ?? true)
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

	public void OnDestroy()
	{
		Disconnect("Game stopped");
	}

	public void Disconnect(string reason)
	{
		if (_disconnecting)
			return;

		_disconnecting = true;
		_cancellationTokenSource.Cancel();
		_client?.Disconnect(reason);
		_client?.Dispose();
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
		// disable main menu controller
		//GetComponent<MainMenuController>().enabled = false;

		// open loading screen
		AsyncOperation loadLoadingScreenTask = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);
		while (!loadLoadingScreenTask.isDone)
			yield return null;

		Debug.Log("Loading screen opened");

		SceneManager.SetActiveScene(SceneManager.GetSceneByName("LoadingScreen"));
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("LoadingScreen"));
		AsyncOperation unloadMainMenuTask = SceneManager.UnloadSceneAsync("MainMenu");

		while (!unloadMainMenuTask?.isDone ?? false)    // if main menu isn't loaded in the first place, asyncoperation is null
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
					case ClientboundIDs.LogIn_Success:
						loginSuccess = new LoginSuccessPacket(packet);
						break;
					case ClientboundIDs.JoinGame:
						joinGame = new JoinGamePacket(packet);
						break;
					case ClientboundIDs.LogIn_Disconnect:
						Disconnect($"Disconnected from server: {new DisconnectPacket(packet).JSONResponse}");
						yield break;
				}
			}
			catch (Exception ex)
			{
				Disconnect($"Failed to login: {ex}");
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
		CurrentWorld = new World()
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
		CurrentWorld.ChunkRenderer = Instantiate(ChunkRendererPrefab, Vector3.zero, Quaternion.identity).GetComponent<ChunkRenderer>();
		CurrentWorld.ChunkRenderer.DebugCanvas = DebugCanvas;
		var referenceLinker = GameObject.FindGameObjectWithTag("ReferenceLinker").GetComponent<ReferenceLinker>();
		Chat = referenceLinker.Chat;
		Chat.ChatSend += Chat_ChatSend;
		PlayerLibrary = new PlayerLibrary(referenceLinker.PlayerList);

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

	private void Chat_ChatSend(object sender, Chat.ChatSendEventArgs e)
	{
		DispatchWritePacket(new CSChatMessagePacket()
		{
			Message = e.Message
		});
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
			case ClientboundIDs.Disconnect:
				Disconnect(new DisconnectPacket(data).JSONResponse);
				break;
			case ClientboundIDs.KeepAlive:
				HandleKeepAlive(new ClientKeepAlivePacket(data));
				break;
			case ClientboundIDs.ChunkData:
				StartCoroutine(CurrentWorld.AddChunkDataCoroutine(new ChunkDataPacket(data)));
				break;
			case ClientboundIDs.PlayerPositionAndLook:
				HandlePositionAndLook(new ClientPlayerPositionAndLookPacket(data));
				break;
			case ClientboundIDs.UnloadChunk:
				CurrentWorld.UnloadChunk(new UnloadChunkPacket(data).Position);
				break;
			case ClientboundIDs.SpawnMob:
				EntityManager.HandleSpawnMobPacket(new SpawnMobPacket(data));
				break;
			case ClientboundIDs.DestroyEntities:
				EntityManager.DestroyEntities(new DestroyEntitiesPacket(data).EntityIDs);
				break;
			case ClientboundIDs.EntityRelativeMove:
				EntityManager.HandleEntityRelativeMovePacket(new EntityRelativeMovePacket(data));
				break;
			case ClientboundIDs.EntityMove:
				EntityManager.HandleEntityLook(new EntityLookPacket(data));
				break;
			case ClientboundIDs.EntityLookAndRelativeMove:
				EntityManager.HandleEntityLookAndRelativeMovePacket(new EntityLookAndRelativeMovePacket(data));
				break;
			case ClientboundIDs.EntityTeleport:
				EntityManager.HandleEntityTeleport(new EntityTeleportPacket(data));
				break;
			case ClientboundIDs.EntityHeadLook:
				EntityManager.HandleEntityHeadLook(new EntityHeadLookPacket(data));
				break;
			case ClientboundIDs.PlayerInfo:
				PlayerLibrary.HandlePlayerInfoPacket(new PlayerInfoPacket(data));
				break;
			case ClientboundIDs.ChatMessage:
				Chat.HandleChatPacket(new ChatMessagePacket(data));
				break;
		}
	}

	private void HandleKeepAlive(ClientKeepAlivePacket pkt)
	{
		DispatchWritePacket(new ServerKeepAlivePacket()
		{
			KeepAliveID = pkt.KeepAliveID
		});
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
			_player.World = CurrentWorld;
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
