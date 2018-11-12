using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility {

	/// <summary>
	/// Used to read bytes from a stream
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	public delegate byte[] ReadBytes(int amount);

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

	public static List<T> ReverseIfLittleEndian<T>(this List<T> list)
	{
		if (BitConverter.IsLittleEndian)
			list.Reverse();

		return list;
	}
}
