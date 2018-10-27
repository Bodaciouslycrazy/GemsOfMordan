using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(IDamageable))]
public class Health : MonoBehaviour {

	[SerializeField]
	private int MaxHealth = 5;
	[SerializeField]
	private int CurHealth = 5;
	[SerializeField]
	private bool Invincible = false;
	[SerializeField]
	private float InvincibleTime = 2f;


	private float CurITime = 0f;

	void Start()
	{
		//entity = GetComponent<IDamageable>();
	}

	void FixedUpdate()
	{
		CurITime -= Time.fixedDeltaTime;

		if (CurITime <= 0 && Invincible)
			Invincible = false;
	}

	public void Hurt(int dam)
	{
		if (Invincible)
			return;

		CurHealth -= dam;

		if(CurHealth > 0)
		{
			TriggerOnHurt();
		}
		else
		{
			CurHealth = 0;
			TriggerOnDeath();
		}
	}

	private void TriggerOnHurt()
	{
		IDamageable entity = GetComponent<IDamageable>();
		if (entity != null)
			entity.OnHurt();
	}

	private void TriggerOnDeath()
	{
		IDamageable entity = GetComponent<IDamageable>();
		if (entity != null)
			entity.OnDeath();
	}

	/*
	public void SetInvincible(bool i)
	{
		Invincible = i;
	}
	*/

	public void SetInvincible()
	{
		Invincible = true;
		CurITime = InvincibleTime;
	}
}
