using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GEntity : MonoBehaviour {

	protected SpriteRenderer sr;
	protected Animator anim;
	protected Rigidbody2D rb;

	private bool facingRight = true;

	protected float GroundAngleTolerance = .01f;

	private GroundState CurrentGroundState = GroundState.WALKING;

	public enum GroundState
	{
		WALKING,
		IN_AIR
	}

	protected bool groundThisFrame = true;
	protected bool groundLastFrame = true;

	public virtual void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
	}

	void OnCollisionStay2D(Collision2D other)
	{
		foreach (ContactPoint2D contact in other.contacts)
		{
			if (Vector2.Dot(contact.normal, Vector2.up) > GroundAngleTolerance)
				groundThisFrame = true;
		}
	}

	protected bool GetFacingRight()
	{
		return facingRight;
	}

	protected void SetFacingRight(bool s)
	{
		facingRight = s;
		GetComponent<SpriteRenderer>().flipX = !s;
	}

	protected virtual void SetGroundState(GroundState s)
	{
		CurrentGroundState = s;
		anim.SetBool("OnGround", (s == GroundState.WALKING));
	}

	protected GroundState GetGroundState()
	{
		return CurrentGroundState;
	}

	protected GroundState UpdateGroundState()
	{
		if (groundThisFrame && CurrentGroundState == GroundState.IN_AIR)
		{
			SetGroundState(GroundState.WALKING);
			//anim.SetBool("OnGround", true);
		}
		else if (!groundThisFrame && CurrentGroundState == GroundState.WALKING)
		{
			SetGroundState(GroundState.IN_AIR);
			//anim.SetBool("OnGround", false);
		}

		return GetGroundState();
	}

	public bool WasOnGround()
	{
		return groundLastFrame;
	}

	protected void GroundFrameEnd()
	{
		groundLastFrame = groundThisFrame;
		groundThisFrame = false;
	}

	protected float CalcJumpForce(float height, float grav)
	{
		return Mathf.Sqrt(2 * grav * height) * rb.mass;
	}

	protected float CalcJumpForce(float height)
	{
		return CalcJumpForce(height, Mathf.Abs(Physics2D.gravity.y));
	}
}
