using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : PersistantCollectable {


	public override bool OnCollect()
	{
		Health h = PlayerController.MainPlayer.GetComponent<Health>();

		if(h.GetHealth() < h.GetMaxHealth())
		{
			h.SetHealth(h.GetMaxHealth());
			SetExists(false);

			return true;
		}

		return false;
	}

}
