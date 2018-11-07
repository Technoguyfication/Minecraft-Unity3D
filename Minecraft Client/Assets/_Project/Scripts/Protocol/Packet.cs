using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet
{
	public int Length
	{
		get
		{
			return (VarInt.GetBytes(PacketID).Length + Payload.Length);
		}
	}
	public int PacketID { get; set; }
	public byte[] Payload { get; set; }

	public byte[] Raw
	{
		get
		{
			List<byte> builder = new List<byte>();
			builder.AddRange(VarInt.GetBytes(Length));
			builder.AddRange(VarInt.GetBytes(PacketID));
			builder.AddRange(Payload);

			return builder.ToArray();
		}
	}

	public override string ToString()
	{
		return $"Length: {Length} ID: {PacketID} Data (raw): {BitConverter.ToString(Payload)}";
	}
}
