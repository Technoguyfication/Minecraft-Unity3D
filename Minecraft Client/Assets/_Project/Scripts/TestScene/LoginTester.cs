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

			var world = new World()
			{
				Dimension = World.DimensionType.OVERWORLD
			};

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
					if (packet.Position.X == -16 && packet.Position.Z == -13)
					{
						Debug.Log($"Loading chunk at ({packet.Position.X}, {packet.Position.Z}): Ground up continuous: {packet.GroundUpContinuous}, Primary Bitmask: {packet.PrimaryBitmask}, Data length: {packet.Data.Length}");
						var chunk = new Chunk(packet, world);

						// create cubes to represent chunk
						for (int x = 0; x < 16; x++)
						{
							for (int y = 65; y < 90; y++)
							{
								for (int z = 0; z < 16; z++)
								{
									Block blk = chunk.GetBlockAt(new BlockPos()
									{
										X = x,
										Y = y,
										Z = z
									});

									if ((blk.Type != Block.BlockType.AIR) && (blk.Type != Block.BlockType.CAVE_AIR))
									{
										GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
										cube.transform.position = new Vector3(x + (chunk.Position.X * 16), y, z + (chunk.Position.Z * 16));
										cube.name = ((int)blk.Type).ToString();

										switch (blk.Type)
										{
											case Block.BlockType.GRASS:
												cube.GetComponent<Renderer>().material.color = Color.green;
												break;
											case Block.BlockType.DIRT:
												cube.GetComponent<Renderer>().material.color = Color.yellow;
												break;
											case Block.BlockType.STONE:
											case Block.BlockType.DIORITE:
											case Block.BlockType.GRANITE:
												cube.GetComponent<Renderer>().material.color = Color.black;
												break;
											default:
												cube.GetComponent<Renderer>().material.color = Color.magenta;
												break;
										}
									}
								}
							}
						}

						break;
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
