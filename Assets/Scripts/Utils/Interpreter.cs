using UnityEngine;
using System.Collections;

public class Interpreter
{
	public Quaternion direction;
	public Vector3    position;
	public GameObject parent;
	
	public Interpreter (Interpreter other)
	{
		this.direction = other.direction;
		this.position  = other.position;
		this.parent    = other.parent;
	}
	
	public Interpreter (Quaternion direction, Vector3 position, GameObject parent = null)
	{
		this.direction = direction;
		this.position  = position;
		this.parent    = parent;
	}
}
