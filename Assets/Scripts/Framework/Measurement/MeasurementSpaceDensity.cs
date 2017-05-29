using UnityEngine;
using System.Collections;

public class MeasurementSpaceDensity : Measurement
{

	private float   elementVol;
	private Vector3 boxMin;
	private Vector3 boxMax;
	
	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;
		
		if (repre.structure == null)
			return 0;
		
		elementVol = 0;
		boxMin = new Vector3 ( 999f,  999f,  999f);
		boxMax = new Vector3 (-999f, -999f, -999f);
		
		_fillValues (repre.structure as LSystemGraph);

		float boxVolume = (boxMax.x - boxMin.x) * (boxMax.y - boxMin.y) * (boxMax.z - boxMin.z);

		float result = 999;
		if(elementVol != 0 && boxVolume !=0)
			result = elementVol / boxVolume;

		return result;
	}
	
	void _fillValues (LSystemGraph node)
	{
		if (node == null)
			return;

		elementVol += node.size.x * node.size.y * node.size.z;

		if(node.position.x > boxMax.x)
			boxMax.x = node.position.x;
		
		if(node.position.y > boxMax.y)
			boxMax.y = node.position.y;
		
		if(node.position.z > boxMax.z)
			boxMax.z = node.position.z;
		
		if(node.position.x < boxMin.x)
			boxMin.x = node.position.x;
		
		if(node.position.y < boxMin.y)
			boxMin.y = node.position.y;
		
		if(node.position.z < boxMin.z)
			boxMin.z = node.position.z;

		foreach (LSystemGraph child in node.neighbour)
		{
			_fillValues(child);
		}
	}
}
