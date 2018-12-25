using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Ionic.Zlib;
using System.IO;

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
	public bool Connected { get { return Client?.Connected ?? false; } }

	public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);
	public event DisconnectedEventHandler Disconnected;

	private bool _disconnecting = false;
	private readonly EncryptionUtility _encryptionUtility = new EncryptionUtility();
	private int _compressionThreshold = -1;
	private TcpClient Client;
	private readonly object _streamWriteLock = new object();
	private readonly object _streamReadLock = new object();

	// we need seperate streams for reading and writing to support AES encryption
	private Stream _clientWriteStream;
	private Stream _clientReadStream;

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
		_clientReadStream = _clientWriteStream = Client.GetStream();
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
		_clientReadStream?.Dispose();
		_clientWriteStream?.Dispose();
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
				int i = 0; ;
				try
				{
					i = _clientReadStream.Read(buffer, bytesRead, amount - bytesRead);
				}
				catch (IOException ex)
				{
					Disconnect($"Connection closed: {ex.Message}");
				}
				catch (Exception ex)
				{
					Disconnect($"Error reading from socket: {ex.Message}");
				}
				if (i == 0)
					Disconnect("Connection closed");

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

		_clientWriteStream.Write(buffer, offset, size);
		if (_clientWriteStream.GetType() == typeof(CryptoStream))
			((CryptoStream)_clientWriteStream).FlushFinalBlock();
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

		if (State == ProtocolState.LOGIN)
		{
			// handle compression packet
			if (packetId == (int)ClientboundIDs.LOGIN_SET_COMPRESSION)
			{
				_compressionThreshold = VarInt.ReadNext(new List<byte>(payload));
				return ReadNextPacket();
			}

			// handle protocol encryption
			if (packetId == (int)ClientboundIDs.LOGIN_ENCRYPTION_REQUEST)
			{
				// lock write stream as we are reassigning the variable
				lock (_streamWriteLock)
				{
					var encryptionRequestPacket = new EncryptionRequestPacket()
					{
						Payload = payload
					};

					// generate shared AES secret
					byte[] sharedSecret = EncryptionUtility.GetSharedSecret();
					_encryptionUtility.SetAESKey(sharedSecret);

					// generate hash
					List<byte> hashBuilder = new List<byte>(sharedSecret);
					hashBuilder.AddRange(encryptionRequestPacket.PublicKey);
					string hash = EncryptionUtility.SHAHash(hashBuilder.ToArray());

					// send hash to mojang's servers
					MojangAuthentication.JoinServer(hash);

					// load RSA public key from server
					_encryptionUtility.SetRSAKey(encryptionRequestPacket.PublicKey);
					string rsaKeyPem = _encryptionUtility.GetPKCSPaddedRSAPublicKey();
					Debug.Log(rsaKeyPem);

					// send encryption response
					var response = new EncryptionResponsePacket()
					{
						SharedSecret = _encryptionUtility.EncryptRSA(sharedSecret),
						VerifyToken = _encryptionUtility.EncryptRSA(encryptionRequestPacket.VerifyToken)
					};
					WritePacket(response);

					// enable encryption by reassigning IO streams to crypto ones
					_clientWriteStream = new CryptoStream(Client.GetStream(), _encryptionUtility.AESEncryptTransform, CryptoStreamMode.Write);
					_clientReadStream = new CryptoStream(Client.GetStream(), _encryptionUtility.AESDecryptTransform, CryptoStreamMode.Read);

					// the next packet (and all following) should be encrypted
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
	{}

	public DisconnectedEventArgs(string reason)
	{
		Reason = reason;
	}
	public string Reason { get; set; }
}
