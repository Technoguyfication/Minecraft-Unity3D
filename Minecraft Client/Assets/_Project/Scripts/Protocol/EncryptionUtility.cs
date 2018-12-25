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
	private RSACryptoServiceProvider _rsa;
	public ICryptoTransform AESEncryptTransform { get; private set; }
	public ICryptoTransform AESDecryptTransform { get; private set; }

	/// <summary>
	/// Encrypts data using the server's public key
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public byte[] EncryptRSA(byte[] data)
	{
		return _rsa.Encrypt(data, false);
	}

	/// <summary>
	/// Gets the PKCS#1 v1.5 padded version of the loaded public key
	/// </summary>
	/// <returns></returns>
	public string GetPKCSPaddedRSAPublicKey()
	{
		return Crypto.ExportPublicKeyToRSAPEM(_rsa);
	}

	/// <summary>
	/// Sets the RSA public key for encryption
	/// </summary>
	/// <param name="publicKey"></param>
	public void SetRSAKey(byte[] publicKey)
	{
		string rsaPem = "-----BEGIN PUBLIC KEY-----\n" + Convert.ToBase64String(publicKey) + "\n-----END PUBLIC KEY-----";
		_rsa = Crypto.DecodeX509PublicKey(rsaPem);
	}

	/// <summary>
	/// Sets the AES key and IV so we can start using AES encryption
	/// </summary>
	public void SetAESKey(byte[] sharedKey)
	{
		_aesProvider = Aes.Create();
		_aesProvider.Key = sharedKey;
		_aesProvider.IV = sharedKey;

		AESEncryptTransform = _aesProvider.CreateEncryptor();
		AESDecryptTransform = _aesProvider.CreateDecryptor();
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
	public static string SHAHash(byte[] input)
	{
		using (var sha = SHA1.Create())
		{
			byte[] digest = sha.ComputeHash(input);
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
