using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Server status response packet https://wiki.vg/Protocol#Response
/// </summary>
public class ResponsePacket : Packet
{
	public ResponsePacket()
	{
		PacketID = (int)ClientboundIDs.Status_Response;
	}

	public ResponsePacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => PacketHelper.GetBytes(JSONResponse);
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					JSONResponse = PacketReader.ReadString(reader);
				}
			}
		}
	}

	public string JSONResponse { get; set; }

	public override string ToString()
	{
		return JSONResponse;
	}
}
