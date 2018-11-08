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

		var p = new HandshakePacket()
		{
			Address = address,
			Port = (ushort)port,
			ProtocolVersion = 404,
			NextState = HandshakePacket.NextStateEnum.STATUS
		};

		Debug.Log(p);

		client.WritePacket(p);
		var _p = client.ReadNextPacket();

		Debug.Log(_p);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
