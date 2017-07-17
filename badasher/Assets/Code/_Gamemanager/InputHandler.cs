﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

	private Player player;
	float timeAtLastButtonPress;

	//private float timeAtLastDash;
	private Coroutine waitForInputsStorage;

	public void Start (){
		player = this.GetComponent<Player> ();
		waitForInputsStorage = StartCoroutine(WaitForDashButtons());
	}

	public void EndThis (){
		StopAllCoroutines ();
	}

	private IEnumerator WaitForDashButtons (){ // main ienumerator
		while (true){
			yield return WaitForDashButton();
			timeAtLastButtonPress = Time.realtimeSinceStartup;
			if (player.IsJumpDashing ()) { // cancel jumpDash into AirDash
				player.PlayerDash ();
			} else if ((player.GetAirState () == Player.AirState.air) && (player.IsAirdashAvailable ())) { // cancel falling into AirDash
				player.PlayerDash ();
			} else if (player.GetDashState () == Player.DashState.dash) { // cancel dash to PowerBoost
				player.PlayerBoostPower ();
			} else if (player.GetDashCooldown () <= 0 && (player.GetDashState () != Player.DashState.boostPower)) { // do dash when no cooldown left and not in boostPower
				player.PlayerDash ();
			} else if (player.GetDashState () != Player.DashState.boostPower && (Time.realtimeSinceStartup - timeAtLastButtonPress) < PlayerConstants.BOOST_POWER_INPUT_BUFFER) {
				player.PlayerBoostPower ();
			}
		}
	}

	private IEnumerator WaitForDashButton (){
		while (!Input.GetKeyDown (KeyCode.D) && !Input.GetKeyDown (KeyCode.Space) && !Input.GetKeyDown (KeyCode.RightArrow) && !Input.GetKeyDown (KeyCode.Escape)) {
			yield return null;
		}
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit();
		Debug.Log ("KEY GOT!");
		yield return new WaitForFixedUpdate ();
	}
}
