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
    private static Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

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
        switch (packet.messageAction)
        {
            case PlayerInfoPacket.MessageAction.AddPlayer:
                for (int i = 0; i < packet.addPlayerActions.Length; i++)
                {
                    PlayerInfoPacket.AddPlayerAction add = packet.addPlayerActions[i];
                    if(!players.ContainsKey(add.guid))
                        players.Add(add.guid, new Player(add.hasDisplayName, add.displayName, null, 0, add.gameMode, add.ping, add.name));
                }
                break;
            case PlayerInfoPacket.MessageAction.UpdateGameMode:
                for (int i = 0; i < packet.updateGamemodeActions.Length; i++)
                {
                    PlayerInfoPacket.UpdateGamemodeAction update = packet.updateGamemodeActions[i];
                    if (players.ContainsKey(update.guid))
                        players[update.guid].gameMode = update.gameMode;
                }
                break;
            case PlayerInfoPacket.MessageAction.UpdateLatency:
                for (int i = 0; i < packet.updateLatencyActions.Length; i++)
                {
                    PlayerInfoPacket.UpdateLatencyAction update = packet.updateLatencyActions[i];
                    if (players.ContainsKey(update.guid))
                        players[update.guid].ping = update.ping;
                }
                break;
            case PlayerInfoPacket.MessageAction.UpdateDisplayName:
                for (int i = 0; i < packet.updateDisplayNameActions.Length; i++)
                {
                    PlayerInfoPacket.UpdateDisplayNameAction update = packet.updateDisplayNameActions[i];
                    if (players.ContainsKey(update.guid))
                    {
                        players[update.guid].hasDisplayName = update.hasDisplayName;
                        players[update.guid].displayName = update.displayName;
                    }
                }
                break;
            case PlayerInfoPacket.MessageAction.RemovePlayer:
                for (int i = 0; i < packet.removePlayerActions.Length; i++)
                {
                    PlayerInfoPacket.RemovePlayerAction remove = packet.removePlayerActions[i];
                    if(players.ContainsKey(remove.guid))
                        players.Remove(remove.guid);
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
