using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraphNode
{
	public int             id;
	public string          symbol;
	public Vector3         position;
	public List<GraphNode> neighbours;

	public LSystemGraph node;

	public float distance;
	public int   prev;
	public bool  visited;

	public bool start  = false;
	public bool finish = false;

	public bool partOfShort;

	public int centerID;
}
