using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandshakePacket : Packet
{
	public HandshakePacket()
	{
		PacketID = 0x00;
	}

	public HandshakePacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get
		{
			List<byte> builder = new List<byte>();
			builder.AddRange(VarInt.GetBytes(ProtocolVersion));
			builder.AddRange(PacketHelper.GetBytes(Address));
			builder.AddRange(BitConverter.GetBytes((ushort)Port).ReverseIfLittleEndian());
			builder.AddRange(VarInt.GetBytes((int)NextState));
			return builder.ToArray();
		}
		set => throw new NotImplementedException();
	}

	public int ProtocolVersion { get; set; } = NetworkClient.PROTOCOL_VERSION;
	public string Address { get; set; }
	public int Port { get; set; }
	public NetworkClient.ProtocolState NextState { get; set; }
}
