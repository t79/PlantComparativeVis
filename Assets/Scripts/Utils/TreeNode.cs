using UnityEngine;
using System.Collections;

public class TreeNode
{
	public int startIndex;
	public int endIndex;

	public Vector3 startPosOld;
	public Vector3 endPosOld;

	public Quaternion originalOrientation;

	public Vector3 startPosNew;
	public Vector3 endPosNew;

	public TreeNode firstChild;
	public TreeNode secondChild;

	public Vector3 hullStart;
	public Vector3 hullEnd;
	public Vector3 hullLeft;
	public Vector3 hullRight;

	public LSystemGraph startLSNode;
}
