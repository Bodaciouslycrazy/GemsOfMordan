using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GEntity : MonoBehaviour {

	protected SpriteRenderer sr;
	protected Animator anim;
	protected Rigidbody2D rb;

	private bool facingRight = true;

	protected float GroundAngleTolerance = .01f;


	private bool groundThisFrame = true;
	private bool groundLastFrame = true;

	public virtual void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
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

	//Ground Calculations

	protected bool IsGrounded()
	{
		return groundThisFrame;
	}

	public bool WasGrounded()
	{
		return groundLastFrame;
	}

	protected bool UpdateGroundState()
	{
		if (groundThisFrame && !groundLastFrame)
		{
			anim.SetBool("OnGround", true);
			OnLand();
		}
		else if (!groundThisFrame && groundLastFrame)
		{
			anim.SetBool("OnGround", false);
			OnLift();
		}

		return groundThisFrame;
	}

	protected void GroundFrameEnd()
	{
		groundLastFrame = groundThisFrame;
		groundThisFrame = false;
	}

	void OnCollisionStay2D(Collision2D other)
	{
		foreach (ContactPoint2D contact in other.contacts)
		{
			if (Vector2.Dot(contact.normal, Vector2.up) > GroundAngleTolerance)
				groundThisFrame = true;
		}
	}

	protected virtual void OnLand()
	{
		
	}

	protected virtual void OnLift()
	{

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
