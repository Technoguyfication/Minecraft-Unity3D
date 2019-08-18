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

	public Action[] Actions { get; set; }
	public ActionType Type { get; set; }
	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					Type = (ActionType)PacketReader.ReadVarInt(reader);
					int actionCount = PacketReader.ReadVarInt(reader);
					Actions = new Action[actionCount];

					for (int i = 0; i < Actions.Length; i++)
					{
						var action = new Action
						{
							UUID = PacketReader.ReadGuid(reader)
						};

						switch (Type)
						{
							case ActionType.AddPlayer:
								action.Name = PacketReader.ReadString(reader);

								// read property array
								action.Properties = new Property[PacketReader.ReadVarInt(reader)];
								for (int j = 0; j < action.Properties.Length; j++)
								{
									var prop = new Property
									{
										Name = PacketReader.ReadString(reader),
										Value = PacketReader.ReadString(reader),
										Signed = PacketReader.ReadBoolean(reader)
									};

									// signature only exists if Signed = true
									if (prop.Signed)
									{
										prop.Signature = PacketReader.ReadString(reader);
									}
								}

								action.GameMode = (GameMode)PacketReader.ReadVarInt(reader);
								action.Ping = PacketReader.ReadVarInt(reader);
								
								// displayname only exists if HasDisplayName = true
								if (PacketReader.ReadBoolean(reader))   // read has display name
								{
									string displayNameJson = PacketReader.ReadString(reader);
									action.DisplayName = ChatComponent.FromJson(displayNameJson);
								}
								break;
							case ActionType.UpdateGameMode:
								action.GameMode = (GameMode)PacketReader.ReadVarInt(reader);
								break;
							case ActionType.UpdateLatency:
								action.Ping = PacketReader.ReadVarInt(reader);
								break;
							case ActionType.UpdateDisplayName:
								// displayname only exists if HasDisplayName = true
								if (PacketReader.ReadBoolean(reader))	// read has display name
								{
									string displayNameJson = PacketReader.ReadString(reader);
									action.DisplayName = ChatComponent.FromJson(displayNameJson);
								}
								break;
							case ActionType.RemovePlayer:
								break;
							default:
								throw new Exception($"Unknown PlayerInfo action type: {Type}");
						}

						Actions[i] = action;
					}
				}
			}
		}
	}

	/// <summary>
	/// Class for a player update action https://wiki.vg/Protocol#Player_Info
	/// </summary>
	public partial class Action
	{
		public Guid UUID { get; set; }
		public string Name { get; set; }
		public Property[] Properties { get; set; }
		public GameMode GameMode { get; set; }
		public int Ping { get; set; }
		public ChatComponent DisplayName { get; set; } = null;
	}

	public class Property
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public bool Signed { get; set; }
		public string Signature { get; set; }
	}

	public enum ActionType
	{
		AddPlayer = 0,
		UpdateGameMode = 1,
		UpdateLatency = 2,
		UpdateDisplayName = 3,
		RemovePlayer = 4
	}
}
