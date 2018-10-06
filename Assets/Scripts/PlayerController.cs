using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

	public float MaxSpeed = 5f;
	public float GroundAccel = 10f;
	public float AirAccel = 7f;
	public float GroundAngleTolerance = .01f;

	//public float JumpHeight = 2.5f;
	//public float JumpCounterForce = 5f;
	public float JumpHeight = 8f;
	public float JumpCancelAccel = 100f;

	//[SerializeField]
	//private GroundCheck feet;
	//private int NumGroundRaycasts = 5;


	private Rigidbody2D rb;
	private Animator anim;
	private SpriteRenderer sr;

	private bool facingRight = true;

	private float Horizontal = 0f;
	private bool pressedJump = false;
	private bool heldJump = false;
	private bool floating = false;
	//private float jumpTimeLeft = 0f;
	[HideInInspector]
	private bool onGround = true;
	private bool ignoreGround = false;

	private bool wasOnGroundLastFrame = true;

	//public Animator TEMPCONTROLLER;


	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		//UPDATE INPUTS
		if (Input.GetButtonDown("Jump"))
			pressedJump = true;
		heldJump = Input.GetButton("Jump");
		Horizontal = Input.GetAxisRaw("Horizontal");



		//Update all the animations!

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

		if (Input.GetButtonDown("Attack"))
		{
			anim.SetTrigger("Punch");
			//TEMPCONTROLLER.SetTrigger("MageSwipe");
			//Punch();
		}

		anim.SetBool("InAir", !onGround);
		if (!onGround)
			anim.SetBool("Falling", rb.velocity.y < 0);
	}

	private void FixedUpdate()
	{
		if (!onGround && ignoreGround) ignoreGround = false;
		//Debug.Log("onGround: " + onGround);

		float accel = 0;
		if(onGround)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("MagePunch"))
				Horizontal = 0;

			float targetXVel = Horizontal * MaxSpeed;
			float dv = targetXVel - rb.velocity.x;
			accel = dv / Time.fixedDeltaTime;

			if (accel > GroundAccel)
				accel = GroundAccel;
			else if (accel < -GroundAccel)
				accel = -GroundAccel;

		}
		else
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

			accel = dv / Time.fixedDeltaTime;
		}

		rb.AddForce(new Vector2(accel * rb.mass, 0f), ForceMode2D.Force);



		// Jumping
		if(onGround && pressedJump)
		{
			rb.AddForce(new Vector2(0, CalcJumpForce(JumpHeight, Mathf.Abs(Physics2D.gravity.y))) , ForceMode2D.Impulse);
			//jumpTimeLeft = JumpHoldTime;
			onGround = false;
			floating = true;
			ignoreGround = true;
			anim.SetTrigger("Jump");
		}
		else if( (!onGround || ignoreGround) && rb.velocity.y > 0 && (!heldJump || !floating))
		{
			floating = false;
			//Apply downward force
			float dv = JumpCancelAccel * Time.fixedDeltaTime;
			if (dv > rb.velocity.y)
				dv = rb.velocity.y;

			float cancelForce = (rb.mass * dv) / Time.fixedDeltaTime;
			rb.AddForce(Vector2.down * cancelForce);
		}
		/*
		else if ( (!onGround || ignoreGround ) && heldJump && floating && jumpTimeLeft > 0f)
		{
			float t = Mathf.Min(jumpTimeLeft, Time.fixedDeltaTime);
			jumpTimeLeft -= t;
			rb.AddForce(Vector2.up * t * JumpHoldForce * rb.mass);
			//Debug.Log("FLOATING - jtl = " + jumpTimeLeft);
		}
		else
		{
			floating = false;
		}
		*/

		wasOnGroundLastFrame = (onGround && !ignoreGround);
		pressedJump = false;
		onGround = false;
	}

	public void Punch()
	{
		/*
		if(!onGround)
		{
			rb.velocity = new Vector2(0, 1f);
		}
		*/

		float xoffset = 0.65f * (facingRight ? 1 : -1);
		Vector2 pos = (Vector2)transform.position + new Vector2( xoffset, -.1f );
		Hitbox hb = Hitbox.GetNextHitbox();

		hb.SetPos(pos, transform);
		hb.SetCollider(.75f, .4f, 0f, facingRight);
		hb.PlayAnimation("MageSwipe");

		Collider2D[] results = hb.GetHits();

		for(int i = 0; i < results.Length; i++)
		{
			if (results[i] == null)
				break;

			Debug.Log("Hit: " + results[i].gameObject.name);
		}
	}

	void OnCollisionStay2D(Collision2D other)
	{
		foreach( ContactPoint2D contact in other.contacts)
		{
			if (Vector2.Dot(contact.normal, Vector2.up) > GroundAngleTolerance)
				onGround = true;
		}
	}

	
	private float CalcJumpForce( float height, float grav)
	{
		return Mathf.Sqrt(2 * grav * height) * rb.mass;
	}
	
	public bool WasOnGround()
	{
		return wasOnGroundLastFrame;
	}

}
