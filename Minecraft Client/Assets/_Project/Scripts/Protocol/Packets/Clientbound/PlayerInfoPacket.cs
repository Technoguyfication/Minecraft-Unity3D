using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInfoPacket : Packet
{
    public PlayerInfoPacket()
	{
		PacketID = (int)ClientboundIDs.PlayerInfo;
	}

    public PlayerInfoPacket(PacketData data) : base(data) { } // packet id should be set correctly if this ctor is used

    public override byte[] Payload
    {
        get => throw new NotImplementedException();
        set
        {
            using (MemoryStream stream = new MemoryStream(value))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int msgAction = PacketReader.ReadVarInt(reader);
                    PacketAction = (InfoPacketAction)msgAction;

                    int playerCount = PacketReader.ReadVarInt(reader);

                    switch (PacketAction)
                    {
                        case InfoPacketAction.AddPlayer:
                            Queue<AddPlayerAction> addedPlayers = new Queue<AddPlayerAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = PacketReader.ReadGUID(reader);

                                string name = PacketReader.ReadString(reader);
                                int propertyCount = PacketReader.ReadVarInt(reader);

                                Queue<Property> properties = new Queue<Property>();
                                for (int prop = 0; prop < propertyCount; prop++)
                                {
                                    string propertyName = PacketReader.ReadString(reader);
                                    string propertyValue = PacketReader.ReadString(reader);
                                    bool isSigned = PacketReader.ReadBoolean(reader);

                                    string signature = "";
                                    if (isSigned)
                                    {
                                        signature = PacketReader.ReadString(reader);
                                    }

                                    properties.Enqueue(new Property(propertyName, propertyValue, isSigned, signature));
                                }

                                GameMode gameMode = (GameMode)PacketReader.ReadVarInt(reader);
                                int ping = PacketReader.ReadVarInt(reader);
                                bool hasDisplayName = PacketReader.ReadBoolean(reader);

                                string displayName = "";
                                if (hasDisplayName)
                                {
                                    displayName = PacketReader.ReadString(reader);
                                }

                                addedPlayers.Enqueue(new AddPlayerAction(guid, name, properties.ToArray(), gameMode, ping, hasDisplayName, displayName));
                            }

                            AddPlayerActions = addedPlayers.ToArray();

                            break;
                        case InfoPacketAction.UpdateGameMode:
                            Queue<UpdateGamemodeAction> updatedGamemodes = new Queue<UpdateGamemodeAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = PacketReader.ReadGUID(reader);
                                GameMode gameMode = (GameMode)PacketReader.ReadVarInt(reader);

                                updatedGamemodes.Enqueue(new UpdateGamemodeAction(guid, gameMode));
                            }

                            UpdateGamemodeActions = updatedGamemodes.ToArray();
                            break;
                        case InfoPacketAction.UpdateLatency:

                            Queue<UpdateLatencyAction> updatedLatencies = new Queue<UpdateLatencyAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = PacketReader.ReadGUID(reader);
                                int ping = PacketReader.ReadVarInt(reader);

                                updatedLatencies.Enqueue(new UpdateLatencyAction(guid, ping));
                            }

                            UpdateLatencyActions = updatedLatencies.ToArray();

                            break;
                        case InfoPacketAction.UpdateDisplayName:

                            Queue<UpdateDisplayNameAction> updatedDisplayNames = new Queue<UpdateDisplayNameAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = PacketReader.ReadGUID(reader);
                                bool hasDisplayName = PacketReader.ReadBoolean(reader);

                                string displayName = "";
                                if (hasDisplayName)
                                {
                                    displayName = PacketReader.ReadString(reader);
                                }

                                updatedDisplayNames.Enqueue(new UpdateDisplayNameAction(guid, hasDisplayName, displayName));
                            }

                            UpdateDisplayNameActions = updatedDisplayNames.ToArray();

                            break;
                        case InfoPacketAction.RemovePlayer:

                            Queue<RemovePlayerAction> removedPlayerActions = new Queue<RemovePlayerAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = PacketReader.ReadGUID(reader);

                                removedPlayerActions.Enqueue(new RemovePlayerAction(guid));
                            }

                            RemovePlayerActions = removedPlayerActions.ToArray();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder outputString = new StringBuilder();
        switch (PacketAction)
        {
            case InfoPacketAction.AddPlayer:
                outputString.Append("PlayerInfoPacket: Action AddPlayer, ");
                for (int i = 0; i < AddPlayerActions.Length; i++)
                {
                    outputString.Append($"For GUID {AddPlayerActions[i].Guid} with name {AddPlayerActions[i].Name} with {AddPlayerActions[i].Properties.Length} properties in game mode {AddPlayerActions[i].GameMode} with ping {AddPlayerActions[i].Ping} has display name is {AddPlayerActions[i].HasDisplayName} with a display name of {AddPlayerActions[i].DisplayName}{((i + 1 != AddPlayerActions.Length) ? "," : "") }");
                }
                break;
            case InfoPacketAction.UpdateGameMode:
                outputString.Append("PlayerInfoPacket: Action UpdateGameMode, ");
                for (int i = 0; i < UpdateGamemodeActions.Length; i++)
                {
                    outputString.Append($"For GUID {UpdateGamemodeActions[i].Guid} new gamemode {UpdateGamemodeActions[i].GameMode}{((i + 1 != UpdateGamemodeActions.Length) ? "," : "") }");
                }
                break;
            case InfoPacketAction.UpdateLatency:
                outputString.Append("PlayerInfoPacket: Action UpdateLatency, ");
                for (int i = 0; i < UpdateLatencyActions.Length; i++)
                {
                    outputString.Append($"For GUID {UpdateLatencyActions[i].Guid} new latency {UpdateLatencyActions[i].Ping}{((i + 1 != UpdateLatencyActions.Length) ? "," : "") }");
                }
                break;
            case InfoPacketAction.UpdateDisplayName:
                outputString.Append("PlayerInfoPacket: Action UpdateGameMode, ");
                for (int i = 0; i < UpdateDisplayNameActions.Length; i++)
                {
                    outputString.Append($"For GUID {UpdateDisplayNameActions[i].Guid} hasDisplayName {UpdateDisplayNameActions[i].HasDisplayName} display name {UpdateDisplayNameActions[i].DisplayName}{((i + 1 != UpdateDisplayNameActions.Length) ? "," : "") }");
                }
                break;
            case InfoPacketAction.RemovePlayer:
                outputString.Append("PlayerInfoPacket: Action RemovePlayer, ");
                for (int i = 0; i < RemovePlayerActions.Length; i++)
                {
                    outputString.Append($"For GUID {RemovePlayerActions[i].Guid}{((i + 1 != RemovePlayerActions.Length) ? "," : "") }");
                }
                break;
            default:
                break;
        }

        return outputString.ToString();
    }

    public InfoPacketAction PacketAction { get; private set; }
    public AddPlayerAction[] AddPlayerActions { get; private set; }
    public UpdateGamemodeAction[] UpdateGamemodeActions { get; private set; }
    public UpdateLatencyAction[] UpdateLatencyActions { get; private set; }
    public UpdateDisplayNameAction[] UpdateDisplayNameActions { get; private set; }
    public RemovePlayerAction[] RemovePlayerActions { get; private set; }

    public struct AddPlayerAction
    {
        public Guid Guid { get; }
        public string Name { get; }
        public Property[] Properties { get; }
        public GameMode GameMode { get; }
        public int Ping { get; }
        public bool HasDisplayName { get; }
        public string DisplayName { get; }

        public AddPlayerAction(Guid guid, string name, Property[] properties, GameMode gameMode, int ping, bool hasDisplayName, string displayName)
        {
            Guid = guid;
            Name = name;
            Properties = properties;
            GameMode = gameMode;
            Ping = ping;
            HasDisplayName = hasDisplayName;
            DisplayName = displayName;
        }
    }

	public struct Property
	{
        public string Name { get; }
        public string Value { get; }
        public bool IsSigned { get; }
        public string Signature { get; }

        public Property(string name, string value, bool isSigned, string signature)
        {
            Name = name;
            Value = value;
            IsSigned = isSigned;
            Signature = signature;
        }
	}

	public struct UpdateGamemodeAction
	{
        public Guid Guid { get; }
        public GameMode GameMode { get; }

        public UpdateGamemodeAction(Guid guid, GameMode gameMode)
        {
            Guid = guid;
            GameMode = gameMode;
        }
	}

    public struct UpdateLatencyAction
    {
        public Guid Guid { get; }
        public int Ping { get; }

        public UpdateLatencyAction(Guid guid, int ping)
        {
            Guid = guid;
            Ping = ping;
        }
    }

    public struct UpdateDisplayNameAction
    {
        public Guid Guid { get; }
        public bool HasDisplayName { get; }
        public string DisplayName { get; }

        public UpdateDisplayNameAction(Guid guid, bool hasDisplayName, string displayName)
        {
            Guid = guid;
            HasDisplayName = hasDisplayName;
            DisplayName = displayName;
        }
    }

    public struct RemovePlayerAction
    {
        public Guid Guid { get; }

        public RemovePlayerAction(Guid guid)
        {
            Guid = guid;
        }
    }

    public enum InfoPacketAction
    {
        AddPlayer,
        UpdateGameMode,
        UpdateLatency,
        UpdateDisplayName,
        RemovePlayer
    }
}
