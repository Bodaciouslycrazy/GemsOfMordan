using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDoorManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SceneDoor[] doors = FindObjectsOfType<SceneDoor>();

		int did = PlayerPrefs.GetInt("DID");

		for(int i = 0; i < doors.Length; i++)
		{
			if(doors[i].Id == did)
			{
				PlayerController.MainPlayer.transform.position = doors[i].transform.position;
				Camera.main.GetComponent<CameraFollow>().CenterCamera();
				return;
			}
		}

		Debug.LogError("CAN'T FIND DOOR WITH ID: " + did);
	}
}
