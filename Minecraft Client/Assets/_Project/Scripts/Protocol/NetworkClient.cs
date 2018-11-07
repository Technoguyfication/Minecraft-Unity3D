using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient
{
	public TcpClient Client = new TcpClient();
	private readonly object StreamReadLock = new object();
	private readonly object StreamWriteLock = new object();

	public NetworkClient()
	{

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
			byte[] buffer = p.Raw;
			Write(buffer, 0, buffer.Length);
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
		byte[] data;

		lock (StreamReadLock)
		{
			length = ReadNextVarInt();
			List<byte> buffer = new List<byte>(ReadBytes(length));
			packetId = VarInt.Read(buffer);
			data = buffer.ToArray();
		}

		return new Packet()
		{
			PacketID = packetId,
			Payload = data
		};

	}

	/// <summary>
	/// Reads the next VarInt from the server
	/// </summary>
	/// <returns></returns>
	private int ReadNextVarInt()
	{
		int value = 0, numRead = 0, result = 0;
		byte read;
		lock (StreamReadLock)
		{
			while (true)
			{
				read = ReadBytes(1)[0];
				value = (read & 0x7F);
				result |= (value << (7 * numRead));

				numRead++;
				if (numRead > 5)
					throw new UnityException("VarInt too big!");

				if ((read & 0x80) != 128) break;
			}
		}
		return result;
	}
}
