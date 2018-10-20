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
	public float PunchVerticalVel = 1f;

	//************************
	//   PRIVATE VARIABLES
	//************************
	private Rigidbody2D rb;
	private Animator anim;
	private SpriteRenderer sr;

	private bool facingRight = true;

	private float Horizontal = 0f;
	private float Vertical = 0f;
	//private bool pressedJump = false;
	private bool heldJump = false;
	private bool floating = false;

	//On ground stuff
	private bool groundThisFrame = true;
	private bool groundLastFrame = true;

	//Action queue stuff
	[Header("ActionQueue")]
	public float MaxQueueTime = .3f;
	private EnumAction QueuedAction = EnumAction.NONE;
	private float QueuedActionTime = 0f;
	enum EnumAction
	{
		NONE,
		JUMP,
		BASIC_ATTACK,
		SPECIAL_ATTACK
	}

	//Player State stuff
	private PState CurrentPState = PState.WALKING;
	enum PState
	{
		WALKING,
		IN_AIR,
		ATTACKING,
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
		if (Input.GetButtonDown("Jump"))
		{
			QueuedAction = EnumAction.JUMP;
			QueuedActionTime = Time.time;
		}
		else if (Input.GetButtonDown("Attack"))
		{
			QueuedAction = EnumAction.BASIC_ATTACK;
			QueuedActionTime = Time.time;
		}
		heldJump = Input.GetButton("Jump");
		Horizontal = Input.GetAxisRaw("Horizontal");
		Vertical = Input.GetAxisRaw("Vertical");



		//Update all the animations!
		/*
		if (Horizontal > 0 && !facingRight && rb.velocity.x > 0)
		{
			facingRight = true;
			sr.flipX = false;
		}
		else if (Horizontal < 0 && facingRight && rb.velocity.x < 0)
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

		//Should probably not have this in Update() ?
		if (Input.GetButtonDown("Attack"))
		{
			anim.SetTrigger("Punch");
			//TEMPCONTROLLER.SetTrigger("MageSwipe");
			//Punch();
		}
		else if(Input.GetButtonDown("Special"))
		{
			anim.SetTrigger("Fly");
			rb.velocity = new Vector2(0, 15);
			floating = true;
		}
		

		anim.SetBool("InAir", !onGround);
		if (!onGround)
			anim.SetBool("Falling", rb.velocity.y < 0);
		*/
	}

	private void FixedUpdate()
	{

		//Go through the action queue!
		if(CurrentPState != PState.ATTACKING && Time.time - QueuedActionTime < MaxQueueTime)
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
			}
		}

		//Do state transitions!
		if (groundThisFrame && CurrentPState == PState.IN_AIR)
			CurrentPState = PState.WALKING;
		else if (!groundThisFrame && CurrentPState == PState.WALKING)
			CurrentPState = PState.IN_AIR;


		//Now that we have completed the action queue, do movement!
		float accelX = 0;
		float accelY = 0;
		if(CurrentPState == PState.WALKING)
		{

			float targetXVel = Horizontal * MaxSpeed;
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
			//jumpTimeLeft = JumpHoldTime;
			//onGround = false;
			floating = true;
			//ignoreGround = true;
			anim.SetTrigger("Jump");
			CurrentPState = PState.IN_AIR;
		}
	}

	private void Punch()
	{
		if(CurrentPState == PState.IN_AIR)
		{
			Vector2 vel = rb.velocity;
			vel.y = PunchVerticalVel;
			rb.velocity = vel;
		}
		

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
