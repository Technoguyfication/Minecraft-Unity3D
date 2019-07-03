using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Used for both ping and pong.
/// </summary>
public class PingPongPacket : Packet
{
	public PingPongPacket()
	{
		PacketID = 0x01;
	}

	public override byte[] Payload
	{
		set => Number = BitConverter.ToInt64(value.ReverseIfLittleEndian(), 0);
		get => BitConverter.GetBytes(Number).ReverseIfLittleEndian();
	}

	public long Number { get; set; }
}
