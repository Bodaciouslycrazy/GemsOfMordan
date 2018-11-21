using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour {

	public AudioClip[] FootstepClips;
	private int LastIndex = -1;

	// Use this for initialization
	void Start () {
		
	}
	
	public void PlayFootstep()
	{
		if (FootstepClips.Length == 0)
			return;

		int max = (LastIndex == -1) ? FootstepClips.Length : FootstepClips.Length - 1;
		int num = Random.Range(0, max);
		if (num == LastIndex) num++;

		AudioSource aus = SoundManager.Singleton.GenerateSound(transform.position);
		aus.clip = FootstepClips[num];
		aus.Play();
	}
}
