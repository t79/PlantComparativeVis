using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Graph
{
	public int         id        = 0;
	public List<Graph> neighbour = new List<Graph> ();
}
