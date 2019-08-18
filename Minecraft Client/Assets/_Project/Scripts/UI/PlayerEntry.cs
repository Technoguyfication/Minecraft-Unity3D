using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
	public TextMeshProUGUI DisplayName;

	public void UpdateEntry(Player player)
	{
		// set display name
		if (player.DisplayName != null)
			DisplayName.text = ChatComponent.GetFormattedString(player.DisplayName, null);
		else
			DisplayName.text = player.Name;

		DisplayName.text += $" {player.Ping.ToString()}ms";
	}
}
