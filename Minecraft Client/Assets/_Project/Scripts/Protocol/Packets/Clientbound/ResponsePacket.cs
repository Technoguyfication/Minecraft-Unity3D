using System;
using System.Collections.Generic;
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
		PacketID = (int)ClientboundIDs.STATUS_RESPONSE;
	}

	public ResponsePacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get
		{
			return PacketStructureUtility.GetBytes(JSONResponse);
		}
		set
		{
			List<byte> buffer = new List<byte>(value);
			JSONResponse = PacketStructureUtility.GetString(buffer);
		}
	}

	public string JSONResponse { get; set; }

	public override string ToString()
	{
		return JSONResponse;
	}
}
