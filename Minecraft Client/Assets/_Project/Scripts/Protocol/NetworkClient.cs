using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zlib;
using UnityEngine;

public class NetworkClient : IDisposable
{
	/// <summary>
	/// See: https://wiki.vg/Protocol_version_numbers
	/// </summary>
	public const int PROTOCOL_VERSION = 404;
	public ProtocolState State { get; set; } = ProtocolState.HANDSHAKING;

	/// <summary>
	/// Gets whether the client is connected to a server or not
	/// </summary>
	public bool Connected => Client?.Connected ?? false;

	public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);
	public event DisconnectedEventHandler Disconnected;

	private bool _disconnecting = false;
	private int _compressionThreshold = -1;
	private TcpClient Client;
	private readonly object _streamWriteLock = new object();
	private readonly object _streamReadLock = new object();
	private AesStream _aesStream;
	private bool _encrypted = false;

	public NetworkClient()
	{

	}

	~NetworkClient()
	{
		Dispose();
	}

	public void Dispose()
	{
		Disconnect("Client disposing");
		Client?.Dispose();
		_aesStream.Dispose();
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

		_disconnecting = false;
		Client = new TcpClient();
		Client.Connect(hostname, port);
	}

	/// <summary>
	/// Disconnect from a server
	/// </summary>
	public void Disconnect(string reason)
	{
		_disconnecting = true;

		if (!Client?.Connected ?? false || _disconnecting)
			return;

		Client?.Close();
		Disconnected?.Invoke(this, new DisconnectedEventArgs(reason ?? "Connection closed"));
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

		lock (_streamReadLock)
		{
			while (bytesRead < amount)
			{
				int i = 0;

				if (_encrypted)
				{
					i = _aesStream.Read(buffer, bytesRead, amount - bytesRead);
				}
				else
				{
					i = Client.GetStream().Read(buffer, bytesRead, amount - bytesRead);
				}

				if (i == 0)
				{
					throw new IOException("Connection closed");
				}

				bytesRead += i;
			}
		}

		return buffer;
	}

	/// <summary>
	/// Sends bytes to the server
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="offset"></param>
	/// <param name="size"></param>
	public void WriteBytes(byte[] buffer, int offset, int size)
	{
		if (!Client.Connected)
			throw new UnityException("Client not connected!");

		if (_encrypted)
			_aesStream.Write(buffer, offset, size);
		else
			Client.GetStream().Write(buffer, offset, size);
	}

	/// <summary>
	/// Sends a packet to the server
	/// </summary>
	/// <param name="p"></param>
	public void WritePacket(Packet p)
	{
		lock (_streamWriteLock)
		{
			List<byte> buffer = new List<byte>();

			// check if compression enabled
			if (_compressionThreshold >= 0)
			{
				List<byte> uncompressedBody = new List<byte>(VarInt.GetBytes(p.PacketID));
				uncompressedBody.AddRange(p.Payload);

				if (uncompressedBody.Count >= _compressionThreshold)
				{
					// compress data
					byte[] compressedBody = ZlibStream.CompressBuffer(uncompressedBody.ToArray());

					// add the packet payload data first, and we will insert the length of the data at the beginning of the packet
					buffer.AddRange(VarInt.GetBytes(uncompressedBody.Count));
					buffer.AddRange(compressedBody);
					buffer.InsertRange(0, VarInt.GetBytes(buffer.Count));
				}
				else
				{
					// data length below compression threshold
					buffer.AddRange(VarInt.GetBytes(uncompressedBody.Count + 1));   // add one for length of data length
					buffer.Add(0);  // 0 data length for uncompressed payload
					buffer.AddRange(uncompressedBody);
				}
			}
			else
			{
				// no compression
				buffer.AddRange(VarInt.GetBytes(p.Length));
				buffer.AddRange(VarInt.GetBytes(p.PacketID));
				buffer.AddRange(p.Payload);
			}

			WriteBytes(buffer.ToArray(), 0, buffer.Count);
			if (_encrypted)
				_aesStream.Flush();
			else
				Client.GetStream().Flush();
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
	/// Gets the next packet from the server
	/// </summary>
	/// <returns></returns>
	public PacketData ReadNextPacket()
	{
		int packetId;
		byte[] payload;

		lock (_streamReadLock)
		{
			int length = VarInt.ReadNext(ReadBytes);
			List<byte> buffer = new List<byte>();

			// check if data is compressed
			if (_compressionThreshold >= 0)
			{
				int dataLength = VarInt.ReadNext(ReadBytes);
				length -= VarInt.GetBytes(dataLength).Length;   // remove size of data length from rest of packet length
				if (dataLength != 0)
				{
					byte[] compressedBuffer = ReadBytes(length);
					buffer.AddRange(ZlibStream.UncompressBuffer(compressedBuffer));
				}
				else
				{
					buffer.AddRange(ReadBytes(length));
				}
			}
			else
			{
				buffer.AddRange(ReadBytes(length));
			}

			packetId = VarInt.ReadNext(buffer);
			payload = buffer.ToArray();
		}

		// handles some stuff during login phase
		if (State == ProtocolState.LOGIN)
		{
			// handle compression packet
			if (packetId == (int)ClientboundIDs.LogIn_SetCompression)
			{
				_compressionThreshold = VarInt.ReadNext(new List<byte>(payload));
				return ReadNextPacket();
			}

			// handle protocol encryption packet
			if (packetId == (int)ClientboundIDs.LogIn_EncryptionRequest)
			{
				var encRequestPkt = new EncryptionRequestPacket()
				{
					Payload = payload
				};

				var aesSecret = CryptoHandler.GenerateSharedSecret();
				var authHash = CryptoHandler.SHAHash(Encoding.ASCII.GetBytes(encRequestPkt.ServerID).Concat(aesSecret, encRequestPkt.PublicKey));

				Debug.Log($"Sending hash to Mojang servers: {authHash}");

				// check session with mojang
				if (!MojangAPI.JoinServer(authHash))
					throw new UnityException("Invalid session. (Try restarting game or relogging into Minecraft account)");

				// use pub key to encrypt shared secret
				using (var rsaProvider = CryptoHandler.DecodeRSAPublicKey(encRequestPkt.PublicKey))
				{
					byte[] encSecret = rsaProvider.Encrypt(aesSecret, false);
					byte[] encToken = rsaProvider.Encrypt(encRequestPkt.VerifyToken, false);

					// respond to server with private key
					var responsePkt = new EncryptionResponsePacket()
					{
						SharedSecret = encSecret,
						VerifyToken = encToken
					};
					WritePacket(responsePkt);


					// enable aes encryption
					_aesStream = new AesStream(Client.GetStream(), aesSecret);
					_encrypted = true;

					// read the next packet
					return ReadNextPacket();
				}
			}
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

public class DisconnectedEventArgs
{
	public DisconnectedEventArgs()
	{ }

	public DisconnectedEventArgs(string reason)
	{
		Reason = reason;
	}
	public string Reason { get; set; }
}
