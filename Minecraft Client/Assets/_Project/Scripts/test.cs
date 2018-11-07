using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		NetworkClient client = new NetworkClient();
		client.Client.Connect("localhost", 25565);

		List<byte> payload = new List<byte>();
		payload.AddRange(VarInt.GetBytes(404));
		byte[] stringRaw = Encoding.Unicode.GetBytes("127.0.0.1");
		payload.AddRange(VarInt.GetBytes(stringRaw.Length));
		payload.AddRange(stringRaw);
		payload.AddRange(BitConverter.GetBytes((short)25565).ReverseIfLittleEndian());
		payload.AddRange(VarInt.GetBytes(1));

		Packet p = new Packet()
		{
			PacketID = 1,
			Payload = payload.ToArray()
		};

		Debug.Log(p);

		client.WritePacket(p);
		var _p = client.ReadNextPacket();

		Debug.Log(_p);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
