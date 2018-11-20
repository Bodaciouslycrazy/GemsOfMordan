using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour {

	public float TimeTillDisappear = 8f;
	public float BlinkTime = 2f;
	public float BLINK_LENGTH = 0.08f;
	public PlayerController.EnumGem Type;


	private float timeAlive = 0;
	private float curBlinkTime = 0;
	private bool Blinking = false;
	private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer>();
		Vector2 vel = new Vector2(Random.Range(-3f, 3f), 5f);
		GetComponent<Rigidbody2D>().velocity = vel;
	}
	
	// Update is called once per frame
	void Update () {
		if(Blinking)
		{
			curBlinkTime -= Time.deltaTime;
			if(curBlinkTime <= 0)
			{
				Color c = sr.color;
				c.a = c.a == 1f ? 0 : 1;
				sr.color = c;

				curBlinkTime += BLINK_LENGTH;
			}
		}
	}

	void FixedUpdate()
	{
		timeAlive += Time.fixedDeltaTime;
		if (timeAlive >= TimeTillDisappear)
			Destroy(gameObject);
		else if (timeAlive >= TimeTillDisappear - BlinkTime)
			Blinking = true;
	}

	public PlayerController.EnumGem Collect()
	{
		Destroy(gameObject);
		return Type;
	}

	public static Gem FindClosestGem(Vector2 pos)
	{
		Gem[] gems = FindObjectsOfType<Gem>();

		if (gems.Length == 0)
			return null;

		float minLength = 99999999f;
		int minIndex = 0;

		for(int i = 0; i < gems.Length; i++)
		{
			float dist = Vector2.Distance(pos, gems[i].transform.position);

			if(dist < minLength)
			{
				minLength = dist;
				minIndex = i;
			}
		}

		return gems[minIndex];
	}
}
