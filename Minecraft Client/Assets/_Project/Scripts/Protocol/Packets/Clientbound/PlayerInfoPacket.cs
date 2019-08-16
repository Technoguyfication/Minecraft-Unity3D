using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInfoPacket : Packet
{
	public override byte[] Payload { get; set; }

	public PlayerInfoPacket()
	{
		PacketID = (int)ClientboundIDs.PLAYER_INFO;
	}

	public PlayerInfoPacket(PacketData data) : base(data) // packet id should be set correctly if this ctor is used
    {
        using (MemoryStream stream = new MemoryStream(data.Payload))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    messageAction = (MessageAction)ReadVarInt(reader);
                    int playerCount = ReadVarInt(reader);

                    Debug.Log($"Action: {messageAction.ToString()}, Players: {playerCount}");

                    switch (messageAction)
                    {
                        case MessageAction.AddPlayer:
                            Queue<AddPlayerAction> addedPlayers = new Queue<AddPlayerAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = new Guid(reader.ReadBytes(16));

                                string name = reader.ReadString();
                                int propertyCount = ReadVarInt(reader);

                                Queue<Property> properties = new Queue<Property>();
                                for (int prop = 0; prop < propertyCount; prop++)
                                {
                                    string propertyName = reader.ReadString();
                                    string propertyValue = reader.ReadString();
                                    bool isSigned = reader.ReadBoolean();
                                    properties.Enqueue(new Property(propertyName, propertyValue, isSigned, (isSigned) ? reader.ReadString() : ""));
                                }

                                GameMode gameMode = (GameMode)ReadVarInt(reader);
                                int ping = ReadVarInt(reader);
                                bool hasDisplayName = reader.ReadBoolean();
                                string displayName = hasDisplayName ? reader.ReadString() : "";

                                addedPlayers.Enqueue(new AddPlayerAction(guid, name, properties.ToArray(), gameMode, ping, hasDisplayName, displayName));
                                Debug.Log($"Added player with name {name}, with {propertyCount} properties, in game mode {gameMode.ToString()}, with ping {ping}, has display name is {hasDisplayName}, with a display name of {displayName}");
                            }

                            addPlayerActions = addedPlayers.ToArray();

                            break;
                        case MessageAction.UpdateGameMode:
                            Queue<UpdateGamemodeAction> updatedGamemodes = new Queue<UpdateGamemodeAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = new Guid(reader.ReadBytes(16));
                                GameMode gameMode = (GameMode)ReadVarInt(reader);

                                updatedGamemodes.Enqueue(new UpdateGamemodeAction(guid, gameMode));

                                Debug.Log($"Updated gamemode for player");
                            }

                            updateGamemodeActions = updatedGamemodes.ToArray();
                            break;
                        case MessageAction.UpdateLatency:

                            Queue<UpdateLatencyAction> updatedLatencies = new Queue<UpdateLatencyAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = new Guid(reader.ReadBytes(16));
                                int ping = ReadVarInt(reader);

                                updatedLatencies.Enqueue(new UpdateLatencyAction(guid, ping));

                                Debug.Log($"Updated ping for player");
                            }

                            updateLatencyActions = updatedLatencies.ToArray();

                            break;
                        case MessageAction.UpdateDisplayName:

                            Queue<UpdateDisplayNameAction> updatedDisplayNames = new Queue<UpdateDisplayNameAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = new Guid(reader.ReadBytes(16));
                                bool hasDisplayName = reader.ReadBoolean();
                                string displayName = hasDisplayName ? reader.ReadString() : "";

                                updatedDisplayNames.Enqueue(new UpdateDisplayNameAction(guid, hasDisplayName, displayName));

                                Debug.Log($"Updated display name / chat for player");
                            }

                            updateDisplayNameActions = updatedDisplayNames.ToArray();

                            break;
                        case MessageAction.RemovePlayer:

                            Queue<RemovePlayerAction> removedPlayerActions = new Queue<RemovePlayerAction>();

                            for (int i = 0; i < playerCount; i++)
                            {
                                Guid guid = new Guid(reader.ReadBytes(16));

                                removedPlayerActions.Enqueue(new RemovePlayerAction(guid));

                                Debug.Log($"Removed player");
                            }

                            removePlayerActions = removedPlayerActions.ToArray();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
        }
    }

    public MessageAction messageAction;
    public AddPlayerAction[] addPlayerActions;
    public UpdateGamemodeAction[] updateGamemodeActions;
    public UpdateLatencyAction[] updateLatencyActions;
    public UpdateDisplayNameAction[] updateDisplayNameActions;
    public RemovePlayerAction[] removePlayerActions;

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

    private static int ReadVarInt(BinaryReader reader)
    {
        int value = 0, numRead = 0, result = 0;
        byte read;
        while (true)
        {
            read = reader.ReadByte();
            value = (read & 0x7F);
            result |= (value << (7 * numRead));

            numRead++;
            if (numRead > 5)
                throw new UnityException("VarInt too big!");
            if ((read & 0x80) != 128) break;
        }
        return result;
    }
}
