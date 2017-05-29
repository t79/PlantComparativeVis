using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionCopy : Abstraction
{		
	public AbstractionCopy()
	{
		dimension = new Vector3 (1f, 1f, 1f);

		scale = 1f;
	}
	
	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;

		result.dimension  = repre.dimension;
		result.type       = repre.type;
		result.gameObject = new GameObject (repre.gameObject.name);
		
		for(int i = 0; i < repre.goList.Count; i++)
		{
			GameObject resObject     = GameObject.Instantiate(repre.goList[i].gameObject) as GameObject;
			resObject.name = repre.goList[i].gameObject.name;
			resObject.transform.parent = result.gameObject.transform;
			result.goList.Add(resObject);
		}
		
		result.structure = copyStructure (null, repre.structure as LSystemGraph);
		result.graph     = copyGraph (repre.graph);
		
		return result;
	}
	
	LSystemGraph copyStructure (LSystemGraph parent, LSystemGraph node)
	{
		if (node == null)
			return null;
		
		LSystemGraph resNode = new LSystemGraph ();
		resNode.id           = node.id;
		resNode.symbol       = node.symbol;
		resNode.parent       = parent;

		resNode.angle           = node.angle;
		resNode.isBranchingNode = node.isBranchingNode;
		resNode.length          = node.length;
		resNode.orientation     = node.orientation;
		resNode.position        = node.position;
		resNode.size            = node.size;
		
		foreach (LSystemGraph child in node.neighbour)
		{
			resNode.neighbour.Add(copyStructure(resNode, child));
		}
		
		return resNode;
	}

	Dictionary<int, GraphNode> copyGraph (Dictionary<int, GraphNode> graph)
	{
		Dictionary<int, GraphNode> result = new Dictionary<int, GraphNode> ();
		
		foreach(KeyValuePair<int, GraphNode> node in graph)
		{
			GraphNode gNode = new GraphNode();
			
			gNode.id         = node.Value.id;
			gNode.symbol     = node.Value.symbol;
			gNode.position   = node.Value.position;
			gNode.node       = node.Value.node;
			gNode.distance   = node.Value.distance;
			gNode.prev       = node.Value.prev;
			gNode.visited    = node.Value.visited;
			gNode.start      = node.Value.start;
			gNode.finish     = node.Value.finish;
			gNode.neighbours = new List<GraphNode> ();
			
			result[node.Key] = gNode;
		}
		
		foreach(KeyValuePair<int, GraphNode> node in graph)
		{
			foreach(GraphNode neighbour in node.Value.neighbours)
			{
				result[node.Value.id].neighbours.Add(result[neighbour.id]);
			}
		}
		
		return result;
	}
}
