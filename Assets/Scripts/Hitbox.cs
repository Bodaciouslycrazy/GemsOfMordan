using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {

	static Hitbox[] HitboxPool = new Hitbox[32];
	static int initializedBoxes = 0;
	static int nextBox = 0;
	static Collider2D[] results = new Collider2D[16];
	static int resultSize = 0;

	[SerializeField]
	private EllipseCollider2D Col;

	private SpriteRenderer rend;
	private Animator anim;

	// Use this for initialization
	void Start () {
		if(initializedBoxes >= HitboxPool.Length)
		{
			Destroy(gameObject);
		}
		else
		{
			HitboxPool[initializedBoxes] = this;
			initializedBoxes++;
			Debug.Log("Initialized a box!");
		}

		rend = GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();
	}

	public void PlayAnimation(string animName)
	{
		anim.SetTrigger(animName);
	}

	public void SetPos( Vector2 pos, Transform parent = null)
	{
		transform.SetParent(parent);
		transform.position = pos;
	}

	public void SetCollider(float width, float height, float angle, bool facingRight)
	{
		Col.radiusX = width;
		Col.radiusY = height;
		Col.rotation = angle;

		rend.flipX = !facingRight;
	}

	public Collider2D[] GetHits()
	{
		return GetHits(new ContactFilter2D());
	}

	public Collider2D[] GetHits( ContactFilter2D filter )
	{
		resultSize = Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);
		for(int i = resultSize; i < results.Length; i++)
		{
			results[i] = null;
		}
		return results;
	}

	public static Hitbox GetNextHitbox()
	{
		//Debug.Log("InitializedHitboxes: " + initializedBoxes);
		if (initializedBoxes <= 0)
			return null;



		Hitbox ret = HitboxPool[nextBox];
		//Debug.Log("Returning box number " + nextBox);
		nextBox = (nextBox + 1) % initializedBoxes;
		return ret;
	}
}
