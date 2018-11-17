using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	[Header("Prefabs")]
	public GameObject PlayerPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void Awake()
	{
		// make sure the game manager stays when we change the level
		DontDestroyOnLoad(this);
	}

	/// <summary>
	/// Signals the game manager to connect to a server
	/// </summary>
	public void ConnectToServer(string hostname, int port)
	{

	}
}

public enum GameMode : int
{
	SURVIVAL = 0,
	CREATIVE = 1,
	ADVENTURE = 2,
	SPECTATOR = 3
}

public enum Difficulty : int
{
	PEACEFUL = 0,
	EASY = 1,
	MEDIUM = 2,
	HARD = 3
}

public enum LevelType
{
	DEFAULT,
	FLAT,
	LARGE_BIOMES,
	AMPLIFIED,
	DEFAULT_1_1
}
