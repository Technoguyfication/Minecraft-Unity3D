using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VarLong : MonoBehaviour {

	public long Value { get; set; }
	public byte[] Data
	{
		get
		{
			return GetBytes(Value);
		}
		set
		{
			Value = Read(value);
		}
	}

	public static long Read(byte[] bytes)
	{
		int numRead = 0;
		long value = 0, result = 0;
		byte read = bytes[0];
		while ((read & 0x80) != 0)
		{
			read = bytes[numRead];
			value = (read & 0x7F);
			result |= (value << (7 * numRead));

			numRead++;
			if (numRead > 10)
				throw new UnityException("VarLong too big!");
		}
		return result;
	}

	public static byte[] GetBytes(long value)
	{
		List<byte> bytes = new List<byte>();
		while ((value & -0x80) != 0)
		{
			bytes.Add((byte)(value & 0x7F | 0x80));
			value = (long)(((uint)value) >> 7);
		}
		bytes.Add((byte)value);
		return bytes.ToArray();
	}
}
