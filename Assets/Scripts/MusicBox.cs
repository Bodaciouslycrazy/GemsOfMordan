using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicBox : MonoBehaviour {
	public static MusicBox Singleton;

	//public AudioClip MainSong;

	private AudioSource src;

	// Use this for initialization
	void Start () {
		
		if(Singleton != null)
		{
			Destroy(gameObject);
			return;
		}
		else
		{
			Singleton = this;
			DontDestroyOnLoad(gameObject);
		}

		src = GetComponent<AudioSource>();
	}

	public void PlaySong()
	{
		if(!src.isPlaying)
			src.Play();
	}

	public void StopSong()
	{
		src.Stop();
	}

	
	public void RestartSong()
	{
		src.Stop();
		src.Play();
	}
	
	
}
