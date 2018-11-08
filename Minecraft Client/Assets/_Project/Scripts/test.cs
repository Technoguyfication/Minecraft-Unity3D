using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Test : MonoBehaviour
{

	public string address = "localhost";
	public int port = 25565;

	// Use this for initialization
	void Start()
	{
		NetworkClient client = new NetworkClient();
		client.Client.Connect(address, port);

		Packet[] packets = new Packet[] {
		new HandshakePacket()
		{
			Address = address,
			Port = (ushort)port,
			ProtocolVersion = 404,
			NextState = NetworkClient.ProtocolState.STATUS
		},
		new RequestPacket()
		};

		client.WritePackets(packets);
		// state has been set to status
		client.State = NetworkClient.ProtocolState.STATUS;

		var _p = client.ReadNextPacket();

		Debug.Log(_p);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
