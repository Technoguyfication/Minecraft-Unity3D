using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Holds the collection of <see cref="Player"/>s on the server we're connected to
/// </summary>
public class PlayerLibrary
{
	public PlayerList PlayerList { get; }

	public PlayerLibrary(PlayerList playerList)
	{
		PlayerList = playerList;
	}

	private readonly Dictionary<Guid, Player> _players = new Dictionary<Guid, Player>();

	/// <summary>
	/// Updates the player library with data from a <see cref="PlayerInfoPacket"/>
	/// </summary>
	/// <param name="packet"></param>
	public void HandlePlayerInfoPacket(PlayerInfoPacket packet)
	{
		foreach (var action in packet.Actions)
		{
			// if removing a player, there's no need to look it up or create it
			if (packet.Type == PlayerInfoPacket.ActionType.RemovePlayer)
			{
				_players.Remove(action.UUID);
				continue;
			}

			// try to lookup player, otherwise add it to the player list
			if (!_players.TryGetValue(action.UUID, out Player player))
			{
				player = new Player();
				_players.Add(action.UUID, player);
			}

			switch (packet.Type)
			{
				case PlayerInfoPacket.ActionType.AddPlayer:
					player.GameMode = action.GameMode;
					player.Name = action.Name;
					player.Ping = action.Ping;
					player.DisplayName = action.DisplayName;
					player.UUID = action.UUID;

#if UNITY_EDITOR
					Debug.Log($"{player.Name} props: {string.Join(", ", action.Properties?.Select(x => x?.ToString()))}");
#endif
					break;
				case PlayerInfoPacket.ActionType.UpdateDisplayName:
					player.DisplayName = action.DisplayName;
					break;
				case PlayerInfoPacket.ActionType.UpdateGameMode:
					player.GameMode = action.GameMode;
					break;
				case PlayerInfoPacket.ActionType.UpdateLatency:
					player.Ping = action.Ping;
					break;
				default:
					throw new Exception($"Invalid player info action type: {packet.Type}");
			}
		}

		// update player list after handling all actions
		PlayerList?.UpdatePlayerList(_players.Values.ToArray());
	}

	/// <summary>
	/// Returns whether the player tracked in the library
	/// </summary>
	/// <param name="playerUuid"></param>
	/// <returns></returns>
	public bool HasPlayer(Guid playerUuid)
	{
		return _players.ContainsKey(playerUuid);
	}

	/// <summary>
	/// Tries to return a Player contained in the library
	/// </summary>
	/// <param name="playerUuid"></param>
	/// <param name="player"></param>
	/// <returns></returns>
	public bool TryGetPlayer(Guid playerUuid, out Player player)
	{
		return _players.TryGetValue(playerUuid, out player);
	}
}
