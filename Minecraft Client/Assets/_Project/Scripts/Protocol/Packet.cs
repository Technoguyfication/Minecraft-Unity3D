using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Packet
{
	public int Length => (VarInt.GetBytes(PacketID).Length + Payload.Length);
	public int PacketID { get; set; }
	public abstract byte[] Payload { get; set; }

	public Packet() { }

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
	// Status
	Status_Response = 0x00,
	Status_Pong = 0x01,

	// Login
	LogIn_Disconnect = 0x00,
	LogIn_EncryptionRequest = 0x01,
	LogIn_Success = 0x02,
	LogIn_SetCompression = 0x03,

	// Play
	ChunkData = 0x22,
	UnloadChunk = 0x1f,
	KeepAlive = 0x21,
	JoinGame = 0x25,
	Disconnect = 0x1b,
	PlayerPositionAndLook = 0x32,
	Entity = 0x27,
	SpawnMob = 0x03,
	DestroyEntities = 0x35,
	EntityRelativeMove = 0x28,
	EntityLookAndRelativeMove = 0x29,
	EntityMove = 0x2A,
	EntityTeleport = 0x50,
	EntityHeadLook = 0x39,
	PlayerInfo = 0x30,
	EntityMetadata = 0x3F,
	ChatMessage = 0x0E
}

public enum ServerboundIDs : int
{
	// Status
	Status_Request = 0x00,
	Status_Ping = 0x01,

	// Login
	LogIn_Start = 0x00,
	LogIn_EncryptionResponse = 0x01,

	// Play
	TeleportConfirm = 0x00,
	KeepAlive = 0x0E,
	Player = 0x0F,
	PlayerPositionAndLook = 0x11,
	ChatMessage = 0x02

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
