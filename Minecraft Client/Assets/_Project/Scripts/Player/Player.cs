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
	public ChatComponent DisplayName { get; set; }
	public Texture SkinTexture { get; set; }
	public byte SkinBitmask { get; set; }
	public GameMode GameMode { get; set; }
	public int Ping { get; set; }
	public string Name { get; set; }
	public Guid UUID { get; set; }
}
