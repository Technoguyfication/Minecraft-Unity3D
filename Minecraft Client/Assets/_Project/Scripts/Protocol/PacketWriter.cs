using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PacketWriter
{
	/// <summary>
	/// Writes a string to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The string value to write</param>
	public static void WriteString(BinaryWriter writer, string value)
	{
		writer.Write(value);
	}

	/// <summary>
	/// Writes a byte[] to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The byte[] value to write</param>
	public static void WriteBytes(BinaryWriter writer, byte[] value)
	{
		writer.Write(value);
	}

	/// <summary>
	/// Writes a float to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The float value to write</param>
	public static void WriteFloat(BinaryWriter writer, float value)
	{
		byte[] valueBytes = BitConverter.GetBytes(value);
		writer.Write(valueBytes.ReverseIfLittleEndian());
	}

	/// <summary>
	/// Writes a double to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The double value to write</param>
	public static void WriteDouble(BinaryWriter writer, double value)
	{
		byte[] valueBytes = BitConverter.GetBytes(value);
		writer.Write(valueBytes.ReverseIfLittleEndian());
	}

	/// <summary>
	/// Writes a VarInt to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The VarInt value to write</param>
	public static void WriteVarInt(BinaryWriter writer, int value)
	{
		writer.Write(VarInt.GetBytes(value));
	}

	/// <summary>
	/// Writes a Int16 (ushort) to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The Int16 (ushort) value to write</param>
	public static void WriteInt16(BinaryWriter writer, ushort value)
	{
		byte[] valueBytes = BitConverter.GetBytes(value);
		writer.Write(valueBytes.ReverseIfLittleEndian());
	}

	/// <summary>
	/// Writes a Int64 (long) to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The Int64 (long) value to write</param>
	public static void WriteInt64(BinaryWriter writer, long value)
	{
		byte[] valueBytes = BitConverter.GetBytes(value);
		writer.Write(valueBytes.ReverseIfLittleEndian());
	}

	/// <summary>
	/// Writes a boolean to the given BinaryWriter
	/// </summary>
	/// <param name="writer">The writer to use</param>
	/// <param name="value">The bool value to write</param>
	public static void WriteBoolean(BinaryWriter writer, bool value)
	{
		writer.Write(value);
	}
}
