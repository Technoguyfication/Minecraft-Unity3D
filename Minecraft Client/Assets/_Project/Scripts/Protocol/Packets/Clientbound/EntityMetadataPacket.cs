using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class EntityMetadataPacket : Packet
{
	public EntityMetadataPacket()
	{
		PacketID = (int)ClientboundIDs.EntityMetadata;
	}

	public EntityMetadataPacket(PacketData data) : base(data) { }

	public override byte[] Payload
	{
		get => throw new NotImplementedException();
		set
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					Queue<EntityMetadataEntry> metaDataEntries = new Queue<EntityMetadataEntry>();
					byte indexKey = PacketReader.ReadByte(reader);
					while (indexKey != 0xFF)
					{
						int type = PacketReader.ReadVarInt(reader);

						switch ((EntityMetadataEntry.MetadataType)type)
						{
							case EntityMetadataEntry.MetadataType.Byte:
								byte byteResult = PacketReader.ReadByte(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { ByteValue = byteResult });
								break;
							case EntityMetadataEntry.MetadataType.VarInt:
								int varIntResult = PacketReader.ReadVarInt(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { VarIntValue = varIntResult });
								break;
							case EntityMetadataEntry.MetadataType.Float:
								float floatResult = PacketReader.ReadSingle(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { FloatValue = floatResult });
								break;
							case EntityMetadataEntry.MetadataType.String:
								string stringResult = PacketReader.ReadString(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { StringValue = stringResult });
								break;
							case EntityMetadataEntry.MetadataType.Chat:
								string chatResult = PacketReader.ReadString(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { StringValue = chatResult });
								break;
							case EntityMetadataEntry.MetadataType.OptChat:
								bool optBoolChatResult = PacketReader.ReadBoolean(reader);

								string optChatResult = null;
								if (optBoolChatResult)
								{
									optChatResult = PacketReader.ReadString(reader);
								}

								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { BoolValue = optBoolChatResult, StringValue = optChatResult });
								break;
							case EntityMetadataEntry.MetadataType.Slot:
								throw new NotImplementedException("NBT Data is not yet supported");
							//PacketReader.ReadSlotData(reader, out SlotData slotDataResult);
							//metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { slotDataValue = slotDataResult });
							//break;
							case EntityMetadataEntry.MetadataType.Boolean:
								bool booleanResult = PacketReader.ReadBoolean(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { BoolValue = booleanResult });
								break;
							case EntityMetadataEntry.MetadataType.Rotation:
								Vector3 rotationResult = PacketReader.ReadRotation(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { RotationValue = rotationResult });
								break;
							case EntityMetadataEntry.MetadataType.Position:
								BlockPos positionResult = PacketReader.ReadPosition(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { PositionValue = positionResult });
								break;
							case EntityMetadataEntry.MetadataType.OptPosition:
								bool optBoolPositionResult = PacketReader.ReadBoolean(reader);

								BlockPos optPositionResult = new BlockPos();
								if (optBoolPositionResult)
								{
									optPositionResult = PacketReader.ReadPosition(reader);
								}

								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { BoolValue = optBoolPositionResult, PositionValue = optPositionResult });
								break;
							case EntityMetadataEntry.MetadataType.Direction:
								int directionResult = PacketReader.ReadVarInt(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { VarIntValue = directionResult });
								break;
							case EntityMetadataEntry.MetadataType.OptUUID:
								bool optBoolUuidResult = PacketReader.ReadBoolean(reader);

								Guid optUuidResult = new Guid();
								if (optBoolUuidResult)
								{
									optUuidResult = PacketReader.ReadGuid(reader);
								}

								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { BoolValue = optBoolUuidResult, GuidValue = optUuidResult });
								break;
							case EntityMetadataEntry.MetadataType.OptBlockID:
								bool optBoolBlockIDResult = PacketReader.ReadBoolean(reader);

								int optBlockIDResult = 0;
								if (optBoolBlockIDResult)
								{
									optBlockIDResult = PacketReader.ReadVarInt(reader);
								}

								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { BoolValue = optBoolBlockIDResult, VarIntValue = optBlockIDResult });
								break;
							case EntityMetadataEntry.MetadataType.NBT:
								throw new NotImplementedException("NBT Data is not yet supported");
							//PacketReader.ReadNbtData(reader, out NbtCompound nbtDataResult);
							//metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { nbtValue = nbtDataResult });
							//break;
							case EntityMetadataEntry.MetadataType.Particle:
								Particle particle = PacketReader.ReadParticle(reader);
								metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { ParticleValue = particle });
								break;
							default:
								break;
						}

						indexKey = PacketReader.ReadByte(reader);
					}

					Entries = metaDataEntries.ToArray();
				}
			}

		}
	}

	public EntityMetadataEntry[] Entries;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("EntityMetadataPacket: ");
		for (int i = 0; i < Entries.Length; i++)
		{
			stringBuilder.Append($"[{Entries[i].IndexKey}, {Entries[i].MetaType}]{(Entries.Length - 1 == i ? "" : ", ")}");
		}

		return stringBuilder.ToString();
	}

	public struct EntityMetadataEntry
	{
		public byte IndexKey;
		public MetadataType MetaType;

		public byte ByteValue;
		public int VarIntValue;
		public float FloatValue;
		public bool BoolValue;
		public string StringValue;
		public SlotData SlotDataValue;
		public Vector3 RotationValue;
		public BlockPos PositionValue;
		public Guid GuidValue;
		//public NbtCompound nbtValue;
		public Particle ParticleValue;

		public EntityMetadataEntry(byte indexKey, MetadataType type)
		{
			IndexKey = indexKey;
			MetaType = type;
			ByteValue = 0;
			VarIntValue = 0;
			FloatValue = 0;
			BoolValue = false;
			StringValue = null;
			SlotDataValue = new SlotData();
			RotationValue = new Vector3();
			PositionValue = new BlockPos();
			GuidValue = new Guid();
			//nbtValue = null;
			ParticleValue = new Particle();
		}

		public enum MetadataType
		{
			Byte = 0,
			VarInt = 1,
			Float = 2,
			String = 3,
			Chat = 4,
			OptChat = 5,
			Slot = 6,
			Boolean = 7,
			Rotation = 8,
			Position = 9,
			OptPosition = 10,
			Direction = 11,
			OptUUID = 12,
			OptBlockID = 13,
			NBT = 14,
			Particle = 15
		}
	}
}
