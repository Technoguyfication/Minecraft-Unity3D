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
				System.Threading.Thread.Sleep(5000);
				client.Disconnect();
			}).Start();

			List<Chunk> chunks = new List<Chunk>();

			while (true)
			{
				PacketData p;

				var world = new World()
				{
					Dimension = World.DimensionType.OVERWORLD
				};
				try
				{
					p = client.ReadNextPacket();
				}
				catch (Exception)
				{ break; }

				if (p.ID == 0x22)
				{
					var packet = new ChunkDataPacket(p);
					Debug.Log(packet);

					var chunk = new Chunk(packet, world);
					chunks.Add(chunk);

					Debug.Log(chunk);
				}
			}

			foreach (Chunk chunk in chunks)
			{
				// create cubes to represent chunk
				for (int x = 0; x < 16; x++)
				{
					for (int y = 60; y < 100; y++)
					{
						for (int z = 0; z < 16; z++)
						{
							Block blk = chunk.GetBlockAt(new BlockPos()
							{
								X = x,
								Y = y,
								Z = z
							});

							if (blk.Type != 0)
							{
								GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
								cube.transform.position = new Vector3(x + (chunk.Position.X * 16), y, z + (chunk.Position.Z * 16));
							}
						}
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
