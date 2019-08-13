using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour
{
	public const int MAX_CHUNK_TIME_SAMPLES = 1000;

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
	//public int QueuedChunks = 0;
	public int ProcessingChunks = 0;
	public int FinishedChunks = 0;
	public int LifetimeFinishedChunks = 0;

	public bool Displaying = false;

	private int _currentFps = 0;

	// Use this for initialization
	void Start()
	{
#if UNITY_EDITOR
		Displaying = true;
#endif

		Canvas.enabled = Displaying;
		StartCoroutine(UpdateFPSCoroutine());
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
			textBuilder.AppendLine($"Minecraft Client Unity v{Application.version}");

			// connected server
			if (ConnectedHostname != null && ConnectedPort != null)
			{
				textBuilder.AppendLine($"Connected to {ConnectedHostname}:{((int)ConnectedPort).ToString()}");
				textBuilder.AppendLine($"Tx: {TxPackets.ToString()}");
				textBuilder.AppendLine($"Recv: {RxPackets.ToString()}");
			}

			textBuilder.AppendLine($"Local Ticks: {TickCount.ToString()}");
			textBuilder.AppendLine($"FPS: {_currentFps} / {((1f / _currentFps) * 1000).ToString("0.00")}ms");

			// player stats
			if (Player != null)
			{
				textBuilder.AppendLine($"({Player.BlockPos.ToString()}); or ({Player.BlockPos.GetPosWithinChunk().ToString()}) in chunk ({Player.BlockPos.GetChunkColumnPos().ToString()})");
				textBuilder.AppendLine($"Pos: {Player.MinecraftPosition} Facing: ({Player.Yaw.ToString("0.0")} / {Player.Pitch.ToString("0.0")})");
			}

			// calculate chunk time
			if (AverageChunkTime.Count > 0)
			{
				if (AverageChunkTime.Count > MAX_CHUNK_TIME_SAMPLES)
				{
					AverageChunkTime.RemoveRange(0, AverageChunkTime.Count - MAX_CHUNK_TIME_SAMPLES);
				}

				textBuilder.AppendLine($"Chunk gen: {AverageChunkTime.Average().ToString("0.0")}ms over {AverageChunkTime.Count} samples; P: {ProcessingChunks.ToString("00")}; Fq: {FinishedChunks.ToString("000")}; F\u029F: {LifetimeFinishedChunks}");
			}

			DebugText.text = textBuilder.ToString();
		}
	}

	private IEnumerator UpdateFPSCoroutine()
	{
		while (true)
		{
			_currentFps = (int)(1f / Time.smoothDeltaTime);
			yield return new WaitForSeconds(0.2f);
		}
	}
}
