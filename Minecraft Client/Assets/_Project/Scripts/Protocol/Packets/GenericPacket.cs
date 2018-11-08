using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GenericPacket : Packet
{
	public override byte[] Payload { get; set; }
}
