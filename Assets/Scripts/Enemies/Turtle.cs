using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turtle : GEntity, IDamageable {
	//public GameObject DropOnDeath;

	public float WalkSpeed = 1f;
	public float StunLength = .5f;

	private Vector3Int lastCoors;

	public AIState CurAIState = AIState.WALKING;
	private float TimeInState = 0f;
	public enum AIState
	{
		WALKING,
		STUNNED
	}

	private void SetAIState(AIState s)
	{
		CurAIState = s;
		TimeInState = 0f;
	}

	public override void Start()
	{
		base.Start();
		lastCoors = GlobalPathfinder.Singleton.GetCoors(transform.position);
	}

	void FixedUpdate()
	{
		UpdateGroundState();
		TimeInState += Time.fixedDeltaTime;

		if (CurAIState == AIState.WALKING)
		{
			float xmod = GetFacingRight() ? -1 : 1;
			//Move!
			Vector2 dpos = new Vector2(xmod * WalkSpeed * Time.fixedDeltaTime, 0);
			rb.MovePosition((Vector2)transform.position + dpos);

			//Check if we need to turn around
			Vector3Int curCoors = GlobalPathfinder.Singleton.GetCoors(transform.position);
			if(curCoors.x != lastCoors.x)
			{
				//Here is where we check.
				Vector3Int wallCheck = curCoors + new Vector3Int((int)(xmod), 0, 0);
				Vector3Int floorCheck = curCoors + new Vector3Int((int)(xmod), -1, 0);

				if (GlobalPathfinder.Singleton.HasTile(wallCheck) || !GlobalPathfinder.Singleton.HasTile(floorCheck))
					SetFacingRight(!GetFacingRight());
			}
			lastCoors = curCoors;
		}
		else if (CurAIState == AIState.STUNNED)
		{
			//switch to idle after stun ends.
			if (TimeInState > StunLength)
				SetAIState(AIState.WALKING);
		}



		GroundFrameEnd();
		//If the player is colliding, hurt him.
		if (CurAIState != AIState.STUNNED)
			CollideWithFriendlies();
	}

	private void CollideWithFriendlies()
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Friendlies"));
		Collider2D[] results = new Collider2D[4];
		int resultSize = Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);

		for (int i = 0; i < 4; i++)
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
		//Instantiate(DropOnDeath, transform.position, Quaternion.identity);

		Destroy(gameObject);
	}
}
