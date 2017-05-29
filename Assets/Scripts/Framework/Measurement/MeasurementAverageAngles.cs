using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementAverageAngles : Measurement
{
	private List<float> angles = new List<float> ();

	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;
		
		if (repre.structure == null)
			return 0;

		angles.Clear ();

		_collectAngles (repre.structure as LSystemGraph);

		float averageAngle = 0;
		foreach (float angle in angles)
		{
			averageAngle += angle;
		}
		averageAngle /= (float)angles.Count;

		return averageAngle;
	}

	void _collectAngles (LSystemGraph node)
	{
		if (node == null)
			return;

		foreach (LSystemGraph child in node.neighbour)
		{
			if(!child.isBranchingNode)
			{
				angles.Add(Quaternion.Angle(node.orientation, child.orientation));
			}

			_collectAngles(child);
		}
	}
}
