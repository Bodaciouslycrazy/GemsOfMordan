using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : GEntity, IDamageable {
	public static PlayerController MainPlayer;


	public EnumGem CurGem = EnumGem.NONE;
	public enum EnumGem
	{
		NONE,
		AIR,
		ZAP
	}

	[Header("Movement")]
	public float MaxSpeed = 5f;
	public float GroundAccel = 10f;
	public float AirAccel = 7f;
	//public float GroundAngleTolerance = .01f;
	public float JumpHeight = 8f;
	public float JumpCancelAccel = 100f;

	[Header("HurtKnockback")]
	public float HurtHorzKnockback = 5;
	public float HurtVertKnockback = 5;

	[Header("BasicAttackVariables")]
	public float PunchVerticalVel = 1f;
	public float PunchMaxHorzVel = 1f;

	[Header("FlyAttackVariables")]
	public float FlyYVelocity = 20f;
	public float FlyAttackLength = .85f;
	private bool FlyAttackCharged = true;

	[Header("DiveKiackAttackVariables")]
	public float DivekickSpeed = 20f;
	public float DivekickLength = .70f;
	private bool DivekickCharged = true;

	[Header("LaserAttackVariables")]
	public GameObject LaserPref;
	public float LaserMinChargeTime = 0.2f;
	public float LaserCooldownTime = 0.2f;
	public float LaserTimePerDamage = 0.85f;
	public float LaserProjectileSpeed = 10f;
	public float LaserSoundPitchSpeed = 1f;
	public float LaserSoundMaxPitch = 3f;

	[Header("Other Variables")]
	public List<Hitbox> ReservedHitboxes;

	[Header("Sounds")]
	public AudioClip SPunchMiss;
	public AudioClip SPunchHit;
	public AudioClip SJump;
	public AudioClip SHurt;

	public AudioClip SLaserCharge;
	public AudioClip SLaserFire;

	//************************
	//   PRIVATE VARIABLES
	//************************
	private float Horizontal = 0f;
	private float Vertical = 0f;
	private int NumInputsStored = 0;
	private bool heldJump = false;
	private bool floating = false;
	private bool forceFloating = false;
	private bool Attacking = false;
	private IEnumerator ActionRoutine = null;
	private bool RoutineRequiresEndTrigger = false;

	private AudioSource SoundLoopSource;

	//Action queue stuff
	[Header("ActionQueue")]
	public float MaxQueueTime = .3f;
	private EnumAction QueuedAction = EnumAction.NONE;
	private EnumDir QueuedActionDirection = EnumDir.N;
	private float QueuedActionTime = 0f;
	
	enum EnumAction
	{
		NONE,
		JUMP,
		BASIC_ATTACK,
		SPECIAL_ATTACK,
		PICKUP
	}

	enum EnumDir
	{
		N = 0,
		NE = 1,
		E = 2,
		SE = 3,
		S = 4,
		SW = 5,
		W = 6,
		NW = 7,
		CENTER
	}
	
	//*********************************************************
	//					GETTERS AND SETTERS
	//*********************************************************

	// Use this for initialization
	public override void Start ()
	{
		base.Start();
		MainPlayer = this;
		SoundLoopSource = GetComponent<AudioSource>();
		LoadPlayerState();
		//ReservedHitboxes = new List<Hitbox>();
	}
	
	// Update is called once per frame
	void Update () {
		//UPDATE INPUTS
		//UPDATE THESE TO AVERAGE OVER MULTIPLE FRAMES
		float RawHorz = Input.GetAxisRaw("Horizontal");
		float RawVert = Input.GetAxisRaw("Vertical");

		if (NumInputsStored == 0)
		{
			Horizontal = RawHorz;
			Vertical = RawVert;
		}
		else
		{
			Horizontal = (NumInputsStored * Horizontal + RawHorz) / (NumInputsStored + 1);
			Vertical = (NumInputsStored * Vertical + RawVert) / (NumInputsStored + 1);
		}
		NumInputsStored++;
		heldJump = Input.GetButton("Jump");

		//UPDATE ACTION BASED INPUTS
		if (Input.GetButtonDown("Jump"))
		{
			QueuedAction = EnumAction.JUMP;
			QueuedActionTime = Time.time;
		}
		else if (Input.GetButtonDown("Attack"))
		{
			QueuedAction = EnumAction.BASIC_ATTACK;
			QueuedActionDirection = GetDirection(RawHorz, RawVert);
			QueuedActionTime = Time.time;
		}
		else if(Input.GetButtonDown("Special"))
		{
			QueuedAction = EnumAction.SPECIAL_ATTACK;
			QueuedActionDirection = GetDirection(RawHorz, RawVert);
			QueuedActionTime = Time.time;
		}
		else if(Input.GetButtonDown("Pickup"))
		{
			QueuedAction = EnumAction.PICKUP;
			QueuedActionDirection = GetDirection(RawHorz, RawVert);
			QueuedActionTime = Time.time;
		}
		

		
		if (Horizontal > 0 && !GetFacingRight() && rb.velocity.x > 0 && !Attacking)
		{
			SetFacingRight(true);
		}
		else if (Horizontal < 0 && GetFacingRight() && rb.velocity.x < 0 && !Attacking)
		{
			SetFacingRight(false);
		}

		if(Horizontal == 0f && (rb.velocity.x < 0.01f && rb.velocity.x > -0.01f)) //player standing still
		{
			//set state to idle
			anim.SetInteger("Accel", 0);
		}
		else if( rb.velocity.x * Horizontal > 0) //player is accelerating,
		{
			//set state to running
			anim.SetInteger("Accel", 1);
		}
		else //Player is deccelerating
		{
			//set state to sliding
			anim.SetInteger("Accel", -1);
		}

		anim.SetBool("Falling", rb.velocity.y < 0);
	}


	//******************************************************************************************
	//									FIXED UPDATE
	//******************************************************************************************


	void FixedUpdate()
	{

		//Complete the queued action, if possible.
		if(!Attacking && Time.time - QueuedActionTime < MaxQueueTime)
		{
			//Complete the queued action
			//If an action is completed, make sure to make the QueuedAction NONE so that the action doesn't happen again.
			
			if(QueuedAction == EnumAction.JUMP && GetGroundState() != GroundState.IN_AIR)
			{
				//JUMP
				//Debug.Log("Jumping");
				Jump();
				QueuedAction = EnumAction.NONE;
			}
			else if(QueuedAction == EnumAction.BASIC_ATTACK)
			{
				//Basic attack
				//Debug.Log("Basic Attack");
				if (QueuedActionDirection == EnumDir.E || QueuedActionDirection == EnumDir.NE || QueuedActionDirection == EnumDir.SE)
					SetFacingRight(true);
				else if (QueuedActionDirection == EnumDir.W || QueuedActionDirection == EnumDir.NW || QueuedActionDirection == EnumDir.SW)
					SetFacingRight(false);

				StartAction("Punch");
				QueuedAction = EnumAction.NONE;
			}
			else if(QueuedAction == EnumAction.SPECIAL_ATTACK)
			{
				if (QueuedActionDirection == EnumDir.E || QueuedActionDirection == EnumDir.NE || QueuedActionDirection == EnumDir.SE)
					SetFacingRight(true);
				else if (QueuedActionDirection == EnumDir.W || QueuedActionDirection == EnumDir.NW || QueuedActionDirection == EnumDir.SW)
					SetFacingRight(false);

				//Special Attack
				ChooseSpecial();
			}
			else if(QueuedAction == EnumAction.PICKUP)
			{
				PickupGem();
			}
		}

		//Do state transitions!
		UpdateGroundState();


		//Now that we have completed the action queue, do movement!
		float accelX = 0;
		float accelY = 0;
		if (GetGroundState() == GroundState.WALKING)
		{
			float targetXVel = Horizontal * MaxSpeed;
			if (Attacking) targetXVel = 0;
			float dv = targetXVel - rb.velocity.x;
			accelX = dv / Time.fixedDeltaTime;

			if (accelX > GroundAccel)
				accelX = GroundAccel;
			else if (accelX < -GroundAccel)
				accelX = -GroundAccel;
		}
		else if(GetGroundState() == GroundState.IN_AIR)
		{
			
			float dv = Horizontal * AirAccel * Time.fixedDeltaTime;
			if (Attacking) dv = 0;

			if(Horizontal > 0 && dv + rb.velocity.x > MaxSpeed)
			{
				dv = Mathf.Max(0, MaxSpeed - rb.velocity.x);
				
			}
			else if(Horizontal < 0 && dv + rb.velocity.x < -MaxSpeed)
			{
				dv = Mathf.Min(0, -MaxSpeed - rb.velocity.x);
			}

			accelX = dv / Time.fixedDeltaTime;

			//cancel jump if they let go
			if( rb.velocity.y > 0 && (!heldJump || !floating) && !Attacking && !forceFloating)
			{
				floating = false;
				float dvy = -JumpCancelAccel * Time.fixedDeltaTime;
				if (-dvy > rb.velocity.y)
					dv = -rb.velocity.y;

				accelY = dvy / Time.fixedDeltaTime;
			}
		}

		rb.AddForce(new Vector2(accelX * rb.mass, accelY * rb.mass), ForceMode2D.Force);

		//Update ground variables!
		GroundFrameEnd();

		//Update input average count
		NumInputsStored = 0;
	}

	private void ChooseSpecial()
	{
		if (CurGem == EnumGem.NONE)
			return;
		else if(CurGem == EnumGem.AIR)
		{
			if(IsDirectionAdjacent(QueuedActionDirection, EnumDir.S) && DivekickCharged)
			{
				StartAction("DiveKick");
				QueuedAction = EnumAction.NONE;
			}
			else if (!IsDirectionAdjacent(QueuedActionDirection, EnumDir.S)  && FlyAttackCharged)
			{
				StartAction("Fly");
				QueuedAction = EnumAction.NONE;
			}
		}
		else if(CurGem == EnumGem.ZAP)
		{
			StartAction("Laser");
			QueuedAction = EnumAction.NONE;
		}
	}

	private void Jump()
	{
		// Jumping
		if (GetGroundState() != GroundState.IN_AIR)
		{
			rb.AddForce(new Vector2(0, CalcJumpForce(JumpHeight)), ForceMode2D.Impulse);
			floating = true;

			AudioSource jsound = SoundManager.Singleton.GenerateSound(transform.position, 1.5f);
			jsound.clip = SJump;
			jsound.Play();

			//This anim trigger is not needed. Jump is triggered in FixedUpdate() when the floor is not detected.
			//anim.SetTrigger("Jump");
		}
	}
	
	//***************************************************************************
	//								ACTIONS
	//***************************************************************************

	private void StartAction(string aName)
	{
		//End current action
		if(ActionRoutine != null)
		{
			StopCoroutine(ActionRoutine);
			ActionRoutine = null;
			EndAction();
		}

		if (aName == "Punch")
			ActionRoutine = Punch();
		else if (aName == "Fly")
			ActionRoutine = Fly();
		else if (aName == "DiveKick")
			ActionRoutine = DiveKick();
		else if (aName == "Knockback")
			ActionRoutine = Knockback();
		else if (aName == "Laser")
			ActionRoutine = Laser();

		anim.ResetTrigger("EndAttack");

		if(ActionRoutine != null)
		{
			StartCoroutine(ActionRoutine);
		}
	}

	#region Attack Coroutines

	private IEnumerator Punch()
	{
		Attacking = true;
		anim.SetTrigger("Punch");

		//Air physics are weird.
		if (GetGroundState() == GroundState.IN_AIR)
		{
			Vector2 newVel = rb.velocity;
			if(newVel.y < PunchVerticalVel) newVel.y = PunchVerticalVel;
			if (newVel.x > PunchMaxHorzVel) newVel.x = PunchMaxHorzVel;
			else if (newVel.x < -PunchMaxHorzVel) newVel.x = -PunchMaxHorzVel;
			rb.velocity = newVel;
		}


		//Now, the hitbox!
		float xoffset = 0.7f * (GetFacingRight() ? 1 : -1);
		Vector2 pos = new Vector2( xoffset, -.1f );
		Hitbox hb = ReservedHitboxes[0];

		hb.SetPos(pos, transform);
		hb.SetCollider(.8f, .5f, 0, 16, GetFacingRight());
		hb.PlayAnimation("MageSwipe");

		Collider2D[] results = hb.GetHits();
		bool phit = false;
		for(int i = 0; i < results.Length; i++)
		{
			if (results[i] == null)
				break;

			Health enemyHealth = results[i].GetComponent<Health>();
			if (enemyHealth == null)
				continue;

			phit = true;
			enemyHealth.Hurt(1);
		}

		AudioClip p = phit ? SPunchHit : SPunchMiss;
		AudioSource hbas = SoundManager.Singleton.GenerateSound(hb.transform.position);
		hbas.clip = p;
		hbas.volume = 0.5f;
		hbas.Play();

		yield return null;
	}


	private IEnumerator Fly()
	{
		Attacking = true;
		FlyAttackCharged = false;
		RoutineRequiresEndTrigger = true;
		anim.SetTrigger("Fly");


		//Set velocity upwards
		rb.velocity = new Vector2(0, FlyYVelocity);

		//Hitbox initialization...
		Hitbox hb = ReservedHitboxes[0];
		hb.SetPos(new Vector2(0, .5f), transform);
		hb.SetCollider(2f, 1.5f, 0, 16, GetFacingRight());
		hb.PlayAnimation("FlyAttack");

		List<Health> hitEnemies = new List<Health>();
		float hbTime = FlyAttackLength;

		//Every frame:
		while (hbTime > 0)
		{
			//Add force to combat gravity.
			rb.AddForce(Physics2D.gravity * -1 * rb.mass);

			Collider2D[] results = hb.GetHits();

			for (int i = 0; i < results.Length; i++)
			{
				if (results[i] == null)
					break;


				Health enemy = results[i].GetComponent<Health>();
				if (enemy != null && !hitEnemies.Contains(enemy))
				{
					enemy.Hurt(2);
					hitEnemies.Add(enemy);
					//Debug.Log("Fly hit enemy!");
				}
			}

			hbTime -= Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		//Allow the player to float out of the attack?
		//floating = true;
		rb.velocity = new Vector2(0, 10);
		forceFloating = true;
		hb.StopAnimation();
		anim.SetTrigger("EndAttack");
		RoutineRequiresEndTrigger = false;
		yield return null;
	}

	private IEnumerator DiveKick()
	{
		if (GetGroundState() != GroundState.WALKING)
		{
			Attacking = true;
			DivekickCharged = false;
			RoutineRequiresEndTrigger = true;
			anim.SetTrigger("DiveKick");

			int xmod = GetFacingRight() ? 1 : -1;

			//Set velocity downwards
			rb.velocity = new Vector2(xmod, -1).normalized * DivekickSpeed;

			//Hitbox initialization...
			Hitbox hb = ReservedHitboxes[0];
			hb.SetPos(new Vector2(xmod * 0.25f, -.5f), transform);
			hb.SetCollider(2f, 1f, -45, 16, GetFacingRight());
			hb.PlayAnimation("DiveAttack");

			//List<Health> hitEnemies = new List<Health>();
			bool hitEnemy = false;
			float hbTime = DivekickLength;


			//Every frame:
			while (hbTime > 0)
			{
				//Add force to combat gravity.
				rb.AddForce(Physics2D.gravity * -1 * rb.mass);

				Collider2D[] results = hb.GetHits();

				for (int i = 0; i < results.Length; i++)
				{
					if (results[i] == null)
						break;


					Health enemy = results[i].GetComponent<Health>();
					if (enemy != null) // && !hitEnemies.Contains(enemy))
					{
						enemy.Hurt(2);
						hitEnemy = true;
						break;
						//hitEnemies.Add(enemy);
						//Debug.Log("Fly hit enemy!");
					}
				}

				if (hitEnemy)
					break;

				hbTime -= Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();

				if (GetGroundState() == GroundState.WALKING)
					break;
			}

			if (hitEnemy)
			{
				forceFloating = true;
				rb.velocity = new Vector2(-xmod * 2, 8);
			}
			else
			{
				rb.velocity = Vector2.zero;
			}

			hb.StopAnimation();
			anim.SetTrigger("EndAttack");
			RoutineRequiresEndTrigger = false;

		}
	}

	public IEnumerator Laser()
	{
		Attacking = true;
		anim.SetTrigger("ChargeLaser");
		SoundLoopSource.clip = SLaserCharge;
		SoundLoopSource.volume = 0.75f;
		SoundLoopSource.pitch = 1f;
		SoundLoopSource.Play();

		rb.velocity = new Vector2(0, -.2f);

		float waited = 0f;

		while(Input.GetButton("Special") || waited < 0.15)
		{
			rb.AddForce(Physics2D.gravity * -1 * rb.mass);
			waited += Time.fixedDeltaTime;

			//Add pitch to looping sound
			float pitch = SoundLoopSource.pitch + (Time.fixedDeltaTime * LaserSoundPitchSpeed);
			if (pitch > LaserSoundMaxPitch) pitch = LaserSoundMaxPitch;
			SoundLoopSource.pitch = pitch;

			yield return new WaitForFixedUpdate();
		}

		float xmod = GetFacingRight() ? 1f : -1f;
		int ldam = Mathf.Min(3, (int)(waited / 0.85) + 1);
		GameObject proj = Instantiate(LaserPref, (Vector2)transform.position + new Vector2(0.5f * xmod, 0), Quaternion.identity);
		proj.GetComponent<Rigidbody2D>().velocity = new Vector2(xmod * LaserProjectileSpeed, 0);
		proj.GetComponent<Projectile>().Damage = ldam;

		anim.SetTrigger("Punch");

		SoundLoopSource.Stop();

		AudioSource fas = SoundManager.Singleton.GenerateSound(transform.position, 1f);
		fas.clip = SLaserFire;
		fas.Play();

		//Cooldown wait
		waited = 0;
		while(waited < LaserCooldownTime)
		{
			waited += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		anim.SetTrigger("EndAttack");

	}

	private IEnumerator Knockback()
	{
		//Debug.Log("Knockback started");
		Attacking = true;
		GetComponent<Health>().SetInvincible();

		float xv = HurtHorzKnockback * (GetFacingRight() ? -1 : 1);
		rb.velocity = new Vector2(xv, HurtVertKnockback);

		anim.SetTrigger("Hurt");

		yield return new WaitForSeconds(.5f);

		GetComponent<Health>().SetBlinking(true);

		yield return null;
	}

		#endregion

	public void EndAction()
	{
		Attacking = false;
		if (RoutineRequiresEndTrigger)
		{
			anim.SetTrigger("EndAttack");
			RoutineRequiresEndTrigger = false;
		}
		//FreeHitboxes();
		//STOP THE CURRENT ATTACK ENUMERATOR
	}

	private void PickupGem()
	{
		Gem g = Gem.FindClosestGem(transform.position);

		if(g != null && Vector2.Distance(transform.position, g.transform.position) < 1f)
		{
			CurGem = g.Collect();
		}
	}

	/*
	public void FreeHitboxes()
	{
		
		for(int i = 0; i < ReservedHitboxes.Count; i++)
		{
			ReservedHitboxes[i].Free();
		}

		ReservedHitboxes.Clear();

	}
	*/


	//**************************************************************
	//							OTHER
	//**************************************************************
	

	private static EnumDir GetDirection(float x, float y)
	{
		if (x == 0 && y == 1)
			return EnumDir.N;
		else if (x == 1 && y == 1)
			return EnumDir.NE;
		else if (x == 1 && y == 0)
			return EnumDir.E;
		else if (x == 1 && y == -1)
			return EnumDir.SE;
		else if (x == 0 && y == -1)
			return EnumDir.S;
		else if (x == -1 && y == -1)
			return EnumDir.SW;
		else if (x == -1 && y == 0)
			return EnumDir.W;
		else if (x == -1 && y == 1)
			return EnumDir.NW;
		else
			return EnumDir.CENTER;
	}

	private bool IsDirectionAdjacent( EnumDir a, EnumDir b )
	{
		if (a == EnumDir.CENTER || b == EnumDir.CENTER)
			return false;

		int diff = Mathf.Abs((int)(a) - (int)(b));
		if (diff == 0 || diff == 1 || diff == 7)
			return true;
		else
			return false;
	}

	public void SavePlayerState()
	{
		PlayerPrefs.SetInt("PHEALTH", GetComponent<Health>().GetHealth());
		PlayerPrefs.SetInt("PGEM", (int)CurGem);
	}

	public void LoadPlayerState()
	{
		int h = PlayerPrefs.GetInt("PHEALTH", 5);
		GetComponent<Health>().SetHealth(h);
		CurGem = (EnumGem)PlayerPrefs.GetInt("PGEM", 0);
	}



	//*****************************************************
	//			IDamageable Implemented Methods
	//*****************************************************

	public void OnHurt()
	{
		AudioSource hurtSound = SoundManager.Singleton.GenerateSound(transform.position, 1.5f);
		hurtSound.clip = SHurt;
		hurtSound.Play();


		StartAction("Knockback");
	}

	public void OnDeath()
	{
		//There has to be something better than just destroying the character. Death animation maybe?
		Destroy(gameObject);
	}

	//Other Variables

	protected override void SetGroundState(GroundState s)
	{
		base.SetGroundState(s);
		if(s == GroundState.WALKING)
		{
			forceFloating = false;
			FlyAttackCharged = true;
			DivekickCharged = true;
		}
	}

	void OnDestroy()
	{
		//FreeHitboxes();
	}
}
