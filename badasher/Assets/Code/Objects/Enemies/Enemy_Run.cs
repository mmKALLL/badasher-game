﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Run : _Enemy {
	// Basic of enemies
	// Dies to run

	override
	public void OnRunThrough(Player player){
		TakeDamageFromAttack (player);
	}
}
