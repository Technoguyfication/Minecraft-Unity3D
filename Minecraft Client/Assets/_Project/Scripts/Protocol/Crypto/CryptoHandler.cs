using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// Used for protocol encryption
/// </summary>
public class CryptoHandler
{
	private const int SHARED_SECRET_SIZE = 16;

	/// <summary>
	/// Gets service for encrypting data with the server's public key
	/// </summary>
	/// <param name="x509key"></param>
	/// <returns></returns>
	public static RSACryptoServiceProvider DecodeRSAPublicKey(byte[] x509key)
	{
		/* Code from StackOverflow no. 18091460 */

		byte[] SeqOID = { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01 };

		using (MemoryStream ms = new MemoryStream(x509key))
		{
			using (BinaryReader reader = new BinaryReader(ms))
			{

				if (reader.ReadByte() == 0x30)
					ReadASNLength(reader); //skip the size
				else
					return null;

				int identifierSize = 0; //total length of Object Identifier section
				if (reader.ReadByte() == 0x30)
					identifierSize = ReadASNLength(reader);
				else
					return null;

				if (reader.ReadByte() == 0x06) //is the next element an object identifier?
				{
					int oidLength = ReadASNLength(reader);
					byte[] oidBytes = new byte[oidLength];
					reader.Read(oidBytes, 0, oidBytes.Length);
					if (oidBytes.SequenceEqual(SeqOID) == false) //is the object identifier rsaEncryption PKCS#1?
						return null;

					int remainingBytes = identifierSize - 2 - oidBytes.Length;
					reader.ReadBytes(remainingBytes);
				}

				if (reader.ReadByte() == 0x03) //is the next element a bit string?
				{
					ReadASNLength(reader); //skip the size
					reader.ReadByte(); //skip unused bits indicator
					if (reader.ReadByte() == 0x30)
					{
						ReadASNLength(reader); //skip the size
						if (reader.ReadByte() == 0x02) //is it an integer?
						{
							int modulusSize = ReadASNLength(reader);
							byte[] modulus = new byte[modulusSize];
							reader.Read(modulus, 0, modulus.Length);
							if (modulus[0] == 0x00) //strip off the first byte if it's 0
							{
								byte[] tempModulus = new byte[modulus.Length - 1];
								Array.Copy(modulus, 1, tempModulus, 0, modulus.Length - 1);
								modulus = tempModulus;
							}

							if (reader.ReadByte() == 0x02) //is it an integer?
							{
								int exponentSize = ReadASNLength(reader);
								byte[] exponent = new byte[exponentSize];
								reader.Read(exponent, 0, exponent.Length);

								RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
								RSAParameters RSAKeyInfo = new RSAParameters
								{
									Modulus = modulus,
									Exponent = exponent
								};
								RSA.ImportParameters(RSAKeyInfo);
								return RSA;
							}
						}
					}
				}
			}
		}
		return null;
	}

	private static int ReadASNLength(BinaryReader reader)
	{
		//Note: this method only reads lengths up to 4 bytes long as
		//this is satisfactory for the majority of situations.
		int length = reader.ReadByte();
		if ((length & 0x00000080) == 0x00000080) //is the length greater than 1 byte
		{
			int count = length & 0x0000000f;
			byte[] lengthBytes = new byte[4];
			reader.Read(lengthBytes, 4 - count, count);
			Array.Reverse(lengthBytes); //
			length = BitConverter.ToInt32(lengthBytes, 0);
		}
		return length;
	}

	/// <summary>
	/// Gets a shared secret for use in AES symmetric encryption
	/// </summary>
	/// <returns></returns>
	public static byte[] GenerateSharedSecret()
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
