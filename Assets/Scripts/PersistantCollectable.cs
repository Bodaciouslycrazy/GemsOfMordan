using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PersistantCollectable : MonoBehaviour {
	[Header("Persistance Variables")]
	public string Prefix;
	public int Id;

	// Use this for initialization
	void Start () {
		if (!Exists())
			Destroy(gameObject);
		//PlayerPrefs.GetInt(Prefix + Id, 1);
	}
	
	public bool Exists()
	{
		return PlayerPrefs.GetInt(Prefix + Id, 1) == 1? true: false;
	}

	public void SetExists(bool e)
	{
		int n = e ? 1 : 0;
		PlayerPrefs.SetInt(Prefix + Id, n);
	}

	void FixedUpdate()
	{
		GameObject pl = CollideWithFriendlies();

		if (pl == PlayerController.MainPlayer.gameObject)
		{
			//Health h = pl.GetComponent<Health>();
			
			bool dest = OnCollect();
			if(dest) Destroy(gameObject);
			
		}
	}

	private GameObject CollideWithFriendlies()
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Friendlies"));
		Collider2D[] results = new Collider2D[1];
		
		int rezSize = Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, results);

		if (rezSize > 0)
			return results[0].gameObject;
		else return null;
	}

	public virtual bool OnCollect()
	{
		SetExists(false);
		return true;
	}
}
