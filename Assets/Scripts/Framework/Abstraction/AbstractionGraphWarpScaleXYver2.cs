using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionGraphWarpScaleXYver2 : Abstraction
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


	List<Vector3> cityCenters = new List<Vector3> ();
	List<Vector3> cityCenterLayout = new List<Vector3> ();

	public AbstractionGraphWarpScaleXYver2()
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
		
		result.dimension                       = 2;
		result.type                            = "mesh";
		result.gameObject                      = new GameObject (repre.gameObject.name);
		result.gameObject.transform.position   = repre.gameObject.transform.position;
		result.gameObject.transform.rotation   = repre.gameObject.transform.rotation;
		result.gameObject.transform.localScale = repre.gameObject.transform.localScale;

		result.graph     = copyGraph (repre.graph);
		result.texList   = copyTextures (repre.texList);

		processCityCenters (repre);
		LayoutCityCentersUniformly ();
		setWarpPoints (result.graph);
		
		minX =  999f;
		maxX = -999f;

		minY =  999f;
		maxY = -999f;
		
		foreach (KeyValuePair<int, GraphNode> node in result.graph)
		{
			Vector2 resPos = warpPosition(node.Key, new Vector2(node.Value.position.x, node.Value.position.y));

			node.Value.position = new Vector3(resPos.x, resPos.y, 0);
			
			if(resPos.x < minX)
			{
				minX = resPos.x;
			}
			
			if(resPos.x > maxX)
			{
				maxX = resPos.x;
			}
			
			if(resPos.y < minY)
			{
				minY = resPos.y;
			}
			
			if(resPos.y > maxY)
			{
				maxY = resPos.y;
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

			if(objectSize.x > 300f)
			{
				scaleX = objectSize.x / 300f;
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

			if(objectSize.y > 300f)
			{
				scaleY = objectSize.y / 300f;
			}
		}

		createMesh (result.graph, result.gameObject, result.goList);
		//result.offset = new Vector3 (-(maxX + minX) / 2, -(maxY + minY) / 2, 0);
		result.offset = new Vector3 (0, 0, 0);
		
		return result;
	}

	void processCityCenters(Representation representation)
	{
		cityCenters.Clear ();

		for (int i = 0; i < representation.additionalInformation.Count; i++)
		{
			cityCenters.Add(representation.additionalInformation[i]);
		}

		Vector2 closestCC = getClosestCC ();
		while(closestCC.x != -1)
		{
			int index1 = (int)closestCC.x;
			int index2 = (int)closestCC.y;
			
			Vector3 one = cityCenters[index1];
			Vector3 two = cityCenters[index2];
			
			Vector3 newCenter = Vector3.Lerp(one, two, 0.5f);
			
			cityCenters.Remove(one);
			cityCenters.Remove(two);
			cityCenters.Add(newCenter);
			
			closestCC = getClosestCC ();
		}
	}
	
	Vector2 getClosestCC ()
	{
		for (int i = 0; i < cityCenters.Count; i++)
		{
			for(int j = 0; j < cityCenters.Count; j++)
			{
				if(i==j)
				{
					continue;
				}
				
				if((cityCenters[i] - cityCenters[j]).magnitude < 80f)
				{
					return new Vector2(i, j);
				}
			}
		}
		
		return new Vector2 (-1f, -1f);
	}

	void LayoutCityCentersUniformly()
	{
		//Vector3 center = new Vector3 (0, 150f, 0);
		Vector3 center = new Vector3 ();
		
		for (int i = 0; i < cityCenters.Count; i++) {
			center+=cityCenters[i];
		}
		center /= cityCenters.Count;
		
		int mostLeft = -1;
		float mlValue = 999f;
		
		List<Vector3> inCirclePositions = new List<Vector3> ();
		SortedDictionary<float, int> inCircN = new SortedDictionary<float, int> ();
		
		for (int i = 0; i < cityCenters.Count; i++)
		{
			Vector3 circPos = center + (cityCenters[i] - center).normalized * 85f;
			inCirclePositions.Add(circPos);
			if(circPos.x < mlValue)
			{
				mlValue = circPos.x;
				mostLeft = i;
			}
		}
		
		for (int i = 0; i < cityCenters.Count; i++)
		{
			Vector3 beginning = (inCirclePositions[mostLeft] - center).normalized;
			int sign = Vector3.Cross(beginning, (inCirclePositions[i] - center)).z < 0 ? -1 : 1;
			int addition = sign < 0 ? 360 : 0;
			float angle = addition + sign * Vector3.Angle(beginning, (inCirclePositions[i] - center).normalized);
			angle = angle % 360;
			
			inCircN[angle] = i;
		}
		
		cityCenterLayout = new List<Vector3> (new Vector3[cityCenters.Count]);
		float rotAngle = 360f / cityCenters.Count;
		
		int a = 0;
		Gizmos.color = Color.blue;
		foreach (KeyValuePair<float, int> point in inCircN)
		{
			Vector3 nPos = new Vector3(-1f, 0, 0);
			
			nPos = Quaternion.AngleAxis(-rotAngle * a, new Vector3(0, 0, -1f)) * nPos;
			nPos = new Vector3 (0, 150f, 0)/*center*/ + nPos * 85f;
			
			cityCenterLayout[point.Value] = nPos;
			a++;
		}
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

	void setWarpPoints (Dictionary<int, GraphNode> graph)
	{
		controlPoints.Clear ();
		warpingPoints.Clear ();

		for (int i = 0; i < cityCenters.Count; i++)
		{
			controlPoints.Add(new Vector2(cityCenters[i].x, cityCenters[i].y));
			warpingPoints.Add(new Vector2(cityCenterLayout[i].x,    cityCenterLayout[i].y));
		}

		if(cityCenters.Count == 1)
		{
			controlPoints.Add (new Vector2 (-150f, 0f  ));
			warpingPoints.Add (new Vector2 (-150f, 0f  ));
		}
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

		res.x = res.x > 150f ? 150f : res.x;
		res.x = res.x <-150f ?-150f : res.x;
		
		res.y = res.y > 300f ? 300f : res.y;
		res.y = res.y <    0 ?    0 : res.y;
		
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

	void createMesh (Dictionary<int, GraphNode> nodes, GameObject parent, List<GameObject> goList)
	{
		List<Vector3> vertices = new List<Vector3> ();
		List<int>     indices  = new List<int> ();

		foreach (KeyValuePair<int, GraphNode> node in nodes)
		{
			Vector3 nodePos = node.Value.position;
			nodePos.x *= scaleX;
			nodePos.y *= scaleY;
			node.Value.position = nodePos;
		}

		foreach (KeyValuePair<int, GraphNode> node in nodes)
		{
			for(int i = 0; i < node.Value.neighbours.Count; i++)
			{
				int index = vertices.Count;
				Vector3 startPoint = node.Value.position;
				Vector3 endPoint   = node.Value.neighbours[i].position;

				Vector3 direction = (endPoint - startPoint).normalized;
				Vector3 normal    = new Vector3(0, 0, -1f);
				Vector3 binormal  = Vector3.Cross(direction, normal);

				Vector3 scale = new Vector3(scaleX, scaleY, 1f);

				Vector3 v0 = startPoint + Vector3.Scale(binormal, scale);
				Vector3 v1 = startPoint - Vector3.Scale(binormal, scale);
				Vector3 v2 = endPoint   + Vector3.Scale(binormal, scale);
				Vector3 v3 = endPoint   - Vector3.Scale(binormal, scale);

				/*
				v0.x *= scaleX; v0.y *= scaleY;
				v1.x *= scaleX; v1.y *= scaleY;
				v2.x *= scaleX; v2.y *= scaleY;
				v3.x *= scaleX; v3.y *= scaleY;
				*/

				vertices.Add(v0);vertices.Add(v1);vertices.Add(v2);vertices.Add(v3);
				indices.Add(index + 0);
				indices.Add(index + 1);
				indices.Add(index + 2);
				indices.Add(index + 0);
				indices.Add(index + 2);
				indices.Add(index + 3);
			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = indices.ToArray ();

		//mesh.RecalculateNormals ();

		GameObject go = new GameObject ();
		go.transform.parent = parent.transform;
		go.AddComponent<MeshFilter> ();
		go.GetComponent<MeshFilter> ().mesh = mesh;
		go.AddComponent<MeshRenderer> ();
		go.GetComponent<MeshRenderer> ().material = Resources.Load<Material>("Materials/SegmentMaterial");

		goList.Add (go);
	}

	void ColorGraph(int centerID, GraphNode node)
	{
		if (node.neighbours.Count == 0)
			return;

		foreach (GraphNode neighbour in node.neighbours)
		{
			float distance = node.distance + (node.position - neighbour.position).magnitude;

			if(distance < neighbour.distance)
			{
				neighbour.centerID = centerID;
				neighbour.distance = distance;

				ColorGraph(centerID, neighbour);
			}
		}
	}
}
