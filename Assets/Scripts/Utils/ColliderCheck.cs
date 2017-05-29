using UnityEngine;
using System.Collections;

public class ColliderCheck : MonoBehaviour
{
	public float sphereRadius = 0.1f;
	
	// Update is called once per frame
	void Update ()
	{
		if (Physics.CheckSphere (transform.position, sphereRadius))
						print ("Hit at position " + transform.position);
	}
}
