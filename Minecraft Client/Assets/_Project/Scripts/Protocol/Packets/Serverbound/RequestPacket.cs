using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// An empty packet https://wiki.vg/Server_List_Ping#Request
/// </summary>
public class RequestPacket : Packet
{
	public override byte[] Payload { get; set; } = new byte[0];

	public RequestPacket()
	{
		PacketID = (int)ServerboundIDs.Status_Request;
	}
}
