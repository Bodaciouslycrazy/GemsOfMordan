using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour {

	private Health displaying;
	private Image HealthBar;

	// Use this for initialization
	void Start () {
		HealthBar = GetComponent<Image>();
		displaying = PlayerController.MainPlayer.GetComponent<Health>();
	}
	
	// Update is called once per frame
	void Update () {
		float amt = (float)displaying.GetHealth() / (float)displaying.GetMaxHealth();

		HealthBar.fillAmount = amt;
	}
}
