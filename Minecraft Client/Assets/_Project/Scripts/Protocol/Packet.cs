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

	public Packet()
	{

	}

	/// <summary>
	/// Create a packet using a <see cref="PacketData"/> object
	/// </summary>
	/// <param name="data"></param>
	protected Packet(PacketData data)
	{
		PacketID = data.ID;
		Payload = data.Payload;
	}

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


[Serializable]
public class MalformedPacketException : Exception
{
	public MalformedPacketException() { }
	public MalformedPacketException(string message) : base(message) { }
	public MalformedPacketException(string message, Exception inner) : base(message, inner) { }
	protected MalformedPacketException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
