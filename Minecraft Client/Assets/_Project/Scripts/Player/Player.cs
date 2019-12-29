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
	public delegate void SkinDataUpdatedEventHandler(object sender, PlayerSkinUpdatedEventArgs e);
	public event SkinDataUpdatedEventHandler SkinDataUpdated;

	/// <summary>
	/// Player name if it has formatting (colors, etc.)
	/// </summary>
	public ChatComponent DisplayName { get; set; }

	/// <summary>
	/// The skin data of the player. To update, use <see cref="UpdateSkinData(PlayerSkinData)"/>
	/// </summary>
	public PlayerSkinData SkinTexture { get; private set; }

	public byte SkinBitmask { get; set; }
	public GameMode GameMode { get; set; }
	public int Ping { get; set; }
	public string Name { get; set; }
	public Guid UUID { get; set; }

	/// <summary>
	/// Updates a player's skin data and invokes <see cref="SkinDataUpdated"/>
	/// </summary>
	/// <param name="skinData"></param>
	public void UpdateSkinData(PlayerSkinData skinData)
	{
		SkinTexture = skinData;
		SkinDataUpdated?.Invoke(this, new PlayerSkinUpdatedEventArgs()
		{
			SkinData = skinData
		});
	}
}

public class PlayerSkinUpdatedEventArgs : EventArgs
{
	public PlayerSkinData SkinData { get; set; }
}

/// <summary>
/// Represent's a player's skin + cape
/// </summary>
public class PlayerSkinData
{
	public Texture2D Body { get; set; }
	public Texture2D Cape { get; set; }
}
