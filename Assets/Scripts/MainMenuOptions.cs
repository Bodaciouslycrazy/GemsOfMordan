using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuOptions : MonoBehaviour {

	public void StartGame()
	{
		string sname = PlayerPrefs.GetString("DSCENE");
		SceneManager.LoadScene(sname);
		MusicBox.Singleton.PlaySong();
	}

	public void ResetGame()
	{
		PlayerPrefs.DeleteAll();
		SceneManager.LoadScene("Start");
		MusicBox.Singleton.PlaySong();
	}
}
