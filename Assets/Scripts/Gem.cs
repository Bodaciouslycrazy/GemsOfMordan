using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour {

	public float TimeTillDisappear = 8f;
	public PlayerController.EnumGem Type;


	private float timeAlive = 0;

	// Use this for initialization
	void Start () {
		Vector2 vel = new Vector2(Random.Range(-1f, 1f), 2f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{
		timeAlive += Time.fixedDeltaTime;
		if (timeAlive >= TimeTillDisappear)
			Destroy(gameObject);
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
