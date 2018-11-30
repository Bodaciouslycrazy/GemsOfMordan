using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public static SoundManager Singleton;

	public GameObject SoundPref;

	// Use this for initialization
	void Start () {
		Singleton = this;
	}

	public AudioSource GenerateSound(Vector3 pos, float time = 1f)
	{
		GameObject obj = Instantiate(SoundPref, pos, Quaternion.identity);
		Destroy(obj, time);
		return obj.GetComponent<AudioSource>();
	}

	public AudioSource GenerateSound(Vector3 pos, AudioClip clip, float vol = 1f, float time = 1f)
	{
		GameObject obj = Instantiate(SoundPref, pos, Quaternion.identity);
		Destroy(obj, time);

		AudioSource source = obj.GetComponent<AudioSource>();
		source.clip = clip;
		source.volume = vol;
		return source;
	}
}
