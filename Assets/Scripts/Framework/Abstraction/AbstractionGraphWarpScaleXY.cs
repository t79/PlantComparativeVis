using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionGraphWarpScaleXY : Abstraction
{
	public bool finalAbstraction = false;

	public  float scaleX = 0.5f;
	public  float scaleY = 0.5f;
	
	float minX =  999f;
	float maxX = -999f;

	float minY =  999f;
	float maxY = -999f;

	private List<Vector2>            controlPoints     = new List<Vector2>();
	private List<Vector2>            warpingPoints     = new List<Vector2>();
	private Dictionary<int, Vector2> deformedPositions = new Dictionary<int, Vector2>();
	private bool                     toRotate          = false;

	List<Vector3> tempMergingVert   = new List<Vector3> ();
	List<Vector2> tempMergingUV     = new List<Vector2> ();
	List<Color>   tempMergingColor  = new List<Color> ();
	List<int>     tempMergingTriang = new List<int>();
	

	List<Vector3> tempPathVert   = new List<Vector3> ();
	List<Vector2> tempPathUV     = new List<Vector2> ();
	List<int>     tempPathTriang = new List<int>();

	public AbstractionGraphWarpScaleXY()
	{
		dimension = new Vector3 (0.5f, 0.5f, 0f);

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

		minY =  999f;
		maxY = -999f;
		
		foreach (KeyValuePair<int, GraphNode> node in result.graph)
		{
			Vector2 resPos = warpPosition(node.Key, new Vector2(node.Value.position.x, node.Value.position.y));
			Vector2 rotResPos = quat * resPos;
			deformedPositions[node.Key] = rotResPos;
			
			if(rotResPos.x < minX)
			{
				minX = rotResPos.x;
			}
			
			if(rotResPos.x > maxX)
			{
				maxX = rotResPos.x;
			}

			if(rotResPos.y < minY)
			{
				minY = rotResPos.y;
			}
			
			if(rotResPos.y > maxY)
			{
				maxY = rotResPos.y;
			}
		}
		
		setVisitedFalse (result.graph);
		
		if (objectSize.x != 0)
		{
			if (objectSize.x < ((maxX - minX) * 0.5f))
			{
				scaleX = objectSize.x / (maxX - minX);
			}
			else
			{
				scaleX = 0.5f;
			}
		}

		if (objectSize.y != 0)
		{
			if (objectSize.y < ((maxY - minY) * 0.5f))
			{
				scaleY = objectSize.y / (maxY - minY);
			}
			else
			{
				scaleY = 0.5f;
			}
		}
		
		result.structure = updateStructureGraph (result.graph[0], new LSystemGraph());
		result.structure = result.structure.neighbour [0];

		if (finalAbstraction)
		{
			maxX = -999;
			minX =  999;
			maxY = -999;
			minY =  999;

			CreateOptimizedMeshStructure(result.structure as LSystemGraph, result.gameObject, result.goList);

			GameObject innerGO = new GameObject("inner");
			innerGO.AddComponent<MeshFilter>();
			innerGO.AddComponent<MeshRenderer>();
			
			Mesh innerMesh = new Mesh();

			Vector3[] vertices  = new Vector3[tempMergingVert.Count];
			Vector2[] uv        = new Vector2[tempMergingUV.Count];
			Color[]   colors    = new Color[tempMergingColor.Count];
			int[]     triangles = new int[tempMergingTriang.Count];

			tempMergingVert.CopyTo(vertices);
			tempMergingUV.CopyTo(uv);
			tempMergingColor.CopyTo(colors);
			tempMergingTriang.CopyTo(triangles);

			innerMesh.vertices  = vertices;
			innerMesh.uv        = uv;
			innerMesh.colors    = colors;
			innerMesh.triangles = triangles;

			innerMesh.RecalculateNormals();
			innerMesh.RecalculateBounds();
			
			innerGO.GetComponent<MeshFilter>().mesh = innerMesh;

			MeshRenderer innerMR = innerGO.GetComponent<MeshRenderer>();
			innerMR.material = new Material(Shader.Find("Sprites/Default"));

			innerGO.transform.parent = result.gameObject.transform;
			result.goList.Add(innerGO);

			//////////////////////////////   PATH   ////////////////////////////////////

			GameObject path = new GameObject("Path");
			path.AddComponent<MeshFilter>();
			path.AddComponent<MeshRenderer>();
			
			Mesh pathMesh = new Mesh();
			
			Vector3[] pathVertices  = new Vector3[tempPathVert.Count];
			Vector2[] pathUV        = new Vector2[tempPathUV.Count];
			int[]     pathTriangles = new int[tempPathTriang.Count];
			
			tempPathVert.CopyTo(pathVertices);
			tempPathUV.CopyTo(pathUV);
			tempPathTriang.CopyTo(pathTriangles);
			
			pathMesh.vertices  = pathVertices;
			pathMesh.uv        = pathUV;
			pathMesh.triangles = pathTriangles;
			
			pathMesh.RecalculateNormals();
			pathMesh.RecalculateBounds();
			
			path.GetComponent<MeshFilter>().mesh = pathMesh;
			
			MeshRenderer pathMR = path.GetComponent<MeshRenderer>();
			Material pathMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			pathMaterial.color = new Color(234f/255f, 162f/255f, 96f/255f);
			pathMR.material = pathMaterial;
			
			path.transform.parent = result.gameObject.transform;
			path.transform.position += new Vector3(0, 0, -5f);

			//////////////////////////////   PATH SHADOW   ////////////////////////////////////

			GameObject pathShadow = new GameObject("PathShadow");

			pathShadow.AddComponent<MeshFilter>();
			pathShadow.AddComponent<MeshRenderer>();
			
			Mesh pathShadowMesh = new Mesh();
			
			Vector3[] pathShadowVertices  = new Vector3[tempPathVert.Count];
			Vector2[] pathShadowUV        = new Vector2[tempPathUV.Count];
			int[]     pathShadowTriangles = new int[tempPathTriang.Count];
			
			tempPathVert.CopyTo(pathShadowVertices);
			tempPathUV.CopyTo(pathShadowUV);
			tempPathTriang.CopyTo(pathShadowTriangles);
			
			pathShadowMesh.vertices  = pathShadowVertices;
			pathShadowMesh.uv        = pathShadowUV;
			pathShadowMesh.triangles = pathShadowTriangles;
			
			pathShadowMesh.RecalculateNormals();
			pathShadowMesh.RecalculateBounds();
			
			pathShadow.GetComponent<MeshFilter>().mesh = pathShadowMesh;
			
			MeshRenderer pathSMR = pathShadow.GetComponent<MeshRenderer>();
			Material pathSMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			pathSMaterial.color = Color.black;
			pathSMR.material = pathSMaterial;

			pathShadow.transform.parent = result.gameObject.transform;
			pathShadow.transform.localScale = Vector3.Scale( pathShadow.transform.localScale, new Vector3(1f, 1.5f, 1f));
			pathShadow.transform.position += new Vector3(0, 0, -4.7f);

			if(maxX < innerMR.bounds.max.x)
				maxX = innerMR.bounds.max.x;

			if(maxY < innerMR.bounds.max.y)
				maxY = innerMR.bounds.max.y;

			if(minX > innerMR.bounds.min.x)
				minX = innerMR.bounds.min.x;
			
			if(minY > innerMR.bounds.min.y)
				minY = innerMR.bounds.min.y;

			if(maxX < pathSMR.bounds.max.x)
				maxX = pathSMR.bounds.max.x;
			
			if(maxY < pathSMR.bounds.max.y)
				maxY = pathSMR.bounds.max.y;
			
			if(minX > pathSMR.bounds.min.x)
				minX = pathSMR.bounds.min.x;
			
			if(minY > pathSMR.bounds.min.y)
				minY = pathSMR.bounds.min.y;

			tempMergingVert.Clear();
			tempMergingUV.Clear();
			tempMergingTriang.Clear();
			tempMergingColor.Clear();

			tempPathVert.Clear();
			tempPathUV.Clear();
			tempPathTriang.Clear();

			result.size = new Vector3(maxX - minX, maxY - minY, 0);
		}
		else
		{
			createMesh (result.structure as LSystemGraph, result.gameObject, result.goList);
		}

		result.offset = new Vector3 (-(maxX + minX) / 2, -(maxY + minY) / 2, 0);
		
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
				node.Value.partOfShort = false;
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
			gp.partOfShort = true;
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

			toRotate = false;
		}
		else
		{
			float xCoord = 0;
			
			foreach(GraphNode node in nearestWayList)
			{
				controlPoints.Add(new Vector2(node.position.x, node.position.y));
				warpingPoints.Add(new Vector2(xCoord, node.distance - distance / 2f));
			}

			toRotate = true;
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
		node.position = new Vector3 (deformedPositions [node.id].x * scaleX, deformedPositions [node.id].y  * scaleY);
		
		if(node.position.x < minX)
		{
			minX = node.position.x;
		}
		
		if(node.position.x > maxX)
		{
			maxX = node.position.x;
		}

		if(node.position.y < minY)
		{
			minY = node.position.y;
		}
		
		if(node.position.y > maxY)
		{
			maxY = node.position.y;
		}
		
		foreach (GraphNode neighbour in node.neighbours)
		{
			LSystemGraph nNode = new LSystemGraph ();
			nNode.id           = node.id;
			nNode.symbol       = "R";
			
			Vector3 gNodePos = new Vector3 (deformedPositions [node.id].x,      deformedPositions [node.id].y);
			Vector3 gNeigPos = new Vector3 (deformedPositions [neighbour.id].x, deformedPositions [neighbour.id].y);
			
			gNodePos.x *= scaleX;
			gNeigPos.x *= scaleX;

			gNodePos.y *= scaleY;
			gNeigPos.y *= scaleY;
			
			nNode.position     = gNodePos;
			
			Vector3 nodeChildVec = (gNeigPos - gNodePos);
			
			nNode.orientation = Quaternion.FromToRotation(Vector3.up, nodeChildVec.normalized);
			//nNode.width  = scaleX;
			//nNode.width  = 0.25f;
			//nNode.width  = 0.5f;
			//nNode.width  = 1f;
			nNode.width  = 2f;
			nNode.length = nodeChildVec.magnitude / 4f;

			nNode.partOfShort = (node.partOfShort && neighbour.partOfShort);
			
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

	void CreateOptimizedMeshStructure (LSystemGraph node, GameObject parent, List<GameObject> goList)
	{
		if (node == null)
			return;

		float length = 1f;
		float width  = 1f;

		if (node.symbol == "R")
		{
			GameObject prefabGO = Resources.Load("2DSegment") as GameObject;
			length = node.length / 2f;
			width = node.width;

			Mesh       mesh             = Mesh.Instantiate(prefabGO.GetComponent<MeshFilter>().sharedMesh) as Mesh;
			Vector3[]  vertices         = mesh.vertices;
			int[]      triangles        = mesh.triangles;
			int        triangleOffset   = tempMergingVert.Count;
			int        pathTriangOffset = tempPathVert.Count;

			if((tempMergingVert.Count + vertices.Length) > 65000)
			{
				GameObject innerGO = new GameObject("inner");
				innerGO.AddComponent<MeshFilter>();
				innerGO.AddComponent<MeshRenderer>();
				
				Mesh innerMesh = new Mesh();
				
				Vector3[] nvertices  = new Vector3[tempMergingVert.Count];
				Vector2[] nuv        = new Vector2[tempMergingUV.Count];
				Color[]   ncolors    = new Color[tempMergingColor.Count];
				int[]     ntriangles = new int[tempMergingTriang.Count];
				
				tempMergingVert.CopyTo(nvertices);
				tempMergingUV.CopyTo(nuv);
				tempMergingColor.CopyTo(ncolors);
				tempMergingTriang.CopyTo(ntriangles);
				
				innerMesh.vertices  = nvertices;
				innerMesh.uv        = nuv;
				innerMesh.colors    = ncolors;
				innerMesh.triangles = ntriangles;
				
				innerMesh.RecalculateNormals();
				innerMesh.RecalculateBounds();
				
				innerGO.GetComponent<MeshFilter>().mesh = innerMesh;
				
				MeshRenderer innerMR = innerGO.GetComponent<MeshRenderer>();
				innerMR.material = new Material(Shader.Find("Sprites/Default"));
				
				innerGO.transform.parent = parent.transform;
				goList.Add(innerGO);

				if(maxX < innerMR.bounds.max.x)
					maxX = innerMR.bounds.max.x;
				
				if(maxY < innerMR.bounds.max.y)
					maxY = innerMR.bounds.max.y;
				
				if(minX > innerMR.bounds.min.x)
					minX = innerMR.bounds.min.x;
				
				if(minY < innerMR.bounds.min.y)
					minY = innerMR.bounds.min.y;

				tempMergingVert.Clear();
				tempMergingUV.Clear();
				tempMergingTriang.Clear();
				tempMergingColor.Clear();
			}

			int j = 0;
			while (j < vertices.Length)
			{
				Vector3 pos = node.orientation * Vector3.Scale(vertices[j], new Vector3 (width, length, parent.transform.localScale.z)) + node.position;

				if(!node.partOfShort)
				{
					tempMergingVert.Add(pos);
					tempMergingUV.Add (new Vector2());
					tempMergingColor.Add(new Color(26f/256f, 161f/256f, 161f/256f));
				}
				else
				{
					tempPathVert.Add(pos);
					tempPathUV.Add(new Vector2());
				}
				j++;
			}


			int k = 0;

			while (k < mesh.triangles.Length)
			{
				if(!node.partOfShort)
				{
					tempMergingTriang.Add(triangles[k] + triangleOffset);
				}
				else
				{
					tempPathTriang.Add(triangles[k] + pathTriangOffset);
				}
				k++;
			}
		}

		foreach (LSystemGraph gNode in node.neighbour)
			CreateOptimizedMeshStructure (gNode, parent, goList);
	}
}
