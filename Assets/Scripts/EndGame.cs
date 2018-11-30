using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour {

	void Start()
	{
		MusicBox.Singleton.StopSong();
	}

	public void CloseApplication()
	{
		Application.Quit();
	}
}
