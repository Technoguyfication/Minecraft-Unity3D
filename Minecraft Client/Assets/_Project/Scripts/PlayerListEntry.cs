using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListEntry : MonoBehaviour
{
    private Text displayText;

    void Awake()
    {
        displayText = GetComponentInChildren<Text>();
    }

    public void SetValues(string displayName, int ping)
    {
        displayText.text = $"{displayName} - {ping}ms";
    }
}
