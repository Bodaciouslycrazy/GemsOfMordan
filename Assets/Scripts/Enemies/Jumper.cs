using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumper : GEntity, IDamageable {

	public GameObject DropOnDeath;

	public float AggroDist = 10f;
	public float JumpPrepareTime = 1f;
	public float JumpCooldownTime = 1.5f;
	public float StunLength = .5f;

	public float JumpHeight = 3f;
	public float JumpHorzForce = 10f;

	public AudioClip SJump;
	public AudioClip SLand;

	public AIState CurAIState = AIState.IDLE;
	private float TimeInState = 0f;
	public enum AIState
	{
		IDLE,
		PREPARING_JUMP,
		JUMPED,
		STUNNED
	}

	private void SetAIState(AIState s)
	{
		CurAIState = s;
		TimeInState = 0f;
	}

	void FixedUpdate()
	{
		UpdateGroundState();
		TimeInState += Time.fixedDeltaTime;

		if (CurAIState == AIState.IDLE)
		{
			//if player is in range, turn to him and prepare jump.
			if( Vector2.Distance( transform.position, PlayerController.MainPlayer.transform.position) < AggroDist )
			{
				if( (transform.position.x < PlayerController.MainPlayer.transform.position.x) != GetFacingRight() )
				{
					SetFacingRight(!GetFacingRight());
				}

				anim.SetTrigger("Prepare");
				SetAIState(AIState.PREPARING_JUMP);
			}
		}
		else if(CurAIState == AIState.PREPARING_JUMP)
		{
			//wait, then jump
			if(TimeInState >= JumpPrepareTime && GetGroundState() == GroundState.WALKING)
			{
				//Jump
				Vector2 j = new Vector2();
				j.x = JumpHorzForce * (GetFacingRight() ? 1 : -1);
				j.y = CalcJumpForce(JumpHeight);
				rb.AddForce(j, ForceMode2D.Impulse);

				SetAIState(AIState.JUMPED);

				AudioSource jsound = SoundManager.Singleton.GenerateSound(transform.position);
				jsound.clip = SJump;
				jsound.volume = 0.75f;
				jsound.Play();
			}
		}
		else if(CurAIState == AIState.JUMPED)
		{
			//wait till you hit the ground, then go to idle.
			if( TimeInState > JumpCooldownTime && GetGroundState() == GroundState.WALKING)
			{
				SetAIState(AIState.IDLE);
			}
		}
		else if(CurAIState == AIState.STUNNED)
		{
			//switch to idle after stun ends.
			if (TimeInState > StunLength)
				SetAIState(AIState.IDLE);
		}



		GroundFrameEnd();
		//If the player is colliding, hurt him.
		if(CurAIState != AIState.STUNNED)
			CollideWithFriendlies();
	}

	private void CollideWithFriendlies()
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Friendlies"));
		Collider2D[] results = new Collider2D[4];
		int resultSize = Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);

		for(int i = 0; i < 4; i++)
		{
			if (results[i] == null)
				break;

			Health h = results[i].GetComponent<Health>();

			if (h != null)
				h.Hurt(1);
		}
	}

	//*****************************************************
	//			IDamageable Implemented Methods
	//*****************************************************

	public void OnHurt()
	{
		SetAIState(AIState.STUNNED);
		rb.velocity = new Vector2(0, 4f);
		anim.SetTrigger("Hurt");
	}

	public void OnDeath()
	{
		Instantiate(DropOnDeath, transform.position, Quaternion.identity);

		Destroy(gameObject);
	}
}
