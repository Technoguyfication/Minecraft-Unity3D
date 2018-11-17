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

	public GameObject meshTestPrefab;

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

			int chunks = 0;

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
					var chunk = new Chunk(packet, world);
					world._chunks.Add(chunk);
					if (chunks > 5)
						return;
					else
						chunks++;
				}

				//Debug.Log($"ID: {p.ID.ToString("X")} Length: {p.Payload.Length}");
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
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

		for (int i = 0; i < world._chunks.Count; i++)
		{
			Chunk c = world._chunks[i];
			var meshTest = Instantiate(meshTestPrefab);
			sw.Start();
			meshTest.GetComponent<ChunkMesh>().GenerateMesh(c);
			sw.Stop();
			Debug.Log($"Took {sw.ElapsedMilliseconds / 1000d}s to generate chunk at {c.Position}");
			meshTest.transform.position = new Vector3(c.Position.Z * 16, 0, c.Position.X * 16);
			meshTest.name = c.Position.ToString();
			sw.Reset();
		}
	}
}
