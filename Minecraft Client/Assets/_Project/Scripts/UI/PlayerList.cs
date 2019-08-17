using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
	public GameObject PlayerListPanel;
	public KeyCode PlayerListKey = KeyCode.Tab;

    public PlayerListEntry playerListEntryPrefab;

    private bool currentlyActive = false;

    private Dictionary<Guid, PlayerListEntry> entries = new Dictionary<Guid, PlayerListEntry>();

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
            currentlyActive = true;
        }
        else if (Input.GetKeyUp(PlayerListKey))
        {
            PlayerListPanel.SetActive(false);
            currentlyActive = false;
        }

        if (currentlyActive)
        {
            Dictionary<Guid, Player> players = PlayerLibrary.GetPlayers();
            HashSet<Guid> playersToRemove = new HashSet<Guid>(entries.Keys);
            foreach (KeyValuePair<Guid, Player> entry in players)
            {
                if (playersToRemove.Contains(entry.Key))
                    playersToRemove.Remove(entry.Key);

                if (!entries.ContainsKey(entry.Key))
                {
                    entries.Add(entry.Key, Instantiate(playerListEntryPrefab, PlayerListPanel.transform));
                }
            }

            foreach (Guid entry in playersToRemove)
            {
                Destroy(entries[entry].gameObject);
                entries.Remove(entry);
            }

            foreach (KeyValuePair<Guid, PlayerListEntry> entry in entries)
            {
                Player player = players[entry.Key];
                entry.Value.SetValues((player.hasDisplayName) ? player.displayName : player.name, player.ping);
            }
        }
	}
}
