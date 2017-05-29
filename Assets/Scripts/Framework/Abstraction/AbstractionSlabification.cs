using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionSlabification : Abstraction
{	
	private TreeNode structureTree;

	float minZ =  999f;
	float maxZ = -999f;

	public AbstractionSlabification()
	{
		dimension = new Vector3 (1f, 1f, 0.5f);

		scale = 1f;
	}
	
	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;
		
		result.type       = "mesh";
		result.dimension  = 3;
		result.gameObject = new GameObject (repre.gameObject.name);
		
		result.structure = copyStructure (null, repre.structure as LSystemGraph);

		structureTree = getNode (result.structure as LSystemGraph);
		
		copyOldToNew (structureTree);

		snapTreeTo2D (structureTree);

		structureToTree (structureTree);

		fillHullInformation (structureTree, result.structure as LSystemGraph);

		rotateGloballyToCenter(structureTree);

		structureToFinalTree (structureTree);

		minZ =  999f;
		maxZ = -999f;

		//Material nMat = Material.Instantiate(repre.gameObject.transform.GetChild (0).gameObject.GetComponent<MeshRenderer> ().material);

		createMesh (result.structure, result.gameObject, result.goList/*, nMat*/);

		result.offset = new Vector3 (0, 0,  -(maxZ + minZ) / 2);
		
		return result;
	}

	void rotateGloballyToCenter(TreeNode node)
	{
		Vector3 nodeHullLeftDir  = (node.hullLeft  - node.hullStart).normalized;
		Vector3 nodeHullRighrDir = (node.hullRight - node.hullStart).normalized;

		Vector3 nodeDir = (nodeHullLeftDir + nodeHullRighrDir).normalized;

		Vector3 globalDir = new Vector3 (0, 1f, 0);

		Quaternion quat = Quaternion.FromToRotation (nodeDir, globalDir);

		rotateTree (node, quat, Vector3.zero);
	}

	void snapStructureToFinalTreeNode (TreeNode node)
	{
		Vector3 oldDir = node.endPosOld - node.startPosOld;
		Vector3 newDir = node.endPosNew - node.startPosNew;
		
		Quaternion toNewOrientation = Quaternion.FromToRotation (oldDir, newDir);
		Vector3 toOldPosition       = node.startPosNew;
		
		node.startLSNode.position = new Vector3 (0, 0, 0);
		node.startLSNode.orientation = toNewOrientation * node.originalOrientation;
		
		rotateBranch (node.startLSNode, node.endIndex);
		translateBranch (node.startLSNode, node.endIndex, toOldPosition);
	}
	
	void structureToFinalTree (TreeNode node)
	{
		if (node == null)
			return;
		
		snapStructureToFinalTreeNode (node);
		
		structureToFinalTree (node.firstChild);
		structureToFinalTree (node.secondChild);
	}

	void snapStructureToTreeNode (TreeNode node)
	{
		Vector3 oldDir = node.endPosOld - node.startPosOld;
		Vector3 newDir = node.endPosNew - node.startPosNew;

		Quaternion toNewOrientation = Quaternion.FromToRotation (oldDir, newDir);
		Vector3 toOldPosition       = node.startPosNew;

		node.startLSNode.position = new Vector3 (0, 0, 0);
		node.startLSNode.orientation = toNewOrientation * node.startLSNode.orientation;

		rotateBranch (node.startLSNode, node.endIndex);
		translateBranch (node.startLSNode, node.endIndex, toOldPosition);
	}

	void rotateBranch (LSystemGraph node, int endIndex)
	{	
		Interpreter current = new Interpreter (node.orientation, node.position);

		Stack<Interpreter> stack = new Stack<Interpreter> ();
		stack.Push(current);
		
		bool branching = false;
		bool wasLast = false;

		current.position = current.position + current.direction * node.spacePos;

		if (node.neighbour.Count == 0)
			return;

		node = node.neighbour[0] as LSystemGraph;

		while (!wasLast)
		{
			switch(node.symbol)
			{
			case "m":
				
				if(!branching)
				{
					current.direction = current.direction * Quaternion.Euler(node.spaceAngle);
				}

				node.position    = current.position;
				node.orientation = current.direction;
				
				current.position = current.position + current.direction * node.spacePos;
				branching = false;
				break;
				
			default:
				break;
			}

			if(node.id == endIndex)
			{
				wasLast = true;
			}
			else
			{
				node = node.neighbour[0] as LSystemGraph;
			}
		}
	}

	void translateBranch (LSystemGraph node, int endIndex, Vector3 toOldPosition)
	{
		bool wasLast = false;
		while (!wasLast)
		{
			node.position += toOldPosition; 

			if(node.id == endIndex)
			{
				wasLast = true;
			}
			else
			{
				node = node.neighbour[0] as LSystemGraph;
			}
		}
	}

	void structureToTree (TreeNode node)
	{
		if (node == null)
			return;
		
		snapStructureToTreeNode (node);
		
		structureToTree (node.firstChild);
		structureToTree (node.secondChild);
	}

	void snapTreeTo2D (TreeNode node)
	{
		if (node == null)
			return;
		
		Vector3 start = node.startPosNew;
		Vector3 end   = node.endPosNew;
		Vector3 move  = -start;
		
		start += move;
		end   += move;
		
		Vector3 nodeDirY = end - start;
		nodeDirY.y = 0;
		float xDirection = nodeDirY.x >= 0 ? 1f : -1f;
		float zDirection = nodeDirY.z >= 0 ? 1f : -1f;
		Vector3 desDirY = new Vector3 (xDirection * 1f, 0f, 0f);
		
		float angle = Vector3.Angle (desDirY, nodeDirY);
		
		Vector3 axis = new Vector3 (0, 1f, 0);
		Quaternion rot = Quaternion.AngleAxis (xDirection * zDirection * angle, axis);
		
		Vector3 nDir   = rot * end - start;
		
		applyTranslation (node.firstChild,  node.startPosNew + nDir - node.endPosNew);
		applyTranslation (node.secondChild, node.startPosNew + nDir - node.endPosNew);

		node.endPosNew   = node.startPosNew + nDir;
		
		snapTreeTo2D (node.firstChild);
		snapTreeTo2D (node.secondChild);
	}

	void applyTranslation(TreeNode node, Vector3 direction)
	{
		if (node == null)
			return;
		
		node.startPosNew = node.startPosNew + direction;
		node.endPosNew   = node.endPosNew + direction;
		
		applyTranslation (node.firstChild, direction);
		applyTranslation (node.secondChild, direction);
	}

	void createMesh(Graph node, GameObject parent, List<GameObject> goList/*, Material nMat*/)
	{
		LSystemGraph lNode = node as LSystemGraph;
		
		if (lNode == null)
			return;
		
		GameObject prefab = null;
		
		switch (lNode.symbol) 
		{
		case "m":
		{
			prefab = Resources.Load("PARP/adp") as GameObject;
			break;
		}
		default:
		{
			GameObject gameObject              = new GameObject();
			gameObject.transform.localPosition = lNode.position;
			gameObject.transform.rotation      = lNode.orientation;
			gameObject.name                    = lNode.symbol;
			gameObject.transform.parent        = parent.transform;
			lNode.size                         = new Vector3();
			
			goList.Add(gameObject);
			foreach (Graph gNode in node.neighbour)
				createMesh (gNode, parent, goList/*, nMat*/);
			
			return;
		}
		}
		
		GameObject go = GameObject.Instantiate (prefab) as GameObject;
		go.transform.localPosition = lNode.position;
		go.transform.rotation      = lNode.orientation;
		go.name                    = lNode.symbol;
		go.transform.parent        = parent.transform;

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

			if(vertexPos.z < minZ)
			{
				minZ = vertexPos.z;
			}
			
			if(vertexPos.z > maxZ)
			{
				maxZ = vertexPos.z;
			}
			
			j++;
		}
		
		lNode.size = boxMax - boxMin;
		/*
		go.GetComponent<MeshRenderer> ().material = nMat;
		*/
		goList.Add (go);
		foreach (Graph gNode in node.neighbour)
			createMesh (gNode, parent, goList/*, nMat*/);
	}

	TreeNode getNode(LSystemGraph structureGraph)
	{
		TreeNode node            = new TreeNode ();
		node.startIndex          = structureGraph.id;
		node.startPosOld         = structureGraph.position;
		node.originalOrientation = structureGraph.orientation;

		node.startLSNode = structureGraph;

		while (structureGraph != null && structureGraph.neighbour.Count == 1)
		{
			structureGraph = structureGraph.neighbour[0] as LSystemGraph;
		}
		
		node.endIndex  = structureGraph.id;
		node.endPosOld = structureGraph.position;
		
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

		// continue only if you have kids
		if (node.firstChild == null || node.secondChild == null)
			return;

		Vector3 firstChildRightDir = (node.firstChild.hullRight - node.firstChild.hullStart).normalized;
		Vector3 secondChildLeftDir  = (node.secondChild.hullLeft  - node.secondChild.hullStart).normalized;
		
		Vector3 nodeDir = (node.hullEnd - node.hullStart).normalized;

		float fcnAngle = Vector3.Angle (firstChildRightDir, nodeDir);
		float scnAngle = Vector3.Angle (secondChildLeftDir, nodeDir);

		if (fcnAngle > 5)
		{
			Quaternion quat = Quaternion.FromToRotation(firstChildRightDir, nodeDir);
			rotateTree(node.firstChild, quat, Vector3.zero);
		}

		if (scnAngle > 5)
		{
			Quaternion quat = Quaternion.FromToRotation(secondChildLeftDir, nodeDir);
			rotateTree(node.secondChild, quat, Vector3.zero);
		} 
		
		computeNewHull(node);
	}

	void computeNewHull(TreeNode node)
	{
		if (node.firstChild == null || node.secondChild == null)
			return;
		
		Vector3 kidsHullLeft = node.firstChild.hullLeft;;
		Vector3 kidsHullRight = node.secondChild.hullRight;;

		Vector3 hullDir = (node.startPosNew - node.endPosNew);
		
		if (Vector3.Angle (hullDir, node.hullLeft) < Vector3.Angle (hullDir, kidsHullLeft))
			node.hullLeft = kidsHullLeft;
		
		if (Vector3.Angle (hullDir, node.hullRight) < Vector3.Angle (hullDir, kidsHullRight))
			node.hullRight = kidsHullRight;
		
		float hullDistance = Mathf.Max (new float[] {
			(node.firstChild.hullLeft - node.hullStart).magnitude,
			(node.firstChild.hullRight - node.hullStart).magnitude,
			(node.secondChild.hullLeft - node.hullStart).magnitude,
			(node.secondChild.hullRight - node.hullStart).magnitude
		});
		
		node.hullLeft  = node.hullStart + hullDistance * (node.hullLeft  - node.hullStart).normalized;
		node.hullRight = node.hullStart + hullDistance * (node.hullRight - node.hullStart).normalized;
		node.hullEnd   = node.hullStart + hullDistance * (node.hullLeft  + node.hullRight).normalized;
	}
	
	private float DistanceToLine(Ray ray, Vector3 point)
	{
		return Vector3.Cross (ray.direction, point - ray.origin).z;
	}
	
	void preFillHullInformation (TreeNode node, LSystemGraph root)
	{
		node.hullStart   = node.startPosNew;
		node.hullStart.z = 0;
		node.hullEnd     = node.endPosNew;
		node.hullEnd.z   = 0;
		
		Vector3 hullDirection = (node.hullEnd - node.hullStart).normalized;
		
		node.hullLeft  = node.hullEnd + new Vector3(-hullDirection.y,  hullDirection.x, hullDirection.z);
		node.hullRight = node.hullEnd + new Vector3( hullDirection.y, -hullDirection.x, hullDirection.z);
		
		float max = -99f;
		float min =  99f;
		for(int i = node.startIndex; i <= node.endIndex; i++)
		{
			Vector3 pos = getGraphNode(i, root).position;
			pos.z = 0;
			float distance = DistanceToLine(new Ray(node.hullStart, node.hullEnd - node.hullStart), pos);
			float angle = Vector3.Angle(node.hullEnd - node.hullStart, pos - node.hullStart);
			float distanceTreshold = (pos - node.hullStart).magnitude * Mathf.Cos(angle * Mathf.Deg2Rad);

			distance *= distanceTreshold < 1.6f ? 0f : 1;

			if((distance > 0) && (distance > max))
			{
				max = distance;
				node.hullLeft = pos;


			}
			else if((distance < 0) && (distance < min))
			{
				min = distance;
				node.hullRight = pos;
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

	LSystemGraph copyStructure (LSystemGraph parent, LSystemGraph node)
	{
		if (node == null)
			return null;
		
		LSystemGraph resNode = new LSystemGraph ();
		resNode.id          = node.id;
		resNode.symbol      = node.symbol;
		resNode.parent      = parent;
		resNode.position    = node.position;
		resNode.orientation = node.orientation;
		resNode.spacePos    = node.spacePos;
		resNode.spaceAngle  = node.spaceAngle;
		
		foreach (LSystemGraph child in node.neighbour)
		{
			resNode.neighbour.Add(copyStructure(resNode, child));
		}
		
		return resNode;
	}
}
