using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Representation
{
	public string  type      = "abstract";
	public int     dimension = 0;
	public Vector3 size      = new Vector3 ();
	public Vector3 offset    = new Vector3 ();
	public Vector3 objectMax = new Vector3 ();
	public Vector3 objectMin = new Vector3 ();

	public List<Vector3> additionalInformation = new List<Vector3> ();
}
