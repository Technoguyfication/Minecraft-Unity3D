using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour {

	public Text SubtitleText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateSubtitleText(string text)
	{
		Debug.Log($"Loading screen: {text}");
		SubtitleText.text = text;
	}
}
