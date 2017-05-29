using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SymmetryVer2
{
	public struct sumElement
	{
		public int   level;
		public float delta;
		public float weight;
	}

	public int              level        = 1;
	public SymNode          symmetryNode = null;
	public int              elemCount    = 0;
	public float            returnValue  = 0;
	public List<sumElement> asymetryList = new List<sumElement> ();

	private List<Symbol> state;

	SymNode _getNode (ref int index, int level)
	{
		if (index >= state.Count)
			return null;
		
		SymNode node       = new SymNode ();
		node.level         = level;
		node.stemCount     = 0;
		node.allCount      = 0;
		node.leftBranches  = new List<SymNode> ();
		node.rightBranches = new List<SymNode> ();

		int stack = 1;
		while (index < state.Count  && (stack >= 1))
		{
			if((state[index].symName == "S") || (state[index].symName == "L"))
			{
				elemCount++;
				node.stemCount++;
				index++;
			}
			else if(state[index].symName == "B")
			{
				if(level > 0)
				{
					index++;
					if(state[index].parameters["angle"].value > 0)
					{
						node.leftBranches.Add(_getNode(ref index, level - 1));
					}
					else
					{
						node.rightBranches.Add(_getNode(ref index, level - 1));
					}
				}
				else
				{
					stack++;
					index++;
				}
			}
			else if((state[index].symName == "E") || (state[index].symName == "T"))
			{
				elemCount++;
				node.stemCount++;
				stack--;
				index++;
			}
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

	void _fillNodeValues (SymNode node)
	{
		sumElement se = new sumElement ();

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

		//se.delta  = (leftSum + rightSum) == 0 ? 0 : (float)Mathf.Abs (leftSum - rightSum) / (float)(leftSum + rightSum);
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


	public void detectSymmetry(int nLevel, List<Symbol> nState)
	{
		level  = nLevel;
		state = nState;

		asymetryList.Clear ();
		elemCount = 0;

		int index = 0;
		symmetryNode = _getNode (ref index, level);

		_fillNodeValues (symmetryNode);

		returnValue = 0;
		for (int i = 0; i < asymetryList.Count; i++)
		{
			returnValue += asymetryList[i].delta * asymetryList[i].weight;
		}
	}
}
