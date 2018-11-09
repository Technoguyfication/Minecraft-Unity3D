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
		using (NetworkClient client = new NetworkClient())
		{
			client.Connect(address, port);

			Packet[] packets = new Packet[] {
				new HandshakePacket()
				{
					Address = address,
					Port = (ushort)port,
					NextState = NetworkClient.ProtocolState.STATUS
				},
				new RequestPacket()
			};

			client.WritePackets(packets);
			// state has been set to status
			client.State = NetworkClient.ProtocolState.STATUS;

			var packetData = client.ReadNextPacket();
			var response = new ResponsePacket(packetData);

			Debug.Log(response);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
