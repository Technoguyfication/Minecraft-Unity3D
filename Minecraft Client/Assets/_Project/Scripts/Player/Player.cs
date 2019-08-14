using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a player on the server. Not physically, that's <see cref="EntityPlayer"/>
/// </summary>
class Player
{
	public bool HasDisplayName { get; set; }
	public string DisplayName { get; set; }
	public Texture Skin { get; set; }
	public byte SkinBitmask { get; set; }
	public GameMode GameMode { get; set; }
	public int Ping { get; set; }
}
