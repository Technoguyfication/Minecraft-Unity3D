using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LoginTester : MonoBehaviour
{
	public string hostname;
	public ushort port;
	public string username;

	public GameObject[] meshTest;

	int frames = 0;

	World world = new World()
	{
		Dimension = World.DimensionType.OVERWORLD
	};

	// Use this for initialization
	void Start()
	{
		using (NetworkClient client = new NetworkClient())
		{
			client.Connect(hostname, port);

			client.WritePacket(new HandshakePacket()
			{
				Address = hostname,
				Port = port,
				NextState = NetworkClient.ProtocolState.LOGIN,
			});
			client.WritePacket(new LoginStartPacket()
			{
				Username = username
			});

			new System.Threading.Tasks.Task(() =>
			{
				System.Threading.Thread.Sleep(6000);
				client.Disconnect();
			}).Start();

			while (true)
			{
				PacketData p;
				try
				{
					p = client.ReadNextPacket();
				}
				catch (Exception)
				{ break; }

				if (p.ID == 0x22)
				{
					var packet = new ChunkDataPacket(p);
					Debug.Log($"Loading chunk at ({packet.Position.X}, {packet.Position.Z}): Ground up continuous: {packet.GroundUpContinuous}, Primary Bitmask: {packet.PrimaryBitmask}, Data length: {packet.Data.Length}");
					var chunk = new Chunk(packet, world);
					world._chunks.Add(chunk);
				}

				Debug.Log($"ID: {p.ID.ToString("X")} Length: {p.Payload.Length}");
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (frames != 5)
		{
			frames++;
			return;
		}

		frames++;

		for (int i = 0; i < meshTest.Length; i++)
		{
			Chunk c = world.GetChunk(new ChunkPos() { X = -22, Z = (-7 + i) });
			//meshTest[i].GetComponent<ChunkMesh>().GenerateMesh(c);
		}
	}
}
