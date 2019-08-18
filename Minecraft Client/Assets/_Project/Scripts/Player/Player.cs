using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a player on the server. Not physically, that's <see cref="EntityPlayer"/>
/// </summary>
public class Player
{
	public bool HasDisplayName { get; set; }
	public string DisplayName { get; set; }
	public Texture SkinTexture { get; set; }
	public byte SkinBitmask { get; set; }
	public GameMode PlayerGameMode { get; set; }
	public int Ping { get; set; }
    public string Name { get; set; }

    public Player(bool hasDisplayName, string displayName, Texture skin, byte skinBitmask, GameMode gameMode, int ping, string name)
    {
        HasDisplayName = hasDisplayName;
        DisplayName = displayName;
        SkinTexture = skin;
        SkinBitmask = skinBitmask;
        PlayerGameMode = gameMode;
        Ping = ping;
        Name = name;
    }
}
