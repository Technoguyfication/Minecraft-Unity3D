using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// Used for protocol encryption
/// </summary>
public class EncryptionUtility
{
	/// <summary>
	/// Generates a minecraft-style hash of a string
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public string Hash(string input)
	{
		using (var sha = SHA1.Create())
		{
			byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
			return new BigInteger(digest.ReverseIfLittleEndian()).ToString("x");	// don't know if we always need to reverse this or not
		}
	}
}
