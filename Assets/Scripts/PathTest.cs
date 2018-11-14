using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTest : MonoBehaviour {

	public GameObject NodeIndicater;

	private List<GameObject> CreatedIndicaters;

	// Use this for initialization
	void Start () {
		CreatedIndicaters = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.F))
		{
			Clear();
			List<Vector2> points = GlobalPathfinder.Singleton.FindPath(transform.position, PlayerController.MainPlayer.transform.position);
			Debug.Log("Returned path with " + points.Count + "points.");
			CreatePoints(points);
		}

		/*
		if(Input.GetKeyDown(KeyCode.C))
		{
			Vector2Int a = new Vector2Int(3, 3);
			Vector2Int b = new Vector2Int(3, 3);

			Debug.Log(" a == b is " + (a == b));
		}
		*/
	}

	private void CreatePoints( List<Vector2> List)
	{
		Vector2 offset = new Vector2(0.5f, 0.5f);
		for(int i = 0; i < List.Count; i++)
		{
			GameObject indicator = Instantiate(NodeIndicater, List[i] + offset, Quaternion.identity);
			CreatedIndicaters.Add(indicator);
		}
	}

	private void Clear()
	{
		for(int i = 0; i < CreatedIndicaters.Count; i++)
		{
			Destroy(CreatedIndicaters[i]);
		}
		CreatedIndicaters.Clear();
	}
}
