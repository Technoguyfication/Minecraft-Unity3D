using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HandshakePacket : Packet
{
	public int ProtocolVersion { get; set; } = NetworkClient.PROTOCOL_VERSION;
	public string Address { get; set; }
	public int Port { get; set; }
	public NetworkClient.ProtocolState NextState { get; set; }

	public HandshakePacket()
	{
		PacketID = (int)ServerboundIDs.Status_Request;
	}

	public HandshakePacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteVarInt(writer, ProtocolVersion);
					PacketWriter.WriteString(writer, Address);
					PacketWriter.WriteInt16(writer, (ushort)Port);
					PacketWriter.WriteVarInt(writer, (int)NextState);

					return stream.ToArray();
				}
			}
		}
		set => throw new NotImplementedException();
	}
}
