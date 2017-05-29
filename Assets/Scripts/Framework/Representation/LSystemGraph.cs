using UnityEngine;
using System.Collections;

public class LSystemGraph : Graph
{
	public Vector3    position    = new Vector3 ();
	public Quaternion orientation = new Quaternion ();
	public Vector3    size        = new Vector3 ();

	public string     symbol      = "";
	public float      angle       = 0;
	public float      length      = 0;
	public float      width       = 0;
	public Vector3    spaceAngle = new Vector3 ();
	public Vector3    spacePos   = new Vector3 ();

	public LSystemGraph parent    = null;

	public bool isBranchingNode   = false;

	public float shortestDensityDistance;

	public bool partOfShort = false;
}
