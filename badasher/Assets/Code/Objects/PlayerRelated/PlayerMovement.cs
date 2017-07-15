﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	// Contains methods (for FixedUpdate) for dashing, jumping, airdashing and running

	public void PlayerDashUpdate (Player player, Rigidbody2D playerRig, out float dashDistanceRemaining, bool boostPower){
		if (player.dashDistanceRemaining <= 0) {
			player.PlayerEndDash ();
			dashDistanceRemaining = 0;
			return;
		}
		float constDashMod;
		if (boostPower) {
			constDashMod = PlayerConstants.BOOST_POWER_SPEED;
		} else {
			constDashMod = PlayerConstants.DASH_SPEED;
		}
		playerRig.MovePosition (transform.position + Vector3.right * constDashMod * Time.fixedDeltaTime);
	}
		

	public void PlayerJump (Player player, Rigidbody2D playerRig, Vector3 vec) { // vec should include direction and power
		playerRig.AddForce (vec * PlayerConstants.JUMP_POWER);
	}

	// calculate jump modifier in calculationLibrary
	public void PlayerJumpDash (Player player, Rigidbody2D playerRig, Vector3 dir, float jumpPower, out float dashDistanceRemaining){
		if (player.dashDistanceRemaining <= 0) {
			player.PlayerEndDash ();
			PlayerJump(player, playerRig, dir*jumpPower);
			dashDistanceRemaining = 0;
			return;
		}
		float moveAmount = PlayerConstants.DASH_SPEED * Time.fixedDeltaTime;
		if (moveAmount > player.dashDistanceRemaining) {
			moveAmount = player.dashDistanceRemaining;
		}
		playerRig.MovePosition(transform.position + dir * (moveAmount));
		dashDistanceRemaining = player.dashDistanceRemaining-moveAmount;
	}

	public void PlayerFall (Player player, Rigidbody2D playerRig){
		// nothing, let the gravity do its work.
	}

	public void PlayerRun (Player player, Rigidbody2D playerRig){
		playerRig.MovePosition(transform.position + Vector3.right * PlayerConstants.RUN_SPEED * Time.fixedDeltaTime);
	}
}
