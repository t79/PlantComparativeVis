using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionGraphWarpScale1D : Abstraction
{
	public enum Axis {X, Y, Z};

	public  Axis  scaleAxis  = Axis.X;
	public  float scaleValue = 0.5f; 
	
	float minX =  999f;
	float maxX = -999f;

	private List<Vector2>            controlPoints     = new List<Vector2>();
	private List<Vector2>            warpingPoints     = new List<Vector2>();
	private Dictionary<int, Vector2> deformedPositions = new Dictionary<int, Vector2>();
	private bool                     toRotate          = false;

	public AbstractionGraphWarpScale1D()
	{
		switch (scaleAxis)
		{
		case Axis.X:
			dimension = new Vector3 (0.5f, 1f, 0f);
			break;
		case Axis.Y:
			dimension = new Vector3 (1f, 0.5f, 0f);
			break;
		case Axis.Z:
			dimension = new Vector3 (1f, 1f, 0.5f);
			break;
		}

		scale = 1f;
	}

	public AbstractionGraphWarpScale1D(Axis axis)
	{
		scaleAxis = axis;
		
		switch (scaleAxis)
		{
		case Axis.X:
			dimension = new Vector3 (0.5f, 1f, 0f);
			break;
		case Axis.Y:
			dimension = new Vector3 (1f, 0.5f, 0f);
			break;
		case Axis.Z:
			dimension = new Vector3 (1f, 1f, 0.5f);
			break;
		}

		scale = 1f;
	}

	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;
		
		deformedPositions.Clear ();
		
		result.dimension                       = 2;
		result.type                            = "mesh";
		result.gameObject                      = new GameObject (repre.gameObject.name);
		result.gameObject.transform.position   = repre.gameObject.transform.position;
		result.gameObject.transform.rotation   = repre.gameObject.transform.rotation;
		result.gameObject.transform.localScale = repre.gameObject.transform.localScale;
		
		result.structure = copyStructure (null, repre.structure as LSystemGraph, repre.gameObject);
		result.graph     = copyGraph (repre.graph);
		result.texList   = copyTextures (repre.texList);
		
		fillShortestPathDistances (result.graph);
		setWarpPoints (result.graph);

		Quaternion quat = toRotate ? Quaternion.AngleAxis(90f, new Vector3(0, 0, 1f)) : Quaternion.identity;
		minX =  999f;
		maxX = -999f;
		foreach (KeyValuePair<int, GraphNode> node in result.graph)
		{
			Vector2 resPos = warpPosition(node.Key, new Vector2(node.Value.position.x, node.Value.position.y));
			deformedPositions[node.Key] = quat * resPos;

			if(resPos.x < minX)
			{
				minX = resPos.x;
			}
			
			if(resPos.x > maxX)
			{
				maxX = resPos.x;
			}
		}

		setVisitedFalse (result.graph);
		
		if (objectSize.x != 0)
		{
			if (objectSize.x < ((maxX - minX) * 0.5f))
			{
				scaleValue = objectSize.x / (maxX - minX);
			}
			else
			{
				scaleValue = 0.5f;
			}
		}

		result.structure = updateStructureGraph (result.graph[0], new LSystemGraph());
		result.structure = result.structure.neighbour [0];

		createMesh (result.structure as LSystemGraph, result.gameObject, result.goList);

		result.offset = new Vector3 (-(maxX + minX) / 2, 0, 0);
		
		return result;
	}

	LSystemGraph copyStructure (LSystemGraph parent, LSystemGraph node, GameObject go)
	{
		if (node == null)
			return null;
		
		LSystemGraph resNode = new LSystemGraph ();
		resNode.id           = node.id;
		resNode.symbol       = node.symbol;
		resNode.parent       = parent;
		resNode.angle        = node.angle;
		resNode.length       = node.length;
		resNode.orientation  = node.orientation;
		resNode.position     = node.position;
		
		Vector3 boxMin = new Vector3 ( 999f,  999f,  999f);
		Vector3 boxMax = new Vector3 (-999f, -999f, -999f);
		
		if (node.id == -1)
		{
			boxMax = new Vector3 ();
			boxMin = new Vector3 ();
		}
		else
		{
			GameObject resObject = go.transform.GetChild (node.id).gameObject;
			if (resObject.GetComponent<MeshFilter> () != null)
			{
				Mesh mesh = Mesh.Instantiate (resObject.GetComponent<MeshFilter> ().sharedMesh) as Mesh;
				Vector3[] vertices = mesh.vertices;
				
				int j = 0;
				while (j < vertices.Length)
				{
					Vector3 vertexPos = resObject.transform.rotation * Vector3.Scale (vertices [j], resObject.transform.localScale) + resObject.transform.position;
					
					if (vertexPos.x > boxMax.x)
						boxMax.x = vertexPos.x;
					
					if (vertexPos.y > boxMax.y)
						boxMax.y = vertexPos.y;
					
					if (vertexPos.z > boxMax.z)
						boxMax.z = vertexPos.z;
					
					if (vertexPos.x < boxMin.x)
						boxMin.x = vertexPos.x;
					
					if (vertexPos.y < boxMin.y)
						boxMin.y = vertexPos.y;
					
					if (vertexPos.z < boxMin.z)
						boxMin.z = vertexPos.z;
					j++;
				}
			} else
			{
				boxMin = resObject.transform.position;
				boxMax = resObject.transform.position;
			}
		}
		
		resNode.size = boxMax - boxMin;
		
		foreach (LSystemGraph child in node.neighbour)
		{
			resNode.neighbour.Add(copyStructure(resNode, child, go));
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
	
	Dictionary<string, Texture2D> copyTextures (Dictionary<string, Texture2D>  texList)
	{
		Dictionary<string, Texture2D> result = new Dictionary<string, Texture2D> ();
		
		foreach (KeyValuePair<string, Texture2D> texture in texList)
		{
			result[texture.Key] = Texture2D.Instantiate(texture.Value);
		}
		
		return result;
	}
	
	void fillShortestPathDistances (Dictionary<int, GraphNode> graph)
	{
		int startIndex = -1;
		foreach (KeyValuePair<int, GraphNode> node in graph)
		{
			if(node.Value.start)
			{
				startIndex = node.Key;
				break;
			}
		}
		
		Dijkstra (startIndex, graph);
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

	void setWarpPoints (Dictionary<int, GraphNode> graph)
	{
		List<GraphNode> nearestWayList = new List<GraphNode> ();
		
		int index = -1;
		foreach (KeyValuePair<int, GraphNode> node in graph)
		{
			if(node.Value.finish)
			{
				index = node.Key;
				break;
			}
		}
		
		float distance = graph[index].distance;
		
		while (index != -1)
		{
			GraphNode gp = graph[index];
			nearestWayList.Add(gp);
			index = gp.prev;
		}
		
		warpingPoints.Clear ();
		controlPoints.Clear ();
		
		Vector3 averageVec = new Vector3 ();
		for (int i = 1; i < nearestWayList.Count; i++)
		{
			averageVec += (nearestWayList[i-1].position - nearestWayList[i].position);
		}
		averageVec /= (nearestWayList.Count - 1);
		
		if (Mathf.Abs (averageVec.x) > Mathf.Abs (averageVec.y))
		{
			float yCoord = 0;
			
			foreach(GraphNode node in nearestWayList)
			{
				controlPoints.Add(new Vector2(node.position.x, node.position.y));
				warpingPoints.Add(new Vector2(node.distance - distance / 2f, yCoord));
			}

			toRotate = true;
		}
		else
		{
			float xCoord = 0;
			
			foreach(GraphNode node in nearestWayList)
			{
				controlPoints.Add(new Vector2(node.position.x, node.position.y));
				warpingPoints.Add(new Vector2(xCoord, node.distance - distance / 2f));
			}

			toRotate = false;
		}
		
		controlPoints.Add (new Vector2 (-150f, 0f  ));
		controlPoints.Add (new Vector2 (-150f, 300f));
		controlPoints.Add (new Vector2 ( 150f, 0f  ));
		controlPoints.Add (new Vector2 ( 150f, 300f));
		
		warpingPoints.Add (new Vector2 (-150f, 0f  ));
		warpingPoints.Add (new Vector2 (-150f, 300f));
		warpingPoints.Add (new Vector2 ( 150f, 0f  ));
		warpingPoints.Add (new Vector2 ( 150f, 300f));
	}
	
	Vector2 warpPosition(int index, Vector2 v)
	{
		List<float> dWeights = new List<float> ();
		Vector2 CPCentroid = new Vector2 ();
		Vector2 WPCentroid = new Vector2 ();
		float dWeightSum = 0;
		int controlPointsCount = controlPoints.Count;
		
		// WEIGHT
		for (int i = 0; i < controlPointsCount; i++)
		{
			if(Vector2.Distance(controlPoints[i], v) == 0)
			{
				return warpingPoints[i];
			}
			
			float weight = 1f / Mathf.Pow(Vector2.Distance(controlPoints[i], v), 3f);
			
			dWeights.Add( weight );
			dWeightSum += weight;
		}
		
		// CENTROIDS
		for (int i = 0; i < controlPointsCount; i++)
		{
			CPCentroid += dWeights[i] * controlPoints[i];
			WPCentroid += dWeights[i] * warpingPoints[i];
		}
		
		CPCentroid /= dWeightSum;
		WPCentroid /= dWeightSum;
		
		// DIFFERENCE LIST
		List<Vector2> CPDifference = new List<Vector2> ();
		List<Vector2> WPDifference = new List<Vector2> ();
		float micro = 0;
		
		for (int i = 0; i < controlPointsCount; i++)
		{
			CPDifference.Add(controlPoints[i] - CPCentroid);
			WPDifference.Add(warpingPoints[i] - WPCentroid);
			
			micro += dWeights[i] * Vector2.Dot(controlPoints[i] - CPCentroid, controlPoints[i] - CPCentroid);
		}
		
		// MATRIX LIST
		List<Vector4> matrix = new List<Vector4> ();
		
		for (int i = 0; i < controlPointsCount; i++)
		{
			Vector4 mat = new Vector4();
			
			Vector2 cpd  = CPDifference[i];
			Vector2 cpd2 = new Vector2(cpd.y, -cpd.x);
			Vector2 vpd  = v - CPCentroid;
			Vector2 vpd2 = new Vector2(vpd.y, -vpd.x);
			
			mat.x += dWeights[i] * Vector2.Dot(cpd,  vpd ) / micro;
			mat.y += dWeights[i] * Vector2.Dot(cpd,  vpd2) / micro;
			mat.z += dWeights[i] * Vector2.Dot(cpd2, vpd ) / micro;
			mat.w += dWeights[i] * Vector2.Dot(cpd2, vpd2) / micro;
			
			matrix.Add(mat);
		}
		
		// DEFORMATION FUNCTION
		Vector2 res = new Vector2 ();
		for (int i = 0; i < controlPointsCount; i++)
		{
			res += multiplyVecMat(WPDifference[i], matrix[i]);
		}
		res += WPCentroid;
		
		return res;
	}
	
	Vector2 multiplyVecMat(Vector2 vec, Vector4 mat)
	{
		float xCoord = vec.x * mat.x + vec.y * mat.z;
		float yCoord = vec.x * mat.y + vec.y * mat.w;
		
		return new Vector2 (xCoord, yCoord);
	}
	
	void setVisitedFalse (Dictionary<int, GraphNode> graphNodes)
	{
		foreach (KeyValuePair<int, GraphNode> node in graphNodes)
		{
			node.Value.visited = false;
		}
	}
	
	LSystemGraph updateStructureGraph (GraphNode node, LSystemGraph parent)
	{
		node.visited = true;
		node.position = new Vector3 (deformedPositions [node.id].x * scaleValue, deformedPositions [node.id].y);

		if(node.position.x < minX)
		{
			minX = node.position.x;
		}
		
		if(node.position.x > maxX)
		{
			maxX = node.position.x;
		}
		
		foreach (GraphNode neighbour in node.neighbours)
		{
			LSystemGraph nNode = new LSystemGraph ();
			nNode.id           = node.id;
			nNode.symbol       = "R";

			Vector3 gNodePos = new Vector3 (deformedPositions [node.id].x,      deformedPositions [node.id].y);
			Vector3 gNeigPos = new Vector3 (deformedPositions [neighbour.id].x, deformedPositions [neighbour.id].y);

			gNodePos.x *= scaleValue;
			gNeigPos.x *= scaleValue;

			nNode.position     = gNodePos;
			
			Vector3 nodeChildVec = (gNeigPos - gNodePos);
			
			nNode.orientation = Quaternion.FromToRotation(Vector3.up, nodeChildVec.normalized);
			nNode.length = nodeChildVec.magnitude / 4f;
			
			parent.neighbour.Add(nNode);

			if(!neighbour.visited)
			{
				updateStructureGraph (neighbour, nNode);
			}
		}
		
		return parent;
	}
	
	void createMesh (LSystemGraph node, GameObject parent, List<GameObject> goList)
	{
		if (node == null)
			return;
		
		GameObject prefab = null;
		float length = 1f;
		float width  = 1f;
		
		switch (node.symbol) 
		{
		case "R":
		{
			prefab = Resources.Load("2DSegment") as GameObject;
			length = node.length / 2f;
			break;
		}
		default:
		{
			GameObject gameObject              = new GameObject();
			gameObject.transform.localPosition = node.position;
			gameObject.transform.rotation      = node.orientation;
			gameObject.transform.localScale    = new Vector3 (width, length, parent.transform.localScale.z);
			gameObject.name                    = node.symbol;
			gameObject.transform.parent        = parent.transform;
			node.size = new Vector3();
			
			goList.Add(gameObject);
			foreach (LSystemGraph gNode in node.neighbour)
				createMesh (gNode, parent, goList);
			
			return;
		}
		}
		
		GameObject go = GameObject.Instantiate (prefab) as GameObject;
		go.transform.localPosition = node.position;
		go.transform.rotation      = node.orientation;
		go.transform.localScale    = new Vector3 (width, length, go.transform.localScale.z);
		go.name                    = node.symbol;
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
			
			j++;
		}
		node.size = boxMax - boxMin;
		
		goList.Add (go);
		foreach (LSystemGraph gNode in node.neighbour)
			createMesh (gNode, parent, goList);
	}
}
