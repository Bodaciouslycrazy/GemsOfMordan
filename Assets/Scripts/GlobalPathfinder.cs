using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class GlobalPathfinder : MonoBehaviour {

	public static GlobalPathfinder Singleton;


	private Tilemap TM;
	private Pathfinder pf;

	// Use this for initialization
	void Start () {
		Singleton = this;

		TM = GetComponent<Tilemap>();

		pf = ScriptableObject.CreateInstance<Pathfinder>();
		pf.tm = TM;
	}
	
	public List<Vector2> FindPath(Vector2 from, Vector2 to)
	{
		pf.Reset();
		return pf.FindPath(from, to);
	}
}
