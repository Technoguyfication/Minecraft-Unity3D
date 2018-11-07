using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		NetworkClient client = new NetworkClient();
		client.Client.Connect("localhost", 25565);
		Packet p = new Packet()
		{
			PacketID = 1,
			Payload = BitConverter.GetBytes((long)45454532)
		};

		client.WritePacket(p);
		var _p = client.ReadNextPacket();
		Debug.Log(_p);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
