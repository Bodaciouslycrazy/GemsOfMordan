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

	public float LerpMult = 1f;
	public float DistPerVel = .5f;
	public float MaxCameraXDist = 2f;
	public float MaxHeightAbovePlatform = 5f;

	public enum State
	{
		MOVING_RIGHT,
		MOVING_LEFT,
		TRANSITION_RIGHT,
		TRANSITION_LEFT
	}

	public State curState = State.MOVING_RIGHT;

	private Vector3 oldPos;
	private Vector3 targetPos;
	private float timeAcc = 0f;
	private float platform = 0f;

	// Use this for initialization
	void Start () {
		oldPos = transform.position;
		targetPos = transform.position;
	}
	
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
}
