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
	public void ConnectToServer()
	{

	}
}
