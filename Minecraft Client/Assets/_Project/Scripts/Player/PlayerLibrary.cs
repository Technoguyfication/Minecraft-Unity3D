using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Holds the collection of <see cref="Player"/>s on the server we're connected to
/// </summary>
public static class PlayerLibrary
{
	private static readonly Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

	/// <summary>
	/// Initialize the PlayerLibrary, which handles setting up events and gets everything ready for packet receiving.
	/// </summary>
	public static void Initialize()
	{
		// This is unused right now, but will be good for when event handling is setup.
		players.Clear();
	}

	/// <summary>
	/// Takes a player info packet, and uses the contained data to update values in this class.
	/// </summary>
	/// <param name="packet"></param>
	public static void HandleUpdatePacket(PlayerInfoPacket packet)
	{
		
	}
}
