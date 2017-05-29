using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementSymmetryStructural : Measurement
{
	public struct SumElement
	{
		public int   level;
		public float delta;
		public float weight;
	}

	public class SymElement
	{
		public int level;
		public int stemCount;
		public int allCount;
		
		public List<SymElement> leftBranches;
		public List<SymElement> rightBranches;
	}

	public int level = 2;

	private List<SumElement> asymetryList = new List<SumElement> ();
	private SymElement       symmetryNode = null;

	int _countParts(LSystemGraph structNode)
	{
		if (structNode == null)
			return 0;

		int sum = 1;
		foreach (LSystemGraph child in structNode.neighbour)
		{
			sum += _countParts(child);
		}

		return sum;
	}

	SymElement _getSymNode (LSystemGraph structNode, int level)
	{
		if (structNode == null)
			return null;
		
		SymElement node    = new SymElement ();
		node.level         = level;
		node.stemCount     = 0;
		node.allCount      = 0;
		node.leftBranches  = new List<SymElement> ();
		node.rightBranches = new List<SymElement> ();

		while (structNode.neighbour.Count != 1 && structNode.neighbour.Count != 0)
		{
			if(structNode.neighbour.Count != 2)
			{
				node.stemCount += structNode.neighbour.Count;
				structNode = structNode.neighbour[2] as LSystemGraph;
			}
			else
			{
				if(level > 0)
				{
					if((structNode.neighbour[0] as LSystemGraph).angle < 0)
					{
						node.rightBranches.Add(_getSymNode(structNode.neighbour[0] as LSystemGraph, level - 1));
					}
					else
					{
						node.leftBranches.Add(_getSymNode(structNode.neighbour[0] as LSystemGraph, level - 1));
					}
				}
				else
				{
					node.stemCount += _countParts(structNode.neighbour[0] as LSystemGraph);
				}

				structNode = structNode.neighbour[1] as LSystemGraph;
			}
		}

		node.stemCount += structNode.neighbour.Count;

		if (structNode.neighbour.Count == 0)
		{
			node.stemCount++;
		}
		
		int leftSum = 0;
		for (int i = 0; i < node.leftBranches.Count; i++)
		{
			leftSum += node.leftBranches[i].allCount;
		}
		
		int rightSum = 0;
		for (int i = 0; i < node.rightBranches.Count; i++)
		{
			rightSum += node.rightBranches[i].allCount;
		}
		
		node.allCount = node.stemCount + leftSum + rightSum;
		
		return node;
	}

	void _fillNodeValues (SymElement node)
	{
		SumElement se = new SumElement ();
		
		se.level = node.level;
		
		int leftSum = 0;
		for (int i = 0; i < node.leftBranches.Count; i++)
		{
			leftSum += node.leftBranches[i].allCount;
		}
		
		int rightSum = 0;
		for (int i = 0; i < node.rightBranches.Count; i++)
		{
			rightSum += node.rightBranches[i].allCount;
		}

		se.delta  = (leftSum + rightSum) == 0 ? 0 : (float)Mathf.Abs (leftSum - rightSum) / (float)(node.allCount);
		se.weight = (float)node.allCount / (float)symmetryNode.allCount;

		asymetryList.Add (se);
		
		for (int i = 0; i < node.leftBranches.Count; i++)
		{
			_fillNodeValues(node.leftBranches[i]);
		}
		
		for (int i = 0; i < node.rightBranches.Count; i++)
		{
			_fillNodeValues(node.rightBranches[i]);
		}
	}

	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;

		if (repre.structure == null)
			return 0;

		asymetryList.Clear ();
		symmetryNode = null;

		symmetryNode = _getSymNode (repre.structure as LSystemGraph, level);
		
		_fillNodeValues (symmetryNode);

		float returnValue = 0;
		for (int i = 0; i < asymetryList.Count; i++)
		{
			returnValue += asymetryList[i].delta * asymetryList[i].weight;
		}

		return returnValue;
	}
}
