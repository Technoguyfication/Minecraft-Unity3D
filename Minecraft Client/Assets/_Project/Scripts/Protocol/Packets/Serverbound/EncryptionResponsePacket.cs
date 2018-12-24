using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncryptionResponsePacket : Packet
{
	public EncryptionResponsePacket()
	{
		PacketID = (int)ServerboundIDs.LOGIN_ENCRYPTION_RESPONSE;
	}

	public EncryptionResponsePacket(PacketData data) : base(data) { }

	public byte[] SharedSecret { get; set; }
	public byte[] VerifyToken { get; set; }

	public override byte[] Payload
	{
		get
		{
			List<byte> builder = new List<byte>();
			builder.AddRange(VarInt.GetBytes(SharedSecret.Length));
			builder.AddRange(SharedSecret);
			builder.AddRange(VarInt.GetBytes(VerifyToken.Length));
			builder.AddRange(VerifyToken);
			return builder.ToArray();
		}
		set
		{
			throw new NotImplementedException();
		}
	}
}
