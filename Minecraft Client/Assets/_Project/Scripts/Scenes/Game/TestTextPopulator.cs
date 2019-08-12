using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestTextPopulator : MonoBehaviour
{
	public TextMeshProUGUI Text;
	public float TextTime = 2f;

	private const string ChatMsgPrefix = "\n<font=\"Minecraft Regular SDF\"><mark=#00000065>";
	private float lastPopulateTime = 0f;

	// Start is called before the first frame update
	void Start()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	// Update is called once per frame
	void Update()
	{
		if (lastPopulateTime + TextTime < Time.time)
		{
			PopulateText(Time.time.ToString());
			lastPopulateTime = Time.time;
		}
	}

	void PopulateText(string text)
	{
		Text.text += ChatMsgPrefix + text;
	}

	void HandleLog(string logString, string stackTrace, LogType type)
	{
		string color = "white";

		switch (type)
		{
			case LogType.Warning:
				color = "yellow";
				break;
			case LogType.Error:
			case LogType.Exception:
			case LogType.Assert:
				color = "red";
				break;
		}

		PopulateText($"<color={color}>{logString}</color>");
	}
}
