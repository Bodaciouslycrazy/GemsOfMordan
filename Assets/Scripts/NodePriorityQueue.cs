using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePriorityQueue : ScriptableObject {

	private List<PFNode> List;

	void Awake()
	{
		Debug.Log("Created NodeQueue!");
		List = new List<PFNode>();
	}

	public void Enqueue(PFNode node)
	{
		int ind = InsertSearch(0, List.Count, node.GetFcost());

		List.Insert(ind, node);
	}

	private int InsertSearch(int start, int end, int fcost)
	{
		if (start == end)
			return start;

		int mid = (start + end) / 2;
		int comp = PFNode.Compare(fcost, List[mid]);

		if (comp <= 0)
			return InsertSearch(start, mid, fcost);
		else
			return InsertSearch(mid + 1, end, fcost);
	}

	public PFNode Dequeue()
	{
		if (List.Count == 0)
			return null;

		PFNode top = List[0];
		List.RemoveAt(0);
		return top;
	}

	public PFNode Peek()
	{
		if (List.Count == 0)
			return null;

		return List[0];
	}

	public PFNode Take(Vector2Int pos)
	{
		for(int i = 0; i < List.Count; i++)
		{
			if (List[i].pos == pos)
			{
				PFNode ret = List[i];
				List.RemoveAt(i);
				return ret;
			}
		}

		return null;
	}

	public void Clear()
	{
		List.Clear();
	}
}
