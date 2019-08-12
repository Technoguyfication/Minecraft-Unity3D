﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour
{
	public Text DebugText;
	public Canvas Canvas;

	[Space(20)]

	public string ConnectedHostname = null;
	public int? ConnectedPort = null;
	public PlayerController Player = null;
	public int TxPackets = 0;
	public int RxPackets = 0;
	public int TickCount = 0;
	public List<float> AverageChunkTime = new List<float>();
	public int QueuedChunks = 0;
	public int FinishedChunks = 0;

	public bool Displaying = false;

	// Use this for initialization
	void Start()
	{
		Canvas.enabled = Displaying;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			Displaying = !Displaying;
			Canvas.enabled = Displaying;

			Debug.Log($"Debug canvas {(Displaying ? "enabled" : "disabled")}.");
		}

		if (Displaying)
		{
			var textBuilder = new StringBuilder();

			// game version
			textBuilder.AppendLine($"Minecraft Client Unity v{PlayerSettings.bundleVersion}");

			// connected server
			if (ConnectedHostname != null && ConnectedPort != null)
			{
				textBuilder.AppendLine($"Connected to {ConnectedHostname}:{((int)ConnectedPort).ToString()}");
				textBuilder.AppendLine($"Tx: {TxPackets.ToString()}");
				textBuilder.AppendLine($"Recv: {RxPackets.ToString()}");
			}

			textBuilder.AppendLine($"Local Ticks: {TickCount.ToString()}");

			// player stats
			if (Player != null)
			{
				textBuilder.AppendLine($"({Player.BlockPos.ToString()}); or ({Player.BlockPos.GetPosWithinChunk().ToString()}) in chunk ({Player.BlockPos.GetChunk().ToString()})");
				textBuilder.AppendLine($"Pos: {Player.MinecraftPosition} Facing: ({Player.Yaw.ToString("0.0")} / {Player.Pitch.ToString("0.0")})");
			}

			// calculate chunk time
			if (AverageChunkTime.Count > 0)
			{
				textBuilder.AppendLine($"Chunk gen: {AverageChunkTime.Average().ToString("0.0000")}s over 25 samples; Q: {QueuedChunks}; F: {FinishedChunks}");

				if (AverageChunkTime.Count > 25)
				{
					AverageChunkTime.RemoveRange(0, AverageChunkTime.Count - 25);
				}
			}

			DebugText.text = textBuilder.ToString();
		}
	}
}
