using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : ScriptableObject {

	public Tilemap tm;
	public int MaxFCost = 25;

	private NodePriorityQueue Queue;
	private List<PFNode> Closed;

	// Use this for initialization
	void Awake () {
		Debug.Log("Created Pathfinder!");
		Queue = CreateInstance<NodePriorityQueue>();
		Closed = new List<PFNode>();
	}

	public List<Vector2> FindPath(Vector2 from, Vector2 to)
	{
		Vector3Int f = tm.WorldToCell(from);
		Vector3Int t = tm.WorldToCell(to);
		return FindPath(
			new Vector2Int(f.x, f.y),
			new Vector2Int(t.x, t.y));
	}

	public List<Vector2> FindPath(Vector2Int from, Vector2Int to)
	{
		Debug.Log(string.Format("Finding path from ({0},{1}) to ({2},{3})", from.x, from.y, to.x, to.y));
		PFNode start = CreateInstance<PFNode>();
		start.pos = from;
		start.gcost = 0;
		start.hcost= Dist(start.pos, to);
		start.backpath = null;
		Queue.Enqueue(start);

		int loops = 0;

		while(true)
		{
			if(loops >= 500)
			{
				Debug.LogError("TOO MANY LOOPS! BREAKING.");
				return null;
			}

			PFNode cur = Queue.Dequeue();

			if (cur == null)
			{
				Debug.LogError("CAN'T FIND PATH");
				return null;
			}

			Closed.Add(cur);

			if(cur.pos == to)
			{
				Debug.Log("FOUND DESTINATION");
				break;
			}

			int newGcost = cur.gcost + 1;
			for(int i = 0; i < 4; i++)
			{
				Vector2Int neighborPos = DirToValue(i) + cur.pos;
				if (tm.HasTile(new Vector3Int(neighborPos.x, neighborPos.y, 0)) || CloseContains(neighborPos)) continue;

				PFNode neighbor = Queue.Take(neighborPos);

				if (neighbor == null)
				{
					neighbor = ScriptableObject.CreateInstance<PFNode>();
					neighbor.pos = neighborPos;
					neighbor.hcost = Dist(neighbor.pos, to);
				}
				else if (neighbor.gcost <= newGcost)
					continue;

				neighbor.gcost = newGcost;
				neighbor.backpath = cur.pos;
				if(neighbor.GetFcost() <= MaxFCost)
					Queue.Enqueue(neighbor);
			}
			loops++;
		}

		PFNode endNode = Closed[Closed.Count - 1];
		List<Vector2Int> BackPath = new List<Vector2Int>();

		while(endNode != null)
		{
			BackPath.Insert(0, endNode.pos);
			endNode = FindInClosed(endNode.backpath);
		}

		return FinalizePositions(BackPath);
	}

	private List<Vector2> FinalizePositions(List<Vector2Int> NodeList)
	{
		Vector2 offset = tm.cellSize / 2;
		List<Vector2> Positions = new List<Vector2>();
		for(int i = 0; i < NodeList.Count; i++)
		{
			Vector3Int cell = new Vector3Int(NodeList[i].x, NodeList[i].y, 0);
			Positions.Add(((Vector2)tm.CellToWorld(cell)) + offset);
		}

		return Positions;
	}
	
	private bool CloseContains(Vector2Int pos)
	{
		for(int i = 0; i < Closed.Count; i++)
		{
			if (Closed[i].pos == pos)
				return true;
		}

		return false;
	}

	private PFNode FindInClosed(Vector2Int? pos)
	{
		if (pos == null)
			return null;

		for (int i = 0; i < Closed.Count; i++)
		{
			if (Closed[i].pos == pos)
				return Closed[i];
		}

		return null;
	}

	private int Dist(Vector2Int a, Vector2Int b)
	{
		return (int)Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
	}

	private Vector2Int DirToValue(int i)
	{
		switch(i)
		{
			case 0:
				return new Vector2Int(1, 0);
			case 1:
				return new Vector2Int(0, 1);
			case 2:
				return new Vector2Int(-1, 0);
			case 3:
				return new Vector2Int(0, -1);
			default:
				throw new System.Exception("blerg!!!");
		}
	}

	public void Reset()
	{
		Queue = CreateInstance<NodePriorityQueue>();
		Closed = new List<PFNode>();
		//Queue.Clear();
		//Closed.Clear();
	}
}
