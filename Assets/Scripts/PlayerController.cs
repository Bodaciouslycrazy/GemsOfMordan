using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable {

	[Header("Movement")]
	public float MaxSpeed = 5f;
	public float GroundAccel = 10f;
	public float AirAccel = 7f;
	public float GroundAngleTolerance = .01f;
	public float JumpHeight = 8f;
	public float JumpCancelAccel = 100f;

	[Header("BasicAttackVariables")]
	public float PunchVerticalVel = 1f;
	public float PunchMaxHorzVel = 1f;

	//************************
	//   PRIVATE VARIABLES
	//************************
	private Rigidbody2D rb;
	private Animator anim;
	private SpriteRenderer sr;

	private bool facingRight = true;

	private float Horizontal = 0f;
	private float Vertical = 0f;
	private int NumInputsStored = 0;
	private bool heldJump = false;
	private bool floating = false;
	private bool Attacking = false;

	//On ground stuff
	private bool groundThisFrame = true;
	private bool groundLastFrame = true;

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
		SPECIAL_ATTACK
	}

	enum EnumDir
	{
		CENTER,
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW
	}
	//Player State stuff
	private PState CurrentPState = PState.WALKING;
	enum PState
	{
		WALKING,
		IN_AIR
	}


	// Use this for initialization
	void Start ()
	{

		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		sr = GetComponent<SpriteRenderer>();
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
		

		
		if (Horizontal > 0 && !facingRight && rb.velocity.x > 0 && !Attacking)
		{
			facingRight = true;
			sr.flipX = false;
		}
		else if (Horizontal < 0 && facingRight && rb.velocity.x < 0 && !Attacking)
		{
			facingRight = false;
			sr.flipX = true;
		}

		if(Horizontal == 0f && rb.velocity.x == 0f) //player standing still
		{
			//set state to idle
			anim.SetInteger("Accel", 0);
			//Debug.Log("Accel = 0");
		}
		else if( rb.velocity.x * Horizontal > 0) //player is accelerating,
		{
			//set state to running
			anim.SetInteger("Accel", 1);
			//Debug.Log("Accel = 1");
		}
		else
		{
			//set state to sliding
			anim.SetInteger("Accel", -1);
			//Debug.Log("Accel = -1");
		}

		anim.SetBool("Falling", rb.velocity.y < 0);
	}


	//******************************************************************************************
	//									FIXED UPDATE
	//******************************************************************************************


	private void FixedUpdate()
	{

		//Complete the queued action, if possible.
		if(!Attacking && Time.time - QueuedActionTime < MaxQueueTime)
		{
			//Complete the queued action
			//If an action is completed, make sure to make the QueuedAction NONE so that the action doesn't happen again.
			
			if(QueuedAction == EnumAction.JUMP && CurrentPState != PState.IN_AIR)
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
				Punch();
				QueuedAction = EnumAction.NONE;
			}
			else if(QueuedAction == EnumAction.SPECIAL_ATTACK)
			{
				//Special Attack
				//Debug.Log("Special Attack");
				QueuedAction = EnumAction.NONE;
			}
		}

		//Do state transitions!
		if (groundThisFrame && CurrentPState == PState.IN_AIR)
		{
			CurrentPState = PState.WALKING;
			//anim.SetTrigger("Land");
			anim.SetBool("OnGround", true);
			//Debug.Log("SET LAND");
		}
		else if (!groundThisFrame && CurrentPState == PState.WALKING)
		{
			CurrentPState = PState.IN_AIR;
			//anim.SetTrigger("Jump");
			anim.SetBool("OnGround", false);
			//Debug.Log("SET JUMP");
		}


		//Now that we have completed the action queue, do movement!
		float accelX = 0;
		float accelY = 0;
		if (CurrentPState == PState.WALKING)
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
		else if(CurrentPState == PState.IN_AIR)
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
			if( rb.velocity.y > 0 && (!heldJump || !floating))
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
		groundLastFrame = groundThisFrame;
		groundThisFrame = false;

		//Update input average count
		NumInputsStored = 0;
	}


	//***************************************************************************
	//								ACTIONS
	//***************************************************************************

	private void Jump()
	{
		// Jumping
		if (CurrentPState != PState.IN_AIR)
		{
			rb.AddForce(new Vector2(0, CalcJumpForce(JumpHeight, Mathf.Abs(Physics2D.gravity.y))), ForceMode2D.Impulse);
			floating = true;

			//This anim trigger is not needed. Jump is triggered in FixedUpdate() when the floor is not detected.
			//This is same with switching to the IN_AIR PState
			//anim.SetTrigger("Jump");
			//CurrentPState = PState.IN_AIR;
		}
	}

	private void Punch()
	{
		//Air physics are weird.
		if(CurrentPState == PState.IN_AIR)
		{
			Vector2 newVel = rb.velocity;
			if(newVel.y < PunchVerticalVel) newVel.y = PunchVerticalVel;
			if (newVel.x > PunchMaxHorzVel) newVel.x = PunchMaxHorzVel;
			else if (newVel.x < -PunchMaxHorzVel) newVel.x = -PunchMaxHorzVel;
			rb.velocity = newVel;
		}

		//CurrentPState = PState.ATTACKING;
		Attacking = true;
		anim.SetTrigger("Punch");

		//Now, the hitbox!
		float xoffset = 0.7f * (facingRight ? 1 : -1);
		Vector2 pos = (Vector2)transform.position + new Vector2( xoffset, -.1f );
		Hitbox hb = Hitbox.GetNextHitbox();

		hb.SetPos(pos, transform);
		hb.SetCollider(.8f, .5f, 0, 16, facingRight);
		hb.PlayAnimation("MageSwipe");

		Collider2D[] results = hb.GetHits();

		for(int i = 0; i < results.Length; i++)
		{
			if (results[i] == null)
				break;

			Health enemyHealth = results[i].GetComponent<Health>();
			if (enemyHealth == null)
				continue;

			enemyHealth.Hurt(1);
		}
	}

	public void EndAttack()
	{
		Attacking = false;
		/*
		if (groundThisFrame)
			CurrentPState = PState.WALKING;
		else
			CurrentPState = PState.IN_AIR;
		*/
	}


	//**************************************************************
	//                 COLLISION AND OTHER PHYSICS
	//**************************************************************


	void OnCollisionStay2D(Collision2D other)
	{
		foreach( ContactPoint2D contact in other.contacts)
		{
			if (Vector2.Dot(contact.normal, Vector2.up) > GroundAngleTolerance)
				groundThisFrame = true;
		}
	}

	
	private float CalcJumpForce( float height, float grav)
	{
		return Mathf.Sqrt(2 * grav * height) * rb.mass;
	}
	
	
	public bool WasOnGround()
	{
		return groundLastFrame;
	}

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

	//*****************************************************
	//			IDamageable Implemented Methods
	//*****************************************************

	public void OnHurt()
	{

	}

	public void OnDeath()
	{
		//There has to be something better than just destroying the character. Death animation maybe?
		Destroy(gameObject);
	}
}
