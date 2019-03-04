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
}

/// <summary>
/// States are assumed to be PLAY unless prepended e.x. STATUS_RESPONSE vs CHUNK_DATA
/// </summary>
public enum ClientboundIDs : int
{
	// status
	STATUS_RESPONSE = 0x00,
	STATUS_PONG = 0x01,

	// login
	LOGIN_DISCONNECT = 0x00,
	LOGIN_ENCRYPTION_REQUEST = 0x01,
	LOGIN_SUCCESS = 0x02,
	LOGIN_SET_COMPRESSION = 0x03,

	// play
	CHUNK_DATA = 0x22,
	UNLOAD_CHUNK = 0x1f,
	KEEP_ALIVE = 0x21,
	JOIN_GAME = 0x25,
	DISCONNECT = 0x1b,
	PLAYER_POSITION_AND_LOOK = 0x32,
	ENTITY = 0x27,
	SPAWN_MOB = 0x03,
}

public enum ServerboundIDs : int
{
	// status
	STATUS_REQUEST = 0x00,
	STATUS_PING = 0x01,

	// login
	LOGIN_START = 0x00,
	LOGIN_ENCRYPTION_RESPONSE = 0x01,

	// play
	KEEP_ALIVE = 0x0E,
	PLAYER = 0x0F,
	PLAYER_POSITION_AND_LOOK = 0x11,

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
