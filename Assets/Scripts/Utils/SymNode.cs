using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SymNode
{
	public int level     = 0;
	public int stemCount = 0;
	public int allCount  = 0;

	public List<SymNode> leftBranches  = new List<SymNode> ();
	public List<SymNode> rightBranches = new List<SymNode> ();
}
