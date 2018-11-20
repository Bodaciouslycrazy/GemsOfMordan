using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zapper : GEntity, IDamageable {

	public GameObject DropOnDeath;
	public GameObject ProjectilePref;

	public float AggroDist = 10f;
	public float MoveSpeed = 2.5f;
	public float TargetDist = 1f;
	public float FireDelay = 1f;
	public float FireCooldown = 1f;
	public float ProjectileSpeed = 5;

	public float StunLength = .5f;

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
			if (CurPath.Count > 1 && Vector2.Distance(transform.position, CurPath[0]) < 0.3)
				CurPath.RemoveAt(0);

			if ((CurPath[0].x > transform.position.x) != GetFacingRight())
				SetFacingRight(!GetFacingRight());

			rb.MovePosition(Vector3.MoveTowards(transform.position, CurPath[0], Time.fixedDeltaTime * MoveSpeed));

			if(IsInLOS())
			{
				SetAIState(AIState.ALIGNING);
			}
			else if(TimeInState >= 1.2f)
			{
				GetPath();
				SetAIState(AIState.PATHING);
			}
		}
		else if(CurAIState == AIState.ALIGNING)
		{
			Vector2 TargPos = PlayerController.MainPlayer.transform.position;
			//Face toward player
			if ((TargPos.x > transform.position.x) != GetFacingRight())
				SetFacingRight(!GetFacingRight());

			if (transform.position.x < TargPos.x) TargPos -= new Vector2(TargetDist, 0);
			else TargPos += new Vector2(TargetDist, 0);
			Vector2 diff = ((Vector2)transform.position) - TargPos;

			//Do y var
			float dy = 0;
			if (diff.y > 0.05 || diff.y < -0.05)
				dy = diff.y > 0 ? -1 : 1;

			//Do x var
			float dx = 0;
			if (diff.x > 0.05 || diff.x < -0.05)
				dx = diff.x > 0 ? -1 : 1;


			//Combine, and find new position
			Vector2 move = new Vector2(dx, dy).normalized * Time.fixedDeltaTime * MoveSpeed;
			rb.MovePosition(((Vector2)transform.position) + move);

			if(!IsInLOS())
			{
				GetPath();
				SetAIState(AIState.PATHING);
			}
			else if(dy == 0 && diff.magnitude <= 5)
			{
				//State transition to charging!
				SetAIState(AIState.CHARGING);
			}
		}
		else if(CurAIState == AIState.CHARGING)
		{
			if(TimeInState >= FireDelay)
			{
				//Fire the shot
				//Debug.Log("PEW PEW PEW!");
				Fire();
				SetAIState(AIState.COOLDOWN);
			}
		}
		else if(CurAIState == AIState.COOLDOWN)
		{
			if (TimeInState >= FireCooldown)
				SetAIState(AIState.IDLE);
		}
		else if(CurAIState == AIState.STUNNED)
		{
			if (TimeInState >= StunLength)
				SetAIState(AIState.IDLE);
		}
	}

	public void Fire()
	{
		float xmod = GetFacingRight() ? 1 : -1;

		Vector2 pos = (Vector2)transform.position + new Vector2(0.5f * xmod, 0);
		Vector2 vel = new Vector2(xmod * ProjectileSpeed, 0);

		GameObject proj = Instantiate(ProjectilePref, pos, Quaternion.identity);
		proj.GetComponent<Rigidbody2D>().velocity = vel;
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
		rb.velocity = new Vector2(0, 0f);
		anim.SetTrigger("Hurt");
	}

	public void OnDeath()
	{
		Instantiate(DropOnDeath, transform.position, Quaternion.identity);

		Destroy(gameObject);
	}
}
