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

	//private IDamageable entity;

	void Start()
	{
		//entity = GetComponent<IDamageable>();
	}

	public void Hurt(int dam)
	{
		if (Invincible)
			return;

		CurHealth -= dam;

		if(CurHealth <= 0)
		{
			CurHealth = 0;
			TriggerOnHurt();
		}
		else
		{
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

	public void SetInvincible(bool i)
	{
		Invincible = i;
	}
}
