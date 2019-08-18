using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EncryptionRequestPacket : Packet
{
	public byte[] PublicKey { get; set; }
	public byte[] VerifyToken { get; set; }
	public string ServerID { get; set; }

	public EncryptionRequestPacket()
	{
		PacketID = (int)ClientboundIDs.Disconnect;
	}

	public EncryptionRequestPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

	public override byte[] Payload
	{
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					ServerID = PacketReader.ReadString(reader);
					int publicKeyLength = PacketReader.ReadVarInt(reader);
					PublicKey = PacketReader.ReadBytes(reader, publicKeyLength);
					int verifyTokenLength = PacketReader.ReadVarInt(reader);
					VerifyToken = PacketReader.ReadBytes(reader, verifyTokenLength);
				}
			}
		}
		get => throw new NotImplementedException();
	}
}
