using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct SlotData
{
    public bool present;
    public int itemId;
    public byte itemCount;
    //public NbtTag nbtData;

    public SlotData(bool present, int itemId, byte itemCount)//, NbtTag nbtData)
    {
        this.present = present;
        this.itemId = itemId;
        this.itemCount = itemCount;
        //this.nbtData = nbtData;
    }
}
