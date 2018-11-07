using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility {

	/// <summary>
	/// returns <see cref="amount"/> entries from the source list in a new list, removing them from the original
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data"></param>
	/// <returns></returns>
	public static List<T> Read<T>(this List<T> data, int amount, int index = 0)
	{
		List<T> subset = new List<T>();
		subset.AddRange(data.GetRange(index, amount));
		data.RemoveRange(index, amount);

		return subset;
	}
}
