using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
	public GameObject PlayerListPanel;
	public KeyCode PlayerListKey = KeyCode.Tab;

	public PlayerEntry playerListEntryPrefab;

	private readonly Dictionary<Guid, PlayerEntry> entries = new Dictionary<Guid, PlayerEntry>();

	// Start is called before the first frame update
	void Start()
	{
		PlayerListPanel.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(PlayerListKey))
		{
			PlayerListPanel.SetActive(true);
		}
		else if (Input.GetKeyUp(PlayerListKey))
		{
			PlayerListPanel.SetActive(false);
		}
	}

	public void AddPlayerInfo(PlayerInfoPacket pkt)
	{

	}
}
