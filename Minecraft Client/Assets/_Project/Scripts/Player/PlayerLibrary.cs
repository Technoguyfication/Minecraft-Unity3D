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
		switch (packet.PacketAction)
		{
			case PlayerInfoPacket.InfoPacketAction.AddPlayer:
				for (int i = 0; i < packet.AddPlayerActions.Length; i++)
				{
					PlayerInfoPacket.AddPlayerAction add = packet.AddPlayerActions[i];
					if (!players.ContainsKey(add.Guid))
						players.Add(add.Guid, new Player(add.HasDisplayName, add.DisplayName, null, 0, add.GameMode, add.Ping, add.Name));
				}
				break;
			case PlayerInfoPacket.InfoPacketAction.UpdateGameMode:
				for (int i = 0; i < packet.UpdateGamemodeActions.Length; i++)
				{
					PlayerInfoPacket.UpdateGamemodeAction update = packet.UpdateGamemodeActions[i];
					if (players.ContainsKey(update.Guid))
						players[update.Guid].PlayerGameMode = update.GameMode;
				}
				break;
			case PlayerInfoPacket.InfoPacketAction.UpdateLatency:
				for (int i = 0; i < packet.UpdateLatencyActions.Length; i++)
				{
					PlayerInfoPacket.UpdateLatencyAction update = packet.UpdateLatencyActions[i];
					if (players.ContainsKey(update.Guid))
						players[update.Guid].Ping = update.Ping;
				}
				break;
			case PlayerInfoPacket.InfoPacketAction.UpdateDisplayName:
				for (int i = 0; i < packet.UpdateDisplayNameActions.Length; i++)
				{
					PlayerInfoPacket.UpdateDisplayNameAction update = packet.UpdateDisplayNameActions[i];
					if (players.ContainsKey(update.Guid))
					{
						players[update.Guid].HasDisplayName = update.HasDisplayName;
						players[update.Guid].DisplayName = update.DisplayName;
					}
				}
				break;
			case PlayerInfoPacket.InfoPacketAction.RemovePlayer:
				for (int i = 0; i < packet.RemovePlayerActions.Length; i++)
				{
					PlayerInfoPacket.RemovePlayerAction remove = packet.RemovePlayerActions[i];
					if (players.ContainsKey(remove.Guid))
						players.Remove(remove.Guid);
				}
				break;
			default:
				break;
		}
	}

	public static Dictionary<Guid, Player> GetPlayers()
	{
		return players;
	}
}
