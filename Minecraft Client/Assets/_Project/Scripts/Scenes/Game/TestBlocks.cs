using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBlocks : MonoBehaviour
{
	public bool DestroyOnStart = true;

	// Start is called before the first frame update
	void Start()
	{
		if (DestroyOnStart)
			Destroy(this);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
