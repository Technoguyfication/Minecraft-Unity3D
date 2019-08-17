using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// https://wiki.vg/Protocol#Definitions
public static class VarInt
{
	/// <summary>
	/// Reads the next VarInt from a list
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static int ReadNext(List<byte> bytes)
	{
		int value, numRead = 0, result = 0;
		byte read;
		while (true)
		{
			read = bytes.Read(1)[0];
			value = (read & 0x7F);
			result |= (value << (7 * numRead));

			numRead++;
			if (numRead > 5)
				throw new UnityException("VarInt too big!");
			if ((read & 0x80) != 128) break;
		}
		return result;
	}

	/// <summary>
	/// Reads the next varint from an array of bytes (does not "seek", use <see cref="ReadNext(List{byte})"/> for that)
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static int ReadNext(byte[] bytes)
	{
		return ReadNext(new List<byte>(bytes));
	}

	/// <summary>
	/// Reads the next VarInt from a stream
	/// </summary>
	/// <returns></returns>
	public static int ReadNext(Utility.ReadBytes readFunction)
	{
		int value, numRead = 0, result = 0;
		byte read;
		while (true)
		{
			read = readFunction(1)[0];
			value = (read & 0x7F);
			result |= (value << (7 * numRead));

			numRead++;
			if (numRead > 5)
				throw new UnityException("VarInt too big!");

			if ((read & 0x80) != 128) break;
		}
		return result;
	}

	/// <summary>
	/// Turns an int into varint bytes
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static byte[] GetBytes(int value)
	{
		List<byte> bytes = new List<byte>();
		while ((value & -0x80) != 0)
		{
			bytes.Add((byte)(value & 0x7F | 0x80));
			value = (int)(((uint)value) >> 7);
		}
		bytes.Add((byte)value);
		return bytes.ToArray();
	}
}
