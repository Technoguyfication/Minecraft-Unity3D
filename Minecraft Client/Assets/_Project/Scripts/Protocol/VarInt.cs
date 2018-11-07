using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// https://wiki.vg/Protocol#Definitions
public class VarInt
{
	/// <summary>
	/// Reads the next VarInt from a list/>
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static int Read(List<byte> bytes)
	{
		int value = 0, numRead = 0, result = 0;
		byte read;
		while (true)
		{
			read = bytes.Read(1)[0];
			value = (read & 0x7F);
			result |= (value << (7 * numRead));

			numRead++;
			if (numRead > 5)
				throw new UnityException("VarInt too big!");
			if ((read & 0x80) != 0) break;
		}
		return result;
	}

	/// <summary>
	/// 
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
