using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using CSharp_easy_RSA_PEM;
using System.IO;

/// <summary>
/// Used for protocol encryption
/// </summary>
public class EncryptionUtility
{
	private const int SHARED_SECRET_SIZE = 16;
	private Aes _aesProvider;
	private ICryptoTransform _aesEncryptTransform;
	private ICryptoTransform _aesDecryptTransform;

	/// <summary>
	/// Encrypts data using the server's public key
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public byte[] EncryptRSA(byte[] data, string publicKey)
	{
		using (var rsa = Crypto.DecodeX509PublicKey(publicKey))
		{
			return rsa.Encrypt(data, true);
		}
	}

	/// <summary>
	/// Encrypts data using the shared AES key
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public byte[] EncryptAES(byte[] data)
	{
		using (var ms = new MemoryStream())
		{
			using (var cs = new CryptoStream(ms, _aesEncryptTransform, CryptoStreamMode.Write))
			{
				cs.Write(data, 0, data.Length);
				cs.Close();
				return ms.ToArray();
			}
		}
	}

	/// <summary>
	/// Decrypts data using AES
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public byte[] DecryptAES(byte[] data)
	{
		using (var ms = new MemoryStream())
		{
			using (var cs = new CryptoStream(ms, _aesDecryptTransform, CryptoStreamMode.Write))
			{
				cs.Write(data, 0, data.Length);
				cs.Close();
				return ms.ToArray();
			}
		}
	}

	/// <summary>
	/// Sets the AES key and IV so we can start using AES encryption
	/// </summary>
	public void SetAESKey(byte[] sharedKey)
	{
		_aesProvider = Aes.Create();
		_aesProvider.Key = sharedKey;
		_aesProvider.IV = sharedKey;

		_aesEncryptTransform = _aesProvider.CreateEncryptor();
		_aesDecryptTransform = _aesProvider.CreateDecryptor();
	}

	/// <summary>
	/// Gets a shared secret for use in AES symmetric encryption
	/// </summary>
	/// <returns></returns>
	public static byte[] GetSharedSecret()
	{
		using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
		{
			byte[] secret = new byte[SHARED_SECRET_SIZE];
			rng.GetBytes(secret);
			return secret;
		}
	}

	/// <summary>
	/// Generates a minecraft-style hash of a string
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string Hash(string input)
	{
		using (var sha = SHA1.Create())
		{
			byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
			var bigint = new System.Numerics.BigInteger(digest.Reverse());

			// if the digest is "negative", flip it using BigInteger. otherwise, supply original digest
			if (bigint.Sign == -1)
			{
				bigint = -bigint;
				return "-" + bigint.ToString("x");
			}
			else
			{
				return BitConverter.ToString(digest.Reverse()).Replace("-", "").ToLower().TrimStart('0');
			}
		}
	}
}
