using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour {
	public Text TxText;
	public Text RxText;
	public Text Ticks;
	public Text Position;

	public PlayerController Player = null;
	public int TxPackets = 0;
	public int RxPackets = 0;
	public int TickCount = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		TxText.text = $"Tx: {TxPackets.ToString()}";
		RxText.text = $"Recv: {RxPackets.ToString()}";
		Ticks.text = $"Client Ticks: {TickCount.ToString()}";
		if (Player != null)
		{
			Position.text = $"({Player.BlockPos.ToString()}); ({Player.BlockPos.GetPosWithinChunk().ToString()}) in ({Player.BlockPos.GetChunk().ToString()})";
		}
	}
}
