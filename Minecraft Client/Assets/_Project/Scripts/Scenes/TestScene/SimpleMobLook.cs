using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMobLook : MonoBehaviour
{
	public Mob TestMob;
	public float CycleSpeed = 5f;
	private float _pitch;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		_pitch = (CycleSpeed * Time.time);
		_pitch %= 180;
		_pitch -= 90;
		TestMob.Pitch = _pitch;
	}
}
