using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Packet
{
	public int Length
	{
		get
		{
			return (VarInt.GetBytes(PacketID).Length + Payload.Length);
		}
	}
	public int PacketID { get; set; }
	public abstract byte[] Payload { get; set; }

	public override string ToString()
	{
		return $"Length: {Length} ID: {PacketID} Data (raw): {BitConverter.ToString(Payload)}";
	}

	public static byte[] GetRaw(Packet p)
	{
		List<byte> builder = new List<byte>();
		builder.AddRange(VarInt.GetBytes(p.Length));
		builder.AddRange(VarInt.GetBytes(p.PacketID));
		builder.AddRange(p.Payload);

		return builder.ToArray();
	}
}
