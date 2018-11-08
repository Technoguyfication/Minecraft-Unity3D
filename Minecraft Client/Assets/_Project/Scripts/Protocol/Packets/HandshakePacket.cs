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

	public override byte[] Payload
	{
		get
		{
			List<byte> builder = new List<byte>();
			builder.AddRange(VarInt.GetBytes(ProtocolVersion));
			builder.AddRange(TypeConverter.GetBytes(Address));
			builder.AddRange(BitConverter.GetBytes(Port).ReverseIfLittleEndian());
			builder.AddRange(VarInt.GetBytes((int)NextState));
			return builder.ToArray();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int ProtocolVersion { get; set; }
	public string Address { get; set; }
	public ushort Port { get; set; }
	public NetworkClient.ProtocolState NextState { get; set; }
}
