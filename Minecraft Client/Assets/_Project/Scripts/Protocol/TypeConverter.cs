using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Used for reading and writing data in packet payload format
/// </summary>
public static class TypeConverter
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
		return Encoding.Unicode.GetString(strRaw.ToArray());
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
