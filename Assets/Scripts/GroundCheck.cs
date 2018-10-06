using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

	private int inside = 0;


	void OnTriggerEnter2D(Collider2D other)
	{
		inside++;
	}

	void OnTriggerExit2D(Collider2D other)
	{
		inside--;
	}

	public bool IsOnGround()
	{
		return inside > 0;
	}
}
