using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementDensityShortestDistance : Measurement
{
	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;
		
		if (repre.structure == null)
			return 0;

		int startIndex = -1;
		int endIndex   = -1;

		foreach (KeyValuePair<int, GraphNode> node in repre.graph)
		{
			if(node.Value.start)
			{
				startIndex = node.Key;
			}

			if(node.Value.finish)
			{
				endIndex = node.Key;
			}
		}

		Dijkstra (startIndex, repre.graph);

		float result = repre.graph [endIndex].distance;

		return result;
	}

	void Dijkstra(int source, Dictionary<int, GraphNode> graph)
	{
		List<GraphNode> unvisited = new List<GraphNode> ();
		graph [source].distance = 0;
		graph [source].prev     = -1;
		
		foreach (KeyValuePair<int, GraphNode> node in graph)
		{
			if(node.Key != source)
			{
				node.Value.distance = 999999f + (float)node.Value.id;
				node.Value.prev     = -1;
			}
			
			unvisited.Add(node.Value);
		}
		
		while (unvisited.Count > 0)
		{
			float min = System.Single.MaxValue;
			int index = -1;
			for(int i = 0; i < unvisited.Count; i++)
			{
				if(unvisited[i].distance < min)
				{
					min   = unvisited[i].distance;
					index = i;
				}
			}
			
			GraphNode u = unvisited[index];
			unvisited.RemoveAt(index);
			
			foreach( GraphNode v in u.neighbours )
			{
				float alt = u.distance + (u.position - v.position).magnitude;
				
				if(alt < v.distance)
				{
					v.distance = alt;
					v.prev = u.id;
				}
			}
		}
	}
}
