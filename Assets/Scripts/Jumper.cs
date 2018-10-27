using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumper : MonoBehaviour, IDamageable {


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{


		//If the player is colliding, hurt him.
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
		//StartAction("Hurt");
	}

	public void OnDeath()
	{
		Destroy(gameObject);
	}
}
