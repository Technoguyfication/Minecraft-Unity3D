using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Used for reading and writing data in packet format. DOES perform endianness fixing
/// </summary>
public static class PacketHelper
{
	/// <summary>
	/// Reads the next string
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static string GetString(List<byte> bytes)
	{
		int strLen = VarInt.ReadNext(bytes);
		var strRaw = bytes.Read(strLen);
		return Encoding.UTF8.GetString(strRaw.ToArray());
	}

	public static short GetInt16(List<byte> bytes)
	{
		byte[] shortRaw = bytes.Read(2).ToArray();
		return BitConverter.ToInt16(shortRaw.ReverseIfLittleEndian(), 0);
	}

	public static ushort GetUInt16(List<byte> bytes)
	{
		byte[] ushortRaw = bytes.Read(2).ToArray();
		return BitConverter.ToUInt16(ushortRaw.ReverseIfLittleEndian(), 0);
	}

	public static int GetInt32(List<byte> bytes)
	{
		byte[] intRaw = bytes.Read(4).ToArray();
		return BitConverter.ToInt32(intRaw.ReverseIfLittleEndian(), 0);
	}

	public static long GetInt64(List<byte> bytes)
	{
		byte[] longRaw = bytes.Read(8, 0).ToArray();
		return BitConverter.ToInt64(longRaw.ReverseIfLittleEndian(), 0);
	}

	public static ulong GetUInt64(List<byte> bytes)
	{
		byte[] ulongRaw = bytes.Read(8, 0).ToArray();
		return BitConverter.ToUInt64(ulongRaw.ReverseIfLittleEndian(), 0);
	}

	public static float GetSingle(List<byte> bytes)
	{
		byte[] floatRaw = bytes.Read(4).ToArray();
		return BitConverter.ToSingle(floatRaw.ReverseIfLittleEndian(), 0);
	}

	public static double GetDouble(List<byte> bytes)
	{
		byte[] doubleRaw = bytes.Read(8).ToArray();
		return BitConverter.ToDouble(doubleRaw.ReverseIfLittleEndian(), 0);
	}

	public static bool GetBoolean(List<byte> bytes)
	{
		byte[] boolRaw = bytes.Read(1, 0).ToArray();
		return BitConverter.ToBoolean(boolRaw, 0);
	}

	public static Guid GetGUID(List<byte> bytes)
	{
		byte[] guidRaw = bytes.Read(16).ToArray();
		return new Guid(guidRaw);
	}

	public static byte[] GetBytes(string data)
	{
		List<byte> builder = new List<byte>();
		byte[] strRaw = Encoding.UTF8.GetBytes(data);
		builder.AddRange(VarInt.GetBytes(strRaw.Length));
		builder.AddRange(strRaw);
		return builder.ToArray();
	}
}
