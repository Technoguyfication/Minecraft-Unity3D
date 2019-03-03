using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EncryptionRequestPacket : Packet
{
	public byte[] PublicKey { get; set; }
	public byte[] VerifyToken { get; set; }
	public string ServerID { get; set; }

	public override byte[] Payload
	{
		set
		{
			List<byte> buffer = new List<byte>(value);
			ServerID = PacketHelper.GetString(buffer);
			int pubKeyLen = VarInt.ReadNext(buffer);
			PublicKey = buffer.Read(pubKeyLen).ToArray();
			int verifyTokenLen = VarInt.ReadNext(buffer);
			VerifyToken = buffer.Read(verifyTokenLen).ToArray();
		}
		get
		{
			throw new NotImplementedException();
		}
	}

	public EncryptionRequestPacket()
	{
		PacketID = (int)ClientboundIDs.DISCONNECT;
	}

	public EncryptionRequestPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

}
