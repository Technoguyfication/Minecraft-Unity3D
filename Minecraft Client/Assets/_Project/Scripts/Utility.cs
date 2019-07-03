using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utility
{

	/// <summary>
	/// Used to read bytes from a stream
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	public delegate byte[] ReadBytes(int amount);

	/// <summary>
	/// Concatentates two or more byte arrays
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="a1"></param>
	/// <param name="a2"></param>
	/// <returns></returns>
	public static byte[] Concat(this byte[] a1, params byte[][] a2)
	{
		byte[] dst = new byte[a1.Length + a2.Sum(a => a.Length)];
		Buffer.BlockCopy(a1, 0, dst, 0, a1.Length);
		int offset = a1.Length;
		foreach (var arr in a2)
		{
			Buffer.BlockCopy(arr, 0, dst, offset, arr.Length);
			offset += arr.Length;
		}
		return dst;
	}

	/// <summary>
	/// returns <see cref="amount"/> entries from the source list in a new list, removing them from the original
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data"></param>
	/// <returns></returns>
	public static List<T> Read<T>(this List<T> data, int amount, int index)
	{
		if (data.Count < amount + index)
			throw new IndexOutOfRangeException("Requested data out of range of List");

		List<T> subset = new List<T>();
		subset.AddRange(data.GetRange(index, amount));
		data.RemoveRange(index, amount);

		return subset;
	}

	public static List<T> Read<T>(this List<T> data, int amount)
	{
		return data.Read(amount, 0);
	}

	public static T[] ReverseIfLittleEndian<T>(this T[] array)
	{
		if (BitConverter.IsLittleEndian)
			Array.Reverse(array);

		return array;
	}

	public static T[] Reverse<T>(this T[] array)
	{
		Array.Reverse(array);
		return array;
	}

	public static List<T> ReverseIfLittleEndian<T>(this List<T> list)
	{
		if (BitConverter.IsLittleEndian)
			list.Reverse();

		return list;
	}

	public static int Mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	public static float Mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
