using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class NetworkClient : IDisposable
{
	/// <summary>
	/// See: https://wiki.vg/Protocol_version_numbers
	/// </summary>
	public const int PROTOCOL_VERSION = 404;

	public ProtocolState State;

	/// <summary>
	/// Gets whether the client is connected to a server or not
	/// </summary>
	public bool Connected { get { return Client?.Connected ?? false; } }

	private TcpClient Client;
	private readonly object StreamReadLock = new object();
	private readonly object StreamWriteLock = new object();

	public NetworkClient()
	{
		State = ProtocolState.HANDSHAKING;
	}

	~NetworkClient()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (Client?.Connected ?? false)
			Disconnect();

		Client?.Dispose();
	}

	/// <summary>
	/// Connect to a Minecraft server, will throw a <see cref="UnityException"/> if it is already connected to a server.
	/// </summary>
	/// <param name="hostname"></param>
	/// <param name="port"></param>
	public void Connect(string hostname, int port)
	{
		if (Client?.Connected ?? false)
			throw new UnityException("Need to disconnect first!");

		Client = new TcpClient();
		Client.Connect(hostname, port);
	}

	/// <summary>
	/// Starts connecting to a server
	/// </summary>
	/// <param name="hostname"></param>
	/// <param name="port"></param>
	public void StartConnect(string hostname, int port)
	{
		if (Client?.Connected ?? false)
			throw new UnityException("Need to disconnect first!");

		Client = new TcpClient();
		Client.ConnectAsync(hostname, port);
	}

	/// <summary>
	/// Disconnect from a server, sending the correct packets if needed
	/// </summary>
	public void Disconnect()
	{
		// TODO: use the correct protocol for disconnecting

		if (!Client.Connected)
			return;

		Client.Dispose();
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
	public PacketData ReadNextPacket()
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

		return new PacketData
		{
			ID = packetId,
			Payload = payload
		};
	}

	public enum ProtocolState
	{
		HANDSHAKING,
		STATUS = 1,
		LOGIN = 2,
		PLAY
	}
}
