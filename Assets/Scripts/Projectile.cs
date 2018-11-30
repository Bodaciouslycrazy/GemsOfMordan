using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public int Damage = 1;
	public string AttackLayer = "Default";
	public bool OpenEDoors = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{
		CollideWithLayer();
	}

	private void CollideWithLayer()
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask(AttackLayer, "Terrain"));
		Collider2D[] results = new Collider2D[1];
		int resultSize = Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);


		if (results[0] == null)
			return;

		Health h = results[0].GetComponent<Health>();
		EDoor d = results[0].GetComponent<EDoor>();

		if (h != null)
		{
			h.Hurt(Damage);
		}
		if(d != null && OpenEDoors)
		{
			d.OpenEDoor();
		}

		Destroy(gameObject);
		return;
		
	}
}
