using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient
{
	public TcpClient Client = new TcpClient();
	public ProtocolState State;

	private readonly object StreamReadLock = new object();
	private readonly object StreamWriteLock = new object();

	public NetworkClient()
	{
		State = ProtocolState.HANDSHAKING;
	}

	/// <summary>
	/// Reads bytes directly from the stream
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	private byte[] ReadBytes(int amount)
	{
		if (!Client.Connected)
			throw new UnityException("Client not connected!");

		byte[] buffer = new byte[amount];
		int bytesRead = 0;

		lock (StreamReadLock)
		{
			while (bytesRead < amount)
			{
				int i = Client.GetStream().Read(buffer, bytesRead, amount - bytesRead);
				if (i == 0)
					throw new UnityException("Network disconnected");

				bytesRead += i;
			}
		}

		return buffer;
	}

	/// <summary>
	/// Sends a packet to the server
	/// </summary>
	/// <param name="p"></param>
	public void WritePacket(Packet p)
	{
		lock (StreamWriteLock)
		{
			byte[] buffer = Packet.GetRaw(p);
			Write(buffer, 0, buffer.Length);
		}
	}

	/// <summary>
	/// Writes multiple packets
	/// </summary>
	/// <param name="packets"></param>
	public void WritePackets(ICollection<Packet> packets)
	{
		foreach (var packet in packets)
		{
			WritePacket(packet);
		}
	}

	/// <summary>
	/// Sends bytes to the server
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="offset"></param>
	/// <param name="size"></param>
	public void Write(byte[] buffer, int offset, int size)
	{
		if (!Client.Connected)
			throw new UnityException("Client not connected!");

		Client.GetStream().Write(buffer, offset, size);
	}

	/// <summary>
	/// Gets the next packet from the server
	/// </summary>
	/// <returns></returns>
	public Packet ReadNextPacket()
	{
		int length;
		int packetId;
		byte[] payload;

		lock (StreamReadLock)
		{
			length = VarInt.ReadNext(ReadBytes);
			List<byte> buffer = new List<byte>(ReadBytes(length));
			packetId = VarInt.ReadNext(buffer);
			payload = buffer.ToArray();
		}

		switch (State)
		{
			case ProtocolState.STATUS:
				switch (packetId)
				{
					case 0x00: return new ResponsePacket() { Payload = payload };
					case 0x01: return new PingPongPacket() { Payload = payload };
				}
				break;
			case ProtocolState.LOGIN:
				switch (packetId)
				{

				}
				break;
		}

		// no clue what the packet is.. return it anyways
		return new GenericPacket { PacketID = packetId, Payload = payload };
	}

	public enum ProtocolState
	{
		HANDSHAKING,
		STATUS = 1,
		LOGIN = 2,
		PLAY
	}
}
