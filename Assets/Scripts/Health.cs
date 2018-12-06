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

	const float BLINK_LENGTH = .08f;

	private bool Dead = false;
	private float CurBlinkTime = 0f;
	private bool Blinking = false;

	private float CurITime = 0f;

	private SpriteRenderer sr;

	void Start()
	{
		sr = GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		if(Blinking)
		{
			CurBlinkTime -= Time.deltaTime;
			if(CurBlinkTime <= 0)
			{
				Color c = sr.color;
				c.a = c.a == 1f? 0 : 1;
				sr.color = c;

				CurBlinkTime += BLINK_LENGTH;
			}
		}
	}

	void FixedUpdate()
	{
		CurITime -= Time.fixedDeltaTime;

		if (CurITime <= 0 && Invincible)
		{
			Invincible = false;
			SetBlinking(false);
		}
	}

	public int GetHealth()
	{
		return CurHealth;
	}

	public void SetHealth(int h)
	{
		CurHealth = h;
	}

	public int GetMaxHealth()
	{
		return MaxHealth;
	}

	public void Hurt(int dam)
	{
		if (Invincible || Dead)
			return;

		CurHealth -= dam;

		if(CurHealth > 0)
		{
			TriggerOnHurt();
		}
		else
		{
			CurHealth = 0;
			Dead = true;
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

	public void SetBlinking(bool b)
	{
		Blinking = b;

		if (b)
		{
			CurBlinkTime = BLINK_LENGTH;
		}
		else
		{
			Color c = sr.color;
			c.a = 1;
			sr.color = c;
		}
	}
}
