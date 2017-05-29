using UnityEngine;
using System.Collections;

public abstract class Abstraction
{
	public string  name;
	public Vector3 dimension;
	public Vector3 objectSize;
	public Vector3 objectOffset;

	public float scale;

	public virtual Representation Process (Representation representation)
	{
		return representation;
	}

	public float DimensionWeight(Vector3 dimDesc)
	{
		bool good = true;
		good &= (dimension.x == dimDesc.x) ? true : false;
		good &= (dimension.y == dimDesc.y) ? true : false;
		//good &= (dimension.z == dimDesc.z) ? true : false;

		return good ? 1f : 0; 
	}
}
