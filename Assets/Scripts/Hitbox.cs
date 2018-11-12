using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Hitbox : MonoBehaviour {

	static Hitbox[] HitboxPool = new Hitbox[32];
	static int initializedBoxes = 0;
	static int nextBox = 0;
	static int resultSize = 0;

	private PolygonCollider2D coll;
	private SpriteRenderer rend;
	private Animator anim;
	private bool Reserved = false;

	// Use this for initialization
	void Start () {
		/*
		if(initializedBoxes >= HitboxPool.Length)
		{
			Destroy(gameObject);
		}
		else
		{
			HitboxPool[initializedBoxes] = this;
			initializedBoxes++;
			//Debug.Log("Initialized a box!");
		}
		*/

		rend = GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();
		coll = GetComponent<PolygonCollider2D>();
	}


	public void PlayAnimation(string animName)
	{
		anim.SetTrigger(animName);
	}

	public void StopAnimation()
	{
		anim.SetTrigger("Stop");
	}

	public void SetPos( Vector2 pos, Transform parent = null)
	{
		SetPos(pos, new Vector2(1, 1), parent);
	}

	public void SetPos( Vector2 pos, Vector2 scale, Transform parent = null)
	{
		transform.parent = null;
		transform.localScale = new Vector3(scale.x, scale.y, 1);
		transform.SetParent(parent);

		if (transform.parent != null)
			transform.localPosition = pos;
		else
			transform.position = pos;
	}

	public void SetCollider(float width, float height, float angle, int points, bool facingRight)
	{
		
		Vector2[] newPoints = new Vector2[points];

		float o = angle * Mathf.Deg2Rad * ((facingRight) ? 1 : -1);
		float radiusX = width / 2;
		float radiusY = height / 2;

		for(int i = 0; i < points; i++)
		{
			float a = i * (2 * Mathf.PI / points);

			float r = (radiusX * radiusY) / Mathf.Sqrt( Mathf.Pow(radiusY*Mathf.Cos(a),2) + Mathf.Pow(radiusX*Mathf.Sin(a),2) );

			newPoints[i] = new Vector2( Mathf.Cos(a + o) * r, Mathf.Sin(a + o) * r) ;
		}

		//newPoints[points] = newPoints[0];
		
		coll.points = newPoints;
		rend.flipX = !facingRight;
	}

	public Collider2D[] GetHits(string layerName = "Enemies")
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask(layerName));
		return GetHits( filter );
	}

	public Collider2D[] GetHits( ContactFilter2D filter )
	{
		Collider2D[] results = new Collider2D[16];
		resultSize = Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);
		//Debug.Log("Hitbox hit " + resultSize + " entities.");
		for(int i = resultSize; i < results.Length; i++)
		{
			results[i] = null;
		}
		return results;
	}

	public void Reserve()
	{
		Reserved = true;
	}

	public void Free()
	{
		Reserved = false;
	}

	public static Hitbox GetNextHitbox()
	{
		//Debug.Log("InitializedHitboxes: " + initializedBoxes);
		if (initializedBoxes <= 0)
			return null;

		int check = 0;

		while(HitboxPool[nextBox].Reserved && check < initializedBoxes)
		{
			nextBox = (nextBox + 1) % initializedBoxes;
			check++;
		}

		if(check == initializedBoxes) //All boxes are reserved
		{
			Debug.LogError("All hitboxes are reserved! Screaaaam!");
			return null;
		}
		else
		{
			Hitbox ret = HitboxPool[nextBox];
			nextBox = (nextBox + 1) % initializedBoxes;
			return ret;
		}
	}
}
