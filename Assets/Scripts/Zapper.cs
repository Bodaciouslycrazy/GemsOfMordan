using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zapper : GEntity, IDamageable {

	public float AggroDist = 10f;
	public float MoveSpeed = 2.5f;
	public float TargetDist = 1f;
	public float FireDelay = 1f;

	private List<Vector2> CurPath;

	public AIState CurAIState = AIState.IDLE;
	private float TimeInState = 0f;
	public enum AIState
	{
		IDLE,
		PATHING,
		ALIGNING,
		CHARGING,
		COOLDOWN,
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

		if(CurAIState == AIState.IDLE)
		{
			if(Vector2.Distance(transform.position, PlayerController.MainPlayer.transform.position) <= AggroDist)
			{
				if (IsInLOS())
					SetAIState(AIState.ALIGNING);
				else
				{
					GetPath();
					SetAIState(AIState.PATHING);
				}
			}
		}
		else if(CurAIState == AIState.PATHING)
		{
			if (CurPath.Count > 1 && Vector2.Distance(transform.position, CurPath[0]) < 0.05)
				CurPath.RemoveAt(0);

			rb.MovePosition(Vector3.MoveTowards(transform.position, CurPath[0], Time.fixedDeltaTime * MoveSpeed));

			if(IsInLOS())
			{
				SetAIState(AIState.ALIGNING);
			}
			else if(TimeInState >= 3)
			{
				GetPath();
				SetAIState(AIState.PATHING);
			}
		}
		else if(CurAIState == AIState.ALIGNING)
		{
			Vector2 diff = transform.position - PlayerController.MainPlayer.transform.position;

			//Do y var
			float dy = 0;
			if (diff.y > 0.05 || diff.y < -0.05)
				dy = diff.y > 0 ? -1 : 1;

			//Do x var
			float dx = 0;

			//Combine, and find new position
			Vector2 move = new Vector2(dx, dy).normalized * Time.fixedDeltaTime * MoveSpeed;
			rb.MovePosition(((Vector2)transform.position) + move);

			if(!IsInLOS())
			{
				GetPath();
				SetAIState(AIState.PATHING);
			}
			else if(dy == 0)
			{
				//State transition to charging!
			}
		}
	}

	private bool IsInLOS()
	{
		//make raycast from me to player.
		//return true if it doesn't collide with any terrain.
		Vector2 dir = PlayerController.MainPlayer.transform.position - transform.position;
		float dist = Vector2.Distance(transform.position, PlayerController.MainPlayer.transform.position);

		RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, LayerMask.GetMask("Terrain"));
		return hit.collider == null;
	}

	private void GetPath()
	{
		CurPath = GlobalPathfinder.Singleton.FindPath(transform.position, PlayerController.MainPlayer.transform.position);
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
		Destroy(gameObject);
	}
}
