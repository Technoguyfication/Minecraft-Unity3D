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
        PacketID = (int)ClientboundIDs.ENTITY_METADATA;
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
                    PacketReader.ReadByte(reader, out byte indexKey);
                    while (indexKey != 0xFF)
                    {
                        PacketReader.ReadVarInt(reader, out int type);

                        switch ((EntityMetadataEntry.MetadataType)type)
                        {
                            case EntityMetadataEntry.MetadataType.Byte:
                                PacketReader.ReadByte(reader, out byte byteResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { byteValue = byteResult });
                                break;
                            case EntityMetadataEntry.MetadataType.VarInt:
                                PacketReader.ReadVarInt(reader, out int varIntResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { varIntValue = varIntResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Float:
                                PacketReader.ReadFloat(reader, out float floatResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { floatValue = floatResult });
                                break;
                            case EntityMetadataEntry.MetadataType.String:
                                PacketReader.ReadString(reader, out string stringResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { stringValue = stringResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Chat:
                                PacketReader.ReadString(reader, out string chatResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { stringValue = chatResult });
                                break;
                            case EntityMetadataEntry.MetadataType.OptChat:
                                PacketReader.ReadBoolean(reader, out bool optBoolChatResult);

                                string optChatResult = null;
                                if (optBoolChatResult)
                                {
                                    PacketReader.ReadString(reader, out optChatResult);
                                }

                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { boolValue = optBoolChatResult, stringValue = optChatResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Slot:
                                throw new NotImplementedException("NBT Data is not yet supported");
                                //PacketReader.ReadSlotData(reader, out SlotData slotDataResult);
                                //metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { slotDataValue = slotDataResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Boolean:
                                PacketReader.ReadBoolean(reader, out bool booleanResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { boolValue = booleanResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Rotation:
                                PacketReader.ReadRotation(reader, out Vector3 rotationResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { rotationValue = rotationResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Position:
                                PacketReader.ReadPosition(reader, out Position positionResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { positionValue = positionResult });
                                break;
                            case EntityMetadataEntry.MetadataType.OptPosition:
                                PacketReader.ReadBoolean(reader, out bool optBoolPositionResult);

                                Position optPositionResult = new Position();
                                if (optBoolPositionResult)
                                {
                                    PacketReader.ReadPosition(reader, out optPositionResult);
                                }

                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { boolValue = optBoolPositionResult, positionValue = optPositionResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Direction:
                                PacketReader.ReadVarInt(reader, out int directionResult);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { varIntValue = directionResult });
                                break;
                            case EntityMetadataEntry.MetadataType.OptUUID:
                                PacketReader.ReadBoolean(reader, out bool optBoolUuidResult);

                                Guid optUuidResult = new Guid();
                                if (optBoolUuidResult)
                                {
                                    PacketReader.ReadGUID(reader, out optUuidResult);
                                }

                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { boolValue = optBoolUuidResult, guidValue = optUuidResult });
                                break;
                            case EntityMetadataEntry.MetadataType.OptBlockID:
                                PacketReader.ReadBoolean(reader, out bool optBoolBlockIDResult);

                                int optBlockIDResult = 0;
                                if (optBoolBlockIDResult)
                                {
                                    PacketReader.ReadVarInt(reader, out optBlockIDResult);
                                }

                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { boolValue = optBoolBlockIDResult, varIntValue = optBlockIDResult });
                                break;
                            case EntityMetadataEntry.MetadataType.NBT:
                                throw new NotImplementedException("NBT Data is not yet supported");
                                //PacketReader.ReadNbtData(reader, out NbtCompound nbtDataResult);
                                //metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { nbtValue = nbtDataResult });
                                break;
                            case EntityMetadataEntry.MetadataType.Particle:
                                PacketReader.ReadParticle(reader, out Particle particle);
                                metaDataEntries.Enqueue(new EntityMetadataEntry(indexKey, (EntityMetadataEntry.MetadataType)type) { particleValue = particle });
                                break;
                            default:
                                break;
                        }

                        PacketReader.ReadByte(reader, out indexKey);
                    }

                    entries = metaDataEntries.ToArray();
                }
            }
            
        }
    }

    public EntityMetadataEntry[] entries;

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("EntityMetadataPacket: ");
        for (int i = 0; i < entries.Length; i++)
        {
            stringBuilder.Append($"[{entries[i].indexKey}, {entries[i].type}]{(entries.Length - 1 == i ? "" : ", ")}");
        }

        return stringBuilder.ToString();
    }

    public struct EntityMetadataEntry
    {
        public byte indexKey;
        public MetadataType type;

        public byte byteValue;
        public int varIntValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public SlotData slotDataValue;
        public Vector3 rotationValue;
        public Position positionValue;
        public Guid guidValue;
        //public NbtCompound nbtValue;
        public Particle particleValue;

        public EntityMetadataEntry(byte indexKey, MetadataType type)
        {
            this.indexKey = indexKey;
            this.type = type;
            byteValue = 0;
            varIntValue = 0;
            floatValue = 0;
            boolValue = false;
            stringValue = null;
            slotDataValue = new SlotData();
            rotationValue = new Vector3();
            positionValue = new Position();
            guidValue = new Guid();
            //nbtValue = null;
            particleValue = new Particle();
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
