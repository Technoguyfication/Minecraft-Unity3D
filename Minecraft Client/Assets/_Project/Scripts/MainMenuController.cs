using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

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
						 Port = (ushort)port,
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
}
