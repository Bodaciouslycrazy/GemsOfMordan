using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {

	void OnCollisionStay2D(Collision2D collision)
	{
		Health h = collision.gameObject.GetComponent<Health>();

		if (h != null)
			h.Hurt(1);
	}
}
