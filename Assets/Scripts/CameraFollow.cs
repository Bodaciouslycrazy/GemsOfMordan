using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public enum FollowMode
	{
		FOLLOW_PLAYER,
		STATIC
	}

	public Transform Following;
	public FollowMode Mode = FollowMode.FOLLOW_PLAYER;

	public float SmoothTime = .3f;
	public float MaxSpeedMult = 2.5f;
	public float LookaheadPerVel = .35f;
	public float LookaheadMaxDist = 2.5f;
	public float VerticalOffset = 0.5f;

	private Vector2 CurVelocity;

	// Use this for initialization
	void Start () {
		//oldPos = transform.position;
		//targetPos = transform.position;
		CurVelocity = new Vector2();
	}

	private void Update()
	{
		if (Mode == FollowMode.STATIC)
		{
			//Do nothing
		}
		else if(Mode == FollowMode.FOLLOW_PLAYER)
		{
			if (Following == null)
				return;

			Vector3 pos = Vector2.SmoothDamp(
				transform.position, 
				GetTargetPos(), 
				ref CurVelocity, 
				SmoothTime, 
				Following.GetComponent<Rigidbody2D>().velocity.magnitude * MaxSpeedMult);
			pos.z = -10;

			transform.position = pos;
		}
	}

	private Vector2 GetTargetPos()
	{
		Vector2 targ = Following.position;
		Vector2 offset = Following.GetComponent<Rigidbody2D>().velocity * LookaheadPerVel;

		if(offset.magnitude > LookaheadMaxDist)
		{
			offset = offset.normalized * LookaheadMaxDist;
		}

		return targ + offset + new Vector2(0, VerticalOffset);
	}

	public void CenterCamera()
	{
		if (!enabled)
			return;

		Vector3 pos = GetTargetPos();
		pos.z = transform.position.z;

		transform.position = pos;
	}

	/*
	void Update()
	{
		timeAcc += Time.deltaTime;
		transform.position = Vector3.Lerp(oldPos, targetPos, timeAcc / Time.fixedDeltaTime);
	}

	void FixedUpdate () {
		if(Mode == FollowMode.FOLLOW_PLAYER)
		{
			//For left and right, use anchor points, and switch points.
			//For up and down, use platform locking with smoothing.
			oldPos = transform.position;
			timeAcc = 0f;


			float X = Following.transform.position.x + Mathf.Min(MaxCameraXDist, DistPerVel * Following.GetComponent<Rigidbody2D>().velocity.x);

			if (Following.GetComponent<PlayerController>().WasOnGround() || Following.position.y < platform)
			{
				//Debug.Log("Platform Updated");
				platform = Following.transform.position.y;
			}
			else if (Following.position.y > platform + MaxHeightAbovePlatform)
			{
				platform = Following.transform.position.y - MaxHeightAbovePlatform;
			}

			float Y = platform + 1f;
			float Z = transform.position.z;

			//semiTarget = new Vector3(X, Y, Z);
			targetPos = new Vector3(Mathf.Lerp(transform.position.x, X, LerpMult), Mathf.Lerp(transform.position.y, Y, LerpMult), Z);

			//transform.position = Vector3.Lerp( transform.position, target, Time.deltaTime * LerpMult);
		}
	}
	*/
}
