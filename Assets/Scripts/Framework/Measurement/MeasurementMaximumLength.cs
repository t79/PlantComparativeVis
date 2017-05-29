using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementMaximumLength : Measurement
{

	private List<float> distances = new List<float> ();
	
	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;
		
		if (repre.structure == null)
			return 0;
		
		distances.Clear ();
		
		_collectDistances (repre.structure as LSystemGraph, repre.structure as LSystemGraph);
		
		float maxDistance = -999f;
		foreach (float distance in distances)
		{
			if(distance > maxDistance)
			{
				maxDistance = distance;
			}
		}
		
		return maxDistance;
	}
	
	void _collectDistances (LSystemGraph root, LSystemGraph node)
	{
		if (node == null)
			return;

		distances.Add(Vector3.Distance(root.position, node.position));

		foreach (LSystemGraph child in node.neighbour)
		{
			_collectDistances(root, child);
		}
	}
}
