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
	public bool hasDisplayName { get; set; }
	public string displayName { get; set; }
	public Texture skin { get; set; }
	public byte skinBitmask { get; set; }
	public GameMode gameMode { get; set; }
	public int ping { get; set; }
    public string name { get; set; }

    public Player(bool hasDisplayName, string displayName, Texture skin, byte skinBitmask, GameMode gameMode, int ping, string name)
    {
        this.hasDisplayName = hasDisplayName;
        this.displayName = displayName;
        this.skin = skin;
        this.skinBitmask = skinBitmask;
        this.gameMode = gameMode;
        this.ping = ping;
        this.name = name;
    }
}
