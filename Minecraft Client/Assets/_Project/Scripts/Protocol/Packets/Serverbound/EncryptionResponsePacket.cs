using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EncryptionResponsePacket : Packet
{
	public EncryptionResponsePacket()
	{
		PacketID = (int)ServerboundIDs.LogIn_EncryptionResponse;
	}

	public EncryptionResponsePacket(PacketData data) : base(data) { }

	public byte[] SharedSecret { get; set; }
	public byte[] VerifyToken { get; set; }

	public override byte[] Payload
	{
		get
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					PacketWriter.WriteVarInt(writer, SharedSecret.Length);
					PacketWriter.WriteBytes(writer, SharedSecret);
					PacketWriter.WriteVarInt(writer, VerifyToken.Length);
					PacketWriter.WriteBytes(writer, VerifyToken);

					return stream.ToArray();
				}
			}
		}
		set => throw new NotImplementedException();
	}
}
