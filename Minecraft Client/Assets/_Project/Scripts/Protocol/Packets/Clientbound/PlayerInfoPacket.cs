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
		PacketID = (int)ClientboundIDs.PLAYER_INFO;
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
                    PacketReader.ReadVarInt(reader, out int msgAction);
                    messageAction = (MessageAction)msgAction;

                    PacketReader.ReadVarInt(reader, out int playerCount);

                    Debug.Log($"Action: {messageAction.ToString()}, Players: {playerCount}");

                    switch (messageAction)
                    {
                        case MessageAction.AddPlayer:
                            Queue<AddPlayerAction> addedPlayers = new Queue<AddPlayerAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                PacketReader.ReadGUID(reader, out Guid guid);

                                PacketReader.ReadString(reader, out string name);
                                PacketReader.ReadVarInt(reader, out int propertyCount);

                                Queue<Property> properties = new Queue<Property>();
                                for (int prop = 0; prop < propertyCount; prop++)
                                {
                                    PacketReader.ReadString(reader, out string propertyName);
                                    PacketReader.ReadString(reader, out string propertyValue);
                                    PacketReader.ReadBoolean(reader, out bool isSigned);

                                    string signature = "";
                                    if (isSigned)
                                    {
                                        PacketReader.ReadString(reader, out signature);
                                    }

                                    properties.Enqueue(new Property(propertyName, propertyValue, isSigned, signature));
                                }

                                PacketReader.ReadVarInt(reader, out int gameMode);
                                PacketReader.ReadVarInt(reader, out int ping);
                                PacketReader.ReadBoolean(reader, out bool hasDisplayName);

                                string displayName = "";
                                if (hasDisplayName)
                                {
                                    PacketReader.ReadString(reader, out displayName);
                                }

                                addedPlayers.Enqueue(new AddPlayerAction(guid, name, properties.ToArray(), (GameMode)gameMode, ping, hasDisplayName, displayName));
                                Debug.Log($"Added player with name {name}, with {propertyCount} properties, in game mode {gameMode.ToString()}, with ping {ping}, has display name is {hasDisplayName}, with a display name of {displayName}");
                            }

                            addPlayerActions = addedPlayers.ToArray();

                            break;
                        case MessageAction.UpdateGameMode:
                            Queue<UpdateGamemodeAction> updatedGamemodes = new Queue<UpdateGamemodeAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                PacketReader.ReadGUID(reader, out Guid guid);
                                PacketReader.ReadVarInt(reader, out int gameMode);

                                updatedGamemodes.Enqueue(new UpdateGamemodeAction(guid, (GameMode)gameMode));

                                Debug.Log($"Updated gamemode for player");
                            }

                            updateGamemodeActions = updatedGamemodes.ToArray();
                            break;
                        case MessageAction.UpdateLatency:

                            Queue<UpdateLatencyAction> updatedLatencies = new Queue<UpdateLatencyAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                PacketReader.ReadGUID(reader, out Guid guid);
                                PacketReader.ReadVarInt(reader, out int ping);

                                updatedLatencies.Enqueue(new UpdateLatencyAction(guid, ping));

                                Debug.Log($"Updated ping for player");
                            }

                            updateLatencyActions = updatedLatencies.ToArray();

                            break;
                        case MessageAction.UpdateDisplayName:

                            Queue<UpdateDisplayNameAction> updatedDisplayNames = new Queue<UpdateDisplayNameAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                PacketReader.ReadGUID(reader, out Guid guid);
                                PacketReader.ReadBoolean(reader, out bool hasDisplayName);

                                string displayName = "";
                                if (hasDisplayName)
                                {
                                    PacketReader.ReadString(reader, out displayName);
                                }

                                updatedDisplayNames.Enqueue(new UpdateDisplayNameAction(guid, hasDisplayName, displayName));

                                Debug.Log($"Updated display name / chat for player");
                            }

                            updateDisplayNameActions = updatedDisplayNames.ToArray();

                            break;
                        case MessageAction.RemovePlayer:

                            Queue<RemovePlayerAction> removedPlayerActions = new Queue<RemovePlayerAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                PacketReader.ReadGUID(reader, out Guid guid);

                                removedPlayerActions.Enqueue(new RemovePlayerAction(guid));

                                Debug.Log($"Removed player");
                            }

                            removePlayerActions = removedPlayerActions.ToArray();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public MessageAction messageAction { get; private set; }
    public AddPlayerAction[] addPlayerActions { get; private set; }
    public UpdateGamemodeAction[] updateGamemodeActions { get; private set; }
    public UpdateLatencyAction[] updateLatencyActions { get; private set; }
    public UpdateDisplayNameAction[] updateDisplayNameActions { get; private set; }
    public RemovePlayerAction[] removePlayerActions { get; private set; }

    public struct AddPlayerAction
	{
        public Guid guid { get; }
		public string name { get; }
		public Property[] properties { get; }
		public GameMode gameMode { get; }
		public int ping { get; }
		public bool hasDisplayName { get; }
		public string displayName { get; }

        public AddPlayerAction(Guid guid, string name, Property[] properties, GameMode gameMode, int ping, bool hasDisplayName, string displayName)
        {
            this.guid = guid;
            this.name = name;
            this.properties = properties;
            this.gameMode = gameMode;
            this.ping = ping;
            this.hasDisplayName = hasDisplayName;
            this.displayName = displayName;
        }
	}

	public struct Property
	{
        public string name;
        public string value;
        public bool isSigned;
        public string signature;

        public Property(string name, string value, bool isSigned, string signature)
        {
            this.name = name;
            this.value = value;
            this.isSigned = isSigned;
            this.signature = signature;
        }
	}

	public struct UpdateGamemodeAction
	{
        public Guid guid { get; }
        public GameMode gameMode { get; }

        public UpdateGamemodeAction(Guid guid, GameMode gameMode)
        {
            this.guid = guid;
            this.gameMode = gameMode;
        }
	}

    public struct UpdateLatencyAction
    {
        public Guid guid { get; }
        public int ping { get; }

        public UpdateLatencyAction(Guid guid, int ping)
        {
            this.guid = guid;
            this.ping = ping;
        }
    }

    public struct UpdateDisplayNameAction
    {
        public Guid guid { get; }
        public bool hasDisplayName { get; }
        public string displayName { get; }

        public UpdateDisplayNameAction(Guid guid, bool hasDisplayName, string displayName)
        {
            this.guid = guid;
            this.hasDisplayName = hasDisplayName;
            this.displayName = displayName;
        }
    }

    public struct RemovePlayerAction
    {
        public Guid guid { get; }

        public RemovePlayerAction(Guid guid)
        {
            this.guid = guid;
        }
    }

    public enum MessageAction
    {
        AddPlayer,
        UpdateGameMode,
        UpdateLatency,
        UpdateDisplayName,
        RemovePlayer
    }
}
