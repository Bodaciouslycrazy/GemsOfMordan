using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFNode : ScriptableObject {

	public Vector2Int pos;
	public Vector2Int? backpath;
	public int gcost;
	public int hcost;
	
	public int GetFcost()
	{
		return gcost + hcost;
	}

	public static int Compare(PFNode a, PFNode b)
	{
		return a.GetFcost() - b.GetFcost();
	}

	public static int Compare(PFNode a, int b)
	{
		return a.GetFcost() - b;
	}

	public static int Compare(int a, PFNode b)
	{
		return a - b.GetFcost();
	}
}
