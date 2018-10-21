using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(IDamageable))]
public class Health : MonoBehaviour {

	[SerializeField]
	private int MaxHealth = 5;
	[SerializeField]
	private int CurHealth = 5;

	private IDamageable entity;

	void Start()
	{
		entity = GetComponent<IDamageable>();
	}

	public void Hurt(int dam)
	{
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
		if (entity != null)
			entity.OnHurt();
	}

	private void TriggerOnDeath()
	{
		if (entity != null)
			entity.OnDeath();
	}
}
