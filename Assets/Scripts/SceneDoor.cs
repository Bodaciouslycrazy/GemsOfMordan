using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : MonoBehaviour {

	//public static int LoadId = 0;
	//public static string LoadScene = "";

	public int Id = 1;
	public string DestinationScene;
	public int DestinationId;
	public bool RequireKeys = false;

	public bool Visible = true;

	private bool upLastFrame = true;

	// Use this for initialization
	void Start () {
		if (!Visible)
			GetComponent<SpriteRenderer>().enabled = false;

		if (RequireKeys && !HasKeys())
			GetComponent<Animator>().SetBool("Open", false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{
		bool upThisFrame = Input.GetAxisRaw("Vertical") == 1f;

		if(Visible && Vector3.Distance(PlayerController.MainPlayer.transform.position, transform.position) < 0.5f && upThisFrame && !upLastFrame && (!RequireKeys || HasKeys()))
		{
			PlayerController.MainPlayer.SavePlayerState();

			PlayerPrefs.SetInt("DID",DestinationId);
			PlayerPrefs.SetString("DSCENE",DestinationScene);

			SceneManager.LoadScene(DestinationScene);
		}

		upLastFrame = upThisFrame;
	}
	public bool HasKeys()
	{
		for(int i = 0; i < 3; i++)
		{
			if (PlayerPrefs.GetInt("Key" + i, 1) == 1)
				return false;
		}

		return true;
	}
}
