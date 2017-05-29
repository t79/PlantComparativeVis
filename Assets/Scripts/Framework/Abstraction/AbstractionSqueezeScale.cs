using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionSqueezeScale : Abstraction
{
	public float   shrinkPercentage      = 50f;
	public Vector2 structureSize         = new Vector2();
	public Vector2 structureNewSize      = new Vector2();
	
	private TreeNode structureTree;
	
	float minXSize =  999f;
	float maxXSize = -999f;

	float minYSize =  999f;
	float maxYSize = -999f;

	float scaleY = 0.5f;

	public AbstractionSqueezeScale()
	{
		dimension = new Vector3 (0.5f, 0.5f, 0);

		scale = 1f;
	}

	public AbstractionSqueezeScale(float scale)
	{
		dimension = new Vector3 (0.5f, 0.5f, 0);

		this.scale = scale;
	}
	
	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;
		
		result.type       = "mesh";
		result.dimension  = 1;
		result.gameObject = new GameObject (repre.gameObject.name);
		
		minXSize =  999f;
		maxXSize = -999f;
		
		_recFillSizeValues (repre.goList, repre.structure as LSystemGraph, ref minXSize, ref maxXSize);
		
		float scaleSize = 0.5f;
		if (objectSize.x != 0)
		{
			if (objectSize.x < (repre.size.x * 0.5f))
			{
				scaleSize = objectSize.x / repre.size.x;
			}
			else
			{
				//scaleSize = 0.5f;
				scaleSize = scale;
			}
		}
		else
		{
			scaleSize = scale;
		}

		scaleSize = scale;
		
		structureSize     = new Vector2 (minXSize, maxXSize);
		structureNewSize  = new Vector2 (minXSize * scaleSize, maxXSize * scaleSize);

		structureNewSize.x -= 1f;
		structureNewSize.y += 1f;

		result.structure = straightBranches (repre.structure);
		
		generateTree (result);
		
		deformStructure (structureTree);
		
		updateStructure (structureTree, result.structure as LSystemGraph);
		
		createMesh (result.structure, result.gameObject, result.goList);

		scaleY = 0.5f;
		if (objectSize.y != 0)
		{
			if (objectSize.y < (maxYSize * 0.5f))
			{
				scaleY = objectSize.y / maxYSize;
			}
			else
			{
				scaleY = scale;
			}
		}
		else
		{
			scaleY = scale;
		}
		scaleY = scale;

		transform (result.gameObject, result.goList);

		updateSize(result.gameObject, result.structure as LSystemGraph);

		result.offset = new Vector3 (-(maxXSize + minXSize) / 2, 0, 0);

		return result;
	}
	
	private void deformStructure (TreeNode node)
	{
		if (node == null)
			return;
		
		if (node.hullRight.x > structureNewSize.y)
		{
			float yValue = newYHullCoordinate(node.startPosNew, node.hullRight, structureNewSize.y);
			
			Vector3 newHullVector = new Vector3(structureNewSize.y, yValue, 0);
			
			Quaternion quat = Quaternion.FromToRotation((node.hullRight - node.hullStart).normalized, (newHullVector - node.hullStart).normalized);
			rotateTree(node, quat, Vector3.zero);
		}
		else if(node.hullLeft.x < structureNewSize.x)
		{
			float yValue = newYHullCoordinate(node.startPosNew, node.hullLeft, structureNewSize.x);
			
			Vector3 newHullVector = new Vector3(structureNewSize.x, yValue, 0);
			
			Quaternion quat = Quaternion.FromToRotation((node.hullLeft - node.hullStart).normalized, (newHullVector - node.hullStart).normalized);
			rotateTree(node, quat, Vector3.zero);
		}
		
		deformStructure (node.firstChild);
		deformStructure (node.secondChild);
	}
	
	void createMesh(Graph node, GameObject parent, List<GameObject> goList)
	{
		LSystemGraph lNode = node as LSystemGraph;
		
		if (lNode == null)
			return;
		
		GameObject prefab = null;
		float length = 1f;
		float width  = 1f;
		
		switch (lNode.symbol) 
		{
		case "S":
		{
			prefab = Resources.Load("2DSegment") as GameObject;
			length = lNode.length / 2f;
			break;
		}
		case "T":
		{
			prefab = Resources.Load("2DTip") as GameObject;
			break;
		}
		case "L":
		{
			prefab = Resources.Load("2DLeaf") as GameObject;
			length = lNode.length;
			width  = lNode.length;
			break;
		}
		default:
		{
			GameObject gameObject              = new GameObject();
			gameObject.transform.localPosition = lNode.position;
			gameObject.transform.rotation      = lNode.orientation;
			gameObject.transform.localScale    = new Vector3 (width, length, gameObject.transform.localScale.z);
			gameObject.name                    = lNode.symbol;
			gameObject.transform.parent        = parent.transform;
			
			goList.Add(gameObject);
			foreach (Graph gNode in node.neighbour)
				createMesh (gNode, parent, goList);
			
			return;
		}
		}
		
		GameObject go = GameObject.Instantiate (prefab) as GameObject;
		go.transform.localPosition = lNode.position;
		go.transform.rotation      = lNode.orientation;
		go.transform.localScale    = new Vector3 (width, length, go.transform.localScale.z);
		go.name                    = lNode.symbol;
		go.transform.parent        = parent.transform;

		Bounds goBound = go.GetComponent<Renderer> ().bounds;

		if (maxYSize < goBound.max.y)
		{
			maxYSize = goBound.max.y;
		}

		if(minYSize > goBound.min.y)
		{
			minYSize = goBound.min.y;
		}

		goList.Add (go);
		foreach (Graph gNode in node.neighbour)
			createMesh (gNode, parent, goList);
	}
	
	private void _recFillSizeValues(List<GameObject> goList, LSystemGraph node, ref float minXSize, ref float maxXSize)
	{
		if (goList [node.id].GetComponent<Renderer>() != null)
		{
			Vector3 endPos = elementEndPos(node);
			
			if(endPos.x < minXSize)
				minXSize = endPos.x;
			
			if(endPos.x > maxXSize)
				maxXSize = endPos.x;
		}
		
		foreach (LSystemGraph child in node.neighbour)
		{
			_recFillSizeValues (goList, child, ref minXSize, ref maxXSize);
		}
	}
	
	private Vector3 elementEndPos( LSystemGraph node )
	{
		Vector3 position       = node.position;
		Quaternion orientation = node.orientation;
		float length           = node.length;
		float leafScale        = node.symbol == "L" ? 3f : 1f;
		
		return position + orientation * (4.0f * length * leafScale * Vector3.up);
	}
	
	Graph straightBranches(Graph root)
	{
		LSystemGraph tmpRoot = root as LSystemGraph;
		
		LSystemGraph result    = new LSystemGraph ();
		result.id              = tmpRoot.id;
		result.symbol          = tmpRoot.symbol;
		result.isBranchingNode = tmpRoot.isBranchingNode;
		result.length          = tmpRoot.length;
		result.angle           = tmpRoot.angle;
		result.position        = tmpRoot.position;
		result.orientation     = tmpRoot.orientation;
		
		foreach (LSystemGraph child in tmpRoot.neighbour)
		{
			result.neighbour.Add(_recStraightBranches(result, child));
		}
		
		return result;
	}
	
	Graph _recStraightBranches(LSystemGraph parent, LSystemGraph root)
	{
		LSystemGraph node    = new LSystemGraph ();
		node.id              = root.id;
		node.symbol          = root.symbol;
		node.length          = root.length;
		node.isBranchingNode = root.isBranchingNode;
		node.parent          = parent;
		
		if(!node.isBranchingNode)
			node.angle = 0;
		else
			node.angle = root.angle;
		
		node.orientation = parent.orientation * Quaternion.AngleAxis(node.angle, Vector3.forward);
		node.position    = parent.position + parent.orientation * (4.0f * parent.length * Vector3.up);
		
		foreach (LSystemGraph child in root.neighbour)
		{
			node.neighbour.Add(_recStraightBranches(node, child));
		}
		
		return node;
	}
	
	void generateTree (RepresentationMesh repre)
	{
		structureTree = getNode (repre.structure as LSystemGraph);
		
		copyOldToNew  (structureTree);
		
		fillHullInformation (structureTree, repre.structure as LSystemGraph);
	}
	
	TreeNode getNode(LSystemGraph structureGraph)
	{
		TreeNode node            = new TreeNode ();
		node.startIndex          = structureGraph.id;
		node.startPosOld         = structureGraph.position;
		node.originalOrientation = structureGraph.orientation;
		
		while (structureGraph != null && structureGraph.neighbour.Count > 2)
		{
			structureGraph = structureGraph.neighbour[2] as LSystemGraph;
		}
		
		if ((structureGraph.neighbour.Count == 1) && (structureGraph.neighbour [0] as LSystemGraph).symbol == "T")
		{
			structureGraph = structureGraph.neighbour[0] as LSystemGraph;
		}
		
		node.endIndex  = structureGraph.id;
		node.endPosOld = elementEndPos(structureGraph);
		
		if (structureGraph.neighbour.Count == 2)
		{
			node.firstChild  = getNode(structureGraph.neighbour[0] as LSystemGraph);
			node.secondChild = getNode(structureGraph.neighbour[1] as LSystemGraph);
		}
		
		return node;
	}
	
	void copyOldToNew(TreeNode node)
	{
		if (node == null)
			return;
		
		node.startPosNew = node.startPosOld;
		node.endPosNew   = node.endPosOld;
		
		copyOldToNew (node.firstChild);
		copyOldToNew (node.secondChild);
	}
	
	void fillHullInformation (TreeNode node, LSystemGraph root)
	{
		if (node == null)
			return;
		
		preFillHullInformation (node, root);
		
		fillHullInformation (node.firstChild, root);
		fillHullInformation (node.secondChild, root);
	}
	
	private float DistanceToLine(Ray ray, Vector3 point)
	{
		return Vector3.Cross (ray.direction, point - ray.origin).z;
	}
	
	void preFillHullInformation (TreeNode node, LSystemGraph root)
	{
		node.hullStart = node.startPosNew;
		node.hullEnd   = node.endPosNew;
		
		Vector3 hullDirection = (node.hullEnd - node.hullStart).normalized;
		
		node.hullLeft  = node.hullEnd + new Vector3(-hullDirection.y,  hullDirection.x, hullDirection.z);
		node.hullRight = node.hullEnd + new Vector3( hullDirection.y, -hullDirection.x, hullDirection.z);
		
		float max = -99;
		float min =  99;
		for(int i = node.startIndex; i <= node.endIndex; i++)
		{
			Vector3 pos = elementEndPos(getGraphNode(i, root));
			pos.z = 0;
			float distance = DistanceToLine(new Ray(node.hullStart, node.hullEnd - node.hullStart), pos);
			float angle = Vector3.Angle(node.hullEnd - node.hullStart, pos - node.hullStart);
			float distanceTreshold = (pos - node.hullStart).magnitude * Mathf.Cos(angle * Mathf.Deg2Rad);
			
			distance *= distanceTreshold < 32 ? 0f : 1;
			
			if(distance > 0)
			{
				if(distance > max)
				{
					max = distance;
					node.hullLeft = pos;
				}
			}
			else if(distance < 0)
			{
				if(distance < min)
				{
					min = distance;
					node.hullRight = pos;
				}
			}
		}
		
		float hullDiameter = (node.hullEnd - node.hullStart).magnitude;
		node.hullLeft  = node.hullStart + hullDiameter * (node.hullLeft  - node.hullStart).normalized;
		node.hullRight = node.hullStart + hullDiameter * (node.hullRight - node.hullStart).normalized;
	}
	
	LSystemGraph getGraphNode(int index, LSystemGraph root)
	{
		if (root.id != index)
		{
			foreach(LSystemGraph child in root.neighbour)
			{
				LSystemGraph lsg = getGraphNode(index, child);
				if(lsg != null)
					return lsg;
			}
		}
		else
		{
			return root;
		}
		
		return null;
	}
	
	float newYHullCoordinate(Vector3 startPoint, Vector3 fromPoint, float xValue)
	{
		float result = (fromPoint - startPoint).sqrMagnitude;
		
		result -= Mathf.Pow (startPoint.x - xValue, 2f);
		
		result = startPoint.y + Mathf.Sqrt (result);
		
		return result;
	}
	
	void rotateTree(TreeNode node, Quaternion quat, Vector3 translate)
	{
		if (node == null)
			return;
		
		Vector3 move  = node.startPosNew;
		Vector3 move2 = node.endPosNew;
		
		node.startPosNew -= move;
		node.endPosNew   -= move;
		
		node.hullStart   -= move;
		node.hullEnd     -= move;
		
		node.hullLeft    -= move;
		node.hullRight   -= move;
		
		node.startPosNew  = quat * node.startPosNew;
		node.endPosNew    = quat * node.endPosNew;
		
		node.hullStart    = quat * node.hullStart;
		node.hullEnd      = quat * node.hullEnd;
		
		node.hullLeft     = quat * node.hullLeft;
		node.hullRight    = quat * node.hullRight;
		
		node.startPosNew += translate + move;
		node.endPosNew   += translate + move;
		
		node.hullStart   += translate + move;
		node.hullEnd     += translate + move;
		
		node.hullLeft    += translate + move;
		node.hullRight   += translate + move;
		
		move2 = node.endPosNew - move2;
		
		rotateTree (node.firstChild, quat, move2);
		rotateTree (node.secondChild, quat, move2);
	}
	
	void _recRotateBranch (LSystemGraph parent, LSystemGraph node, int endIndex)
	{
		if (node.id > endIndex)
			return;
		
		node.orientation = parent.orientation * Quaternion.AngleAxis(node.angle, Vector3.forward);
		node.position    = parent.position + parent.orientation * (4.0f * parent.length * Vector3.up);
		
		foreach (LSystemGraph child in node.neighbour)
			_recRotateBranch (node, child, endIndex);
	}
	
	void _translateBranch (LSystemGraph node, int endIndex, Vector3 toOldPosition)
	{
		if (node.id > endIndex)
			return;
		
		node.position += toOldPosition;
		
		foreach (LSystemGraph child in node.neighbour)
			_translateBranch (child, endIndex, toOldPosition);
	}
	
	void snapStructureToFinalTreeNode (TreeNode node, LSystemGraph root)
	{
		Vector3 oldDir = node.endPosOld - node.startPosOld;
		Vector3 newDir = node.endPosNew - node.startPosNew;
		
		Quaternion toNewOrientation = Quaternion.FromToRotation (oldDir, newDir);
		Vector3 toOldPosition       = node.startPosNew;
		
		LSystemGraph lSystemNode = getGraphNode(node.startIndex, root);
		
		lSystemNode.position    = new Vector3 (0, 0, 0);
		lSystemNode.orientation = toNewOrientation * node.originalOrientation;
		
		foreach(LSystemGraph child in lSystemNode.neighbour)
			_recRotateBranch (lSystemNode, child, node.endIndex);
		
		_translateBranch (lSystemNode, node.endIndex, toOldPosition);
	}
	
	void updateStructure (TreeNode node, LSystemGraph root)
	{
		if (node == null)
			return;
		
		snapStructureToFinalTreeNode (node, root);
		
		updateStructure (node.firstChild,  root);
		updateStructure (node.secondChild, root);
	}

	void transform (GameObject go, List<GameObject> resGO)
	{
		for(int i = 0; i < resGO.Count; i++)
		{
			GameObject resObject     = resGO[i].gameObject;
			Mesh       mesh          = Mesh.Instantiate(resObject.GetComponent<MeshFilter>().sharedMesh) as Mesh;
			Vector3[]  vertices      = mesh.vertices;
			
			int j = 0;
			while (j < vertices.Length)
			{

				vertices[j] = resObject.transform.rotation * Vector3.Scale(vertices[j], resObject.transform.localScale) + resObject.transform.position;
				vertices[j].y *= scaleY;
				
				j++;
			}
			
			mesh.vertices = vertices;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			
			resObject.GetComponent<MeshFilter>().mesh = mesh;
			
			resObject.transform.parent = go.transform;
			resObject.transform.position = Vector3.zero;
			resObject.transform.rotation = Quaternion.identity;
			resObject.transform.localScale = Vector3.one;
		}
	}

	void updateSize(GameObject gameObject, LSystemGraph node)
	{
		if (node == null)
			return;

		GameObject go = gameObject.transform.GetChild (node.id).gameObject;
		
		Mesh       mesh          = Mesh.Instantiate(go.GetComponent<MeshFilter>().sharedMesh) as Mesh;
		Vector3[]  vertices      = mesh.vertices;
		
		Vector3 boxMin = new Vector3 ( 999f,  999f,  999f);
		Vector3 boxMax = new Vector3 (-999f, -999f, -999f);
		int j = 0;
		while (j < vertices.Length)
		{
			Vector3 vertexPos = go.transform.rotation * Vector3.Scale(vertices[j], go.transform.localScale) + go.transform.position;
			
			if(vertexPos.x > boxMax.x)
				boxMax.x = vertexPos.x;
			
			if(vertexPos.y > boxMax.y)
				boxMax.y = vertexPos.y;
			
			if(vertexPos.z > boxMax.z)
				boxMax.z = vertexPos.z;
			
			if(vertexPos.x < boxMin.x)
				boxMin.x = vertexPos.x;
			
			if(vertexPos.y < boxMin.y)
				boxMin.y = vertexPos.y;
			
			if(vertexPos.z < boxMin.z)
				boxMin.z = vertexPos.z;
			
			j++;
		}
		
		node.size = boxMax - boxMin;
		
		foreach (LSystemGraph child in node.neighbour)
		{
			updateSize(gameObject, child);
		}

	}
}
