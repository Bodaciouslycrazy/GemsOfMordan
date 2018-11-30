using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EDoor : MonoBehaviour {

	private Animator anim;
	private Collider2D coll;

	void Start()
	{
		anim = GetComponent<Animator>();
		coll = GetComponent<Collider2D>();
	}

	public void OpenEDoor()
	{
		anim.SetTrigger("Open");
		coll.enabled = false;
		GetComponent<AudioSource>().Play();
	}
	
}
