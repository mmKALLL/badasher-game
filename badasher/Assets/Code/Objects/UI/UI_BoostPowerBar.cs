﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BoostPowerBar : MonoBehaviour {

	// TODO
	// Boostpower bar that updates when boost power is changed

	Player player;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
