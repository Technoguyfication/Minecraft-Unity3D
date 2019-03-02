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

	CryptoStream enc;
	CryptoStream dec;
	public AesStream(Stream underlyingStream, byte[] secret)
	{
		BaseStream = underlyingStream;
		enc = new CryptoStream(underlyingStream, GenerateAES(secret).CreateEncryptor(), CryptoStreamMode.Write);
		dec = new CryptoStream(underlyingStream, GenerateAES(secret).CreateDecryptor(), CryptoStreamMode.Read);
	}

	public override bool CanRead
	{
		get { return true; }
	}

	public override bool CanSeek
	{
		get { return false; }
	}

	public override bool CanWrite
	{
		get { return true; }
	}

	public override void Flush()
	{
		BaseStream.Flush();
	}

	public override long Length
	{
		get { throw new NotSupportedException(); }
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override int ReadByte()
	{
		return dec.ReadByte();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return dec.Read(buffer, offset, count);
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
		enc.WriteByte(b);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		enc.Write(buffer, offset, count);
	}

	private RijndaelManaged GenerateAES(byte[] key)
	{
		RijndaelManaged cipher = new RijndaelManaged();
		cipher.Mode = CipherMode.CFB;
		cipher.Padding = PaddingMode.None;
		cipher.KeySize = 128;
		cipher.FeedbackSize = 8;
		cipher.Key = key;
		cipher.IV = key;
		return cipher;
	}
}
