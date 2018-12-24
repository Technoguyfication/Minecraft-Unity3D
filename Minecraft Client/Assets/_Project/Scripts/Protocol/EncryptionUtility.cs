using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using CSharp_easy_RSA_PEM;

/// <summary>
/// Used for protocol encryption
/// </summary>
public class EncryptionUtility
{
	private const int KEY_SIZE = 1024;
	private readonly RSACryptoServiceProvider _rsaProvider;

	/// <summary>
	/// Creates a new encryption utility class with the defined public key
	/// </summary>
	/// <param name="publicKey">An X509</param>
	public EncryptionUtility(string publicKey)
	{
		_rsaProvider = Crypto.DecodeX509PublicKey(publicKey);
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
