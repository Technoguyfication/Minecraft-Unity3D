using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour
{
	public Text TxText;
	public Text RxText;
	public Text Ticks;
	public Text BlockPosition;
	public Text Position;
	public Text ChunkTime;

	public PlayerController Player = null;
	public int TxPackets = 0;
	public int RxPackets = 0;
	public int TickCount = 0;
	public List<float> AverageChunkTime = new List<float>();

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		TxText.text = $"Tx: {TxPackets.ToString()}";
		RxText.text = $"Recv: {RxPackets.ToString()}";
		Ticks.text = $"Local Ticks: {TickCount.ToString()}";
		if (Player != null)
		{
			BlockPosition.text = $"({Player.BlockPos.ToString()}); or ({Player.BlockPos.GetPosWithinChunk().ToString()}) in chunk ({Player.BlockPos.GetChunk().ToString()})";
			Position.text = $"Pos: {Player.MinecraftPosition} Facing: ({Player.Yaw.ToString("0.00")} / {Player.Pitch.ToString("0.00")})";
		}

		// calculate chunk time
		if (AverageChunkTime.Count > 0)
		{
			ChunkTime.text = $"Avg chunk gen time: {AverageChunkTime.Average().ToString("0.00")}s";
			if (AverageChunkTime.Count > 25)
			{
				AverageChunkTime.RemoveRange(0, AverageChunkTime.Count - 25);
			}
		}
	}
}
