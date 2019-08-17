using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// An encrypted AES stream wrapper on top of an underlying stream
/// </summary>
public class AesStream : Stream
{
	Stream BaseStream { get; set; }

	readonly CryptoStream encryptionStream;
	readonly CryptoStream decryptionStream;
	readonly SymmetricAlgorithm encryptor;
	readonly SymmetricAlgorithm decryptor;
	public AesStream(Stream underlyingStream, byte[] secret)
	{
		BaseStream = underlyingStream;

		encryptor = GenerateAES(secret);
		encryptionStream = new CryptoStream(underlyingStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write);

		decryptor = GenerateAES(secret);
		decryptionStream = new CryptoStream(underlyingStream, decryptor.CreateDecryptor(), CryptoStreamMode.Read);

	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override void Flush()
	{
		BaseStream.Flush();
	}

	public override long Length => throw new NotSupportedException();

	public override long Position
	{
		get => throw new NotSupportedException();
		set => throw new NotSupportedException();
	}

	public override int ReadByte()
	{
		return decryptionStream.ReadByte();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return decryptionStream.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void WriteByte(byte b)
	{
		encryptionStream.WriteByte(b);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		encryptionStream.Write(buffer, offset, count);
	}

	private RijndaelManaged GenerateAES(byte[] key)
	{
		RijndaelManaged cipher = new RijndaelManaged
		{
			Mode = CipherMode.CFB,
			Padding = PaddingMode.None,
			KeySize = 128,
			FeedbackSize = 8,
			Key = key,
			IV = key
		};

		return cipher;
	}

	~AesStream()
	{
		Dispose();
	}

	new void Dispose()
	{
		encryptor?.Dispose();
		decryptor?.Dispose();

		base.Dispose();
	}
}
