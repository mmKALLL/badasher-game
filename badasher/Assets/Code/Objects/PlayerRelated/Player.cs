﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
	// Stores data and has plethora of methods for interacting with

	PlayerCollisions playerCol;
	PlayerMovement playerMov;
	Rigidbody2D playerRig;
	Animator playerAnimator;

	private int boostPower;
	private float dashCooldown; // use this for the UI element
	private bool airdashAvailable = true;
	private bool jumpDashing = false;
	public enum DashState {none, dash, boostPower}; // none = default run. DON'T ADD states that aren't a type of dash here.
	public enum AirState {ground, air};
	public enum LiveState {alive, invunerable, dead};
	private DashState dashState;
	private AirState airState;
	private LiveState liveState;

	public float dashDistanceRemaining;
	private Vector3 dir;
	private float jumpPower;

	private bool stopFixedUpdate = false;
	private Coroutine dashCooldownStorage;

	#region gets
	public int GetBoostPower(){
		return this.boostPower;
	}

	public float GetDashCooldown(){
		return this.dashCooldown;
	}

	public DashState GetDashState(){
		return this.dashState;
	}

	public AirState GetAirState(){
		return this.airState;
	}

	public LiveState GetLiveState(){
		return this.liveState;

	}
	public bool IsJumpDashing(){
		return this.jumpDashing;
	}
	public bool IsAirdashAvailable(){
		return this.airdashAvailable;
	}
	#endregion

	#region sets
	public void SetDashState (DashState dash){
		this.dashState = dash;
	}
	public void SetAirState (AirState air){
		this.airState = air;
	}
	public void SetJumpDashing (bool varx){
		this.jumpDashing = varx;
	}
	#endregion

	public void Awake(){
		dashCooldown = PlayerConstants.DASH_COOLDOWN;
		StartCoroutine (DashCooldownReduce ());
		playerMov = this.GetComponent<PlayerMovement> ();
		playerCol = this.GetComponent<PlayerCollisions> ();
		playerRig = this.GetComponent<Rigidbody2D> ();
		playerAnimator = this.GetComponent<Animator> ();
	}

	public void Start(){
		boostPower = PlayerConstants.BOOST_POWER_DEFAULT;
		airState = AirState.air;
	}

	#region fixedUpdate
	public void FixedUpdate(){

		// TODO Hacked together something that checks if dead.
		if (this.transform.position.y < -25 + 7) {
			this.TakeDamage(9999);
		}

		//Debug.Log (dashState + " + " + airState);
		//Debug.Log("ddRemain " + dashDistanceRemaining);
		if (!stopFixedUpdate) {
			switch (dashState) {
			case DashState.none: // basic run
				switch (airState) {
				case AirState.ground:
					playerMov.PlayerRun (this, playerRig);
					break;
				case AirState.air:
					playerMov.PlayerFall (this, playerRig);
					break;
				}
				break;
			case DashState.dash:
				switch (airState) {
				case AirState.ground:
					playerMov.PlayerDashUpdate(this, playerRig, out dashDistanceRemaining, false);
					break;
				case AirState.air:
					if (jumpDashing) {
						playerMov.PlayerJumpDash(this, playerRig, dir, jumpPower, out dashDistanceRemaining, false);
					} else {
						playerMov.PlayerDashUpdate(this, playerRig, out dashDistanceRemaining, false);
					}
					break;
				}
				break;
			case DashState.boostPower:
			/*switch (airState) {
			case AirState.ground:
				break;
			case AirState.air:
				break;
			}*/
				if (jumpDashing) {
					playerMov.PlayerJumpDash(this, playerRig, dir, jumpPower, out dashDistanceRemaining, true);
				} else {
					playerMov.PlayerDashUpdate(this, playerRig, out dashDistanceRemaining, true);
				}
				break;
			}
		}
	}
	#endregion

	public IEnumerator PlayerPauseMovement(float slowdownLength){
		stopFixedUpdate = true;
		yield return new WaitForSeconds(slowdownLength);
		stopFixedUpdate = false;
	}

	#region change states
	public void PlayerLand (){
		this.jumpDashing = false;
		this.airState = AirState.ground;
		playerAnimator.SetBool ("InAir", false);
		this.airdashAvailable = true;
		TurnKinematic ();
	}

	public void PlayerHitRamp (){
		Debug.Log ("IN player hitting ramp");
		ResetAirdash ();
		this.jumpDashing = true;
		this.airState = AirState.air;
		playerAnimator.SetBool ("InAir",true);
		dir = CalculationLibrary.CalculateDashJumpDir(dashDistanceRemaining);
		bool boostPowerBool;
		if (this.dashState == DashState.boostPower) {
			boostPowerBool = true;
		} else {
			boostPowerBool = false;
		}
		TurnKinematic ();
		jumpPower = CalculationLibrary.CalculateDashJumpPower(dashDistanceRemaining, boostPowerBool);
		dashDistanceRemaining += PlayerConstants.DASH_DISTANCE * PlayerConstants.JUMP_DASH_DASHDISTANCE_ADD_PERCENTAGE;
	}

	public void PlayerDash (){
		// called to do everything dash related
		Debug.Log("DASH!");
		if (this.airState == AirState.air) {
			if (airdashAvailable == true) {
				Debug.Log ("AIRDASH!");
				airdashAvailable = false;
			} else {
				Debug.Log ("No airdash available!");
				return;
			}
		} else {
			this.dashCooldown = PlayerConstants.DASH_COOLDOWN;
		}
		this.jumpDashing = false;
		this.dashState = DashState.dash;
		playerAnimator.SetBool ("Dashing", true);
		dashDistanceRemaining = PlayerConstants.DASH_DISTANCE; 	// doesn't actually matter, unless player goes to DashJump on 
																// the same frame he starts dashing which shouldn't happen anyway
	}

	public void PlayerBoostPower (){
		if (SpendBoostPower()) {
			ResetAirdash ();
			Debug.Log ("BoostPowerDash");
			this.dashCooldown = 0;
			this.jumpDashing = false;
			this.dashState = DashState.boostPower;
			playerAnimator.SetBool ("BoostPower", true);
			dashDistanceRemaining = PlayerConstants.BOOST_POWER_DISTANCE;
		}
	}

	public void PlayerDashEnd (){
		this.jumpDashing = false;
		StartCoroutine (PlayerPauseMovement (PlayerConstants.DASH_GROUND_END_LAG));
		if (dashState != DashState.boostPower) {
			if (dashCooldownStorage != null) {
				StopCoroutine (dashCooldownStorage);
			}
			dashCooldownStorage = StartCoroutine(DashCooldownReduce());
		}
		this.dashState = DashState.none;
		playerAnimator.SetBool ("BoostPower", false);
		playerAnimator.SetBool ("Dashing", false);
	}
	#endregion


	#region interact with variables
	public void GainBoostPower (int gainAmount){
		this.boostPower += gainAmount;
		if (boostPower > PlayerConstants.BOOST_POWER_MAX) {
			boostPower = PlayerConstants.BOOST_POWER_MAX;
		}
	}

	public void TakeDamage (int damageAmount){
		if (this.liveState != LiveState.invunerable) {
			boostPower -= damageAmount;
			if (boostPower < 0) {
				playerRig.velocity = Vector2.zero;
				dashState = DashState.none;
				airState = AirState.air;
				airdashAvailable = true;
				if (this.transform.position.x < 800) {
					this.transform.position = new Vector3 (0, 1, 0); // The clipping is intentional.
				} else {
					this.transform.position = new Vector3 (800.0f * Mathf.Floor(this.transform.position.x / 800.0f), 30, 0); // The clipping is intentional?
				}
				this.boostPower = PlayerConstants.BOOST_POWER_DEFAULT;
				//this.liveState = LiveState.dead;
				//SceneManager.LoadScene (0); // TODO FIXME Change this to another scene.
			}
		}
	}

	private IEnumerator DamageInvunerability (){
		this.liveState = LiveState.invunerable;
		yield return new WaitForSeconds (PlayerConstants.DAMAGE_INVUNERABILITY_TIME);
		this.liveState = LiveState.alive;
	}

	public bool SpendBoostPower (){
		if (this.boostPower >= PlayerConstants.BOOST_POWER_COST) {
			this.boostPower -= PlayerConstants.BOOST_POWER_COST;
			return true;
		}
		return false;
	}
	#endregion

	private IEnumerator DashCooldownReduce (){
		while (dashCooldown > 0) {
			dashCooldown -= Time.deltaTime;
			yield return null;
		}
		Debug.Log (dashCooldown);
		yield return new WaitForEndOfFrame();
	}

	public void TurnKinematic(){
		playerRig.bodyType = RigidbodyType2D.Kinematic;
		playerRig.velocity = Vector2.zero;
	}

	private void ResetAirdash(){
		airdashAvailable = true;
	}
}
