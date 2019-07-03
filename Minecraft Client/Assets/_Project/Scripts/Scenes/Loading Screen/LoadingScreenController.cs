using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{

	public GameObject LoadingScreenCanvas;
	public GameObject LoadingScreenCamera;
	public Text SubtitleText;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void HideLoadingScreen()
	{
		LoadingScreenCanvas.SetActive(false);
		Destroy(LoadingScreenCamera);
	}

	public void UpdateSubtitleText(string text)
	{
		Debug.Log($"Loading screen: {text}");
		SubtitleText.text = text;
	}
}
