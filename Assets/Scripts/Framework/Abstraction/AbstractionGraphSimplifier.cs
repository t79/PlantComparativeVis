using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Center
{
	public int id;
	public int graphId;
	public int posId;
	
	public int count;
	public Dictionary<int, int> connectionSize;
}

public class AbstractionGraphSimplifier : Abstraction
{
	public bool finalAbstraction = false;

	public  float scaleX = 0.5f;
	public  float scaleY = 0.5f;
	
	float minX =  999f;
	float maxX = -999f;

	float minY =  999f;
	float maxY = -999f;

	List<Vector3> cityCenters = new List<Vector3> ();
	List<Vector3> cityCenterLayout = new List<Vector3> ();
	List<int> cityCenterLayoutAngle = new List<int> ();
	List<int>     centerNodes = new List<int> ();
	int           cityCentersCount = -1;

	private Dictionary<int, Center> centers = new Dictionary<int, Center>();

	public AbstractionGraphSimplifier()
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

		result.texList   = copyTextures (repre.texList);

		cityCenters.Clear ();
		centerNodes.Clear ();
		cityCenterLayout.Clear ();
		centers.Clear ();
		cityCenterLayoutAngle.Clear ();
		
		for (int i = 0; i < representation.additionalInformation.Count; i++)
		{
			cityCenters.Add(representation.additionalInformation[i]);
		}
		cityCentersCount = cityCenters.Count;

		centerNodes = new List<int>(new int[cityCenters.Count]);
		for(int k = 0; k < centerNodes.Count; k++) centerNodes[k] = -1;

		foreach (KeyValuePair<int, GraphNode> node in repre.graph)
		{
			if(node.Value.centerID != -1)
			{
				centerNodes[node.Value.centerID] = node.Key;
				node.Value.distance = 0;
			}
		}

		for(int i = 0; i < centerNodes.Count; i++)
		{
			if(centerNodes[i] == -1)
			{
				int newID = GetNewNodeCenter(cityCenters[i], repre);
				centerNodes[i] = newID;
				repre.graph[newID].centerID = i;
				repre.graph[newID].distance = 0;
			}
		}

		processCityCenters (repre);
		LayoutCityCentersUniformly ();

		for(int i = 0; i < centerNodes.Count; i++)
		{	
			centers[repre.graph[centerNodes[i]].centerID] = new Center();
			centers[repre.graph[centerNodes[i]].centerID].id = repre.graph[centerNodes[i]].centerID;
			centers[repre.graph[centerNodes[i]].centerID].posId = i;
			centers[repre.graph[centerNodes[i]].centerID].graphId = centerNodes[i];
			centers[repre.graph[centerNodes[i]].centerID].count = 0;
			centers[repre.graph[centerNodes[i]].centerID].connectionSize = new Dictionary<int, int>();
		}

		foreach (KeyValuePair<int, Center> center in centers)
		{
			repre.graph[center.Value.graphId].distance = 0;
			repre.graph[center.Value.graphId].centerID = center.Value.id;
			
			ColorGraph(center.Value.id, repre.graph[center.Value.graphId]);
		}

		CheckColoring (repre.graph);
		
		FillCenterValues (repre);
		
		minX =  999f;
		maxX = -999f;

		minY =  999f;
		maxY = -999f;

		/*
		foreach (KeyValuePair<int, GraphNode> node in result.graph)
		{

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
		*/
		
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
				scaleX *= 0.9f;
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
				scaleY *= 0.9f;
			}
		}

		createMesh (result.gameObject, result.goList);
		//result.offset = new Vector3 (-(maxX + minX) / 2, -(maxY + minY) / 2, 0);
		result.offset = new Vector3 (0, 0, 0);
		
		return result;
	}

	void CheckColoring (Dictionary<int, GraphNode> nodes)
	{
		foreach (KeyValuePair<int, GraphNode> node in nodes)
		{
			if(node.Value.centerID == -1)
				node.Value.centerID = GetColorOfClosestColored(node.Value.position, nodes);
		}
	}

	int GetColorOfClosestColored(Vector3 position, Dictionary<int, GraphNode> nodes)
	{
		int   closestColor    = -1;
		float closestdistance = float.MaxValue;

		foreach (KeyValuePair<int, GraphNode> node in nodes)
		{
			if((node.Value.centerID != -1) && (closestdistance > (node.Value.position - position).magnitude))
			{
				closestColor = node.Value.centerID;
				closestdistance = (node.Value.position - position).magnitude;
			}
		}

		return closestColor;
	}

	void FillCenterValues (RepresentationMesh repre)
	{
		foreach (KeyValuePair<int, GraphNode> node in repre.graph)
		{
			if(!centers.ContainsKey(node.Value.centerID))
				Debug.Log(node.Value.centerID);
			
			centers[node.Value.centerID].count++;
			
			foreach(GraphNode neighbor in node.Value.neighbours)
			{
				if(node.Value.centerID != neighbor.centerID)
				{
					if(!centers[node.Value.centerID].connectionSize.ContainsKey(neighbor.centerID))
					{
						centers[node.Value.centerID].connectionSize[neighbor.centerID] = 1;
					}
					else
					{
						centers[node.Value.centerID].connectionSize[neighbor.centerID]++;
					}
				}
			}
		}
	}

	void processCityCenters(RepresentationMesh representation)
	{
		Vector2 closestCC = getClosestCC ();
		while(closestCC.x != -1)
		{
			int index1 = (int)closestCC.x;
			int index2 = (int)closestCC.y;
			
			Vector3 one = cityCenters[index1];
			Vector3 two = cityCenters[index2];
			
			Vector3 newCenter = Vector3.Lerp(one, two, 0.5f);
			
			int nodeCenterIndex1 = centerNodes[index1];
			int nodeCenterIndex2 = centerNodes[index2];
			
			int newNodeCenter = GetNewNodeCenter(newCenter, representation);
			
			cityCenters.Remove(one);
			cityCenters.Remove(two);
			cityCenters.Add(newCenter);
			
			centerNodes.Remove(nodeCenterIndex1);
			centerNodes.Remove(nodeCenterIndex2);
			centerNodes.Add(newNodeCenter);
			
			int center1 = representation.graph[nodeCenterIndex1].centerID;
			int center2 = representation.graph[nodeCenterIndex2].centerID;
			
			representation.graph[newNodeCenter].centerID = center1 == -1 ? center2 : center1;
			representation.graph[newNodeCenter].distance = 0;
			
			representation.graph[nodeCenterIndex1].centerID = -1;
			representation.graph[nodeCenterIndex2].centerID = -1;
			representation.graph[nodeCenterIndex1].distance = -1f;
			representation.graph[nodeCenterIndex2].distance = -1f;
			
			closestCC = getClosestCC ();
		}
	}

	int GetNewNodeCenter(Vector3 newCenter, RepresentationMesh representation)
	{
		int index = -1;
		float minDistance = float.MaxValue;
		
		foreach (KeyValuePair<int, GraphNode> node in representation.graph)
		{
			if((node.Value.position - newCenter).sqrMagnitude < minDistance)
			{
				minDistance = (node.Value.position - newCenter).sqrMagnitude;
				index = node.Key;
			}
		}
		
		return index;
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
		cityCenterLayoutAngle = new List<int> (new int[cityCenters.Count]);
		//float rotAngle = 360f / cityCenters.Count;
		float rotAngle = 360f / cityCentersCount;
		
		int a = 0;
		foreach (KeyValuePair<float, int> point in inCircN)
		{
			Vector3 nPos = new Vector3(-1f, 0, 0);
			
			nPos = Quaternion.AngleAxis(-rotAngle * a, new Vector3(0, 0, -1f)) * nPos;
			nPos = new Vector3 (0, 150f, 0)/*center*/ + nPos * 85f;
			
			cityCenterLayout[point.Value] = nPos;
			cityCenterLayoutAngle[point.Value] = a;
			a++;
		}
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


	void setVisitedFalse (Dictionary<int, GraphNode> graphNodes)
	{
		foreach (KeyValuePair<int, GraphNode> node in graphNodes)
		{
			node.Value.visited = false;
		}
	}

	void ColorGraph(int centerID, GraphNode node)
	{
		if (node.neighbours.Count == 0)
			return;
		
		foreach (GraphNode neighbour in node.neighbours)
		{
			float distance = node.distance + (node.position - neighbour.position).magnitude;
			
			if(distance < neighbour.distance || neighbour.distance == -1)
			{
				neighbour.centerID = centerID;
				neighbour.distance = distance;
				
				ColorGraph(centerID, neighbour);
			}
		}
	}

	void createMesh (GameObject parent, List<GameObject> goList)
	{
		Vector3 scaleVec = new Vector3 (scaleX, scaleY, 1f);

		foreach (KeyValuePair<int, Center> center in centers)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.name = cityCenterLayoutAngle[center.Value.posId].ToString();
			go.transform.parent     = parent.transform;
			go.transform.position   = Vector3.Scale(cityCenterLayout[center.Value.posId], scaleVec);
			go.transform.localScale = Vector3.Scale(new Vector3((float)center.Value.count / 7.5f, (float)center.Value.count / 7.5f, 0.01f), scaleVec);
			goList.Add(go);


			foreach(KeyValuePair<int, int> connection in center.Value.connectionSize)
			{
				string one = cityCenterLayoutAngle[center.Value.posId].ToString() + cityCenterLayoutAngle[centers[connection.Key].posId].ToString();
				string two = cityCenterLayoutAngle[centers[connection.Key].posId].ToString() + cityCenterLayoutAngle[center.Value.posId].ToString();
				if(parent.transform.Find(one) != null || parent.transform.Find(two) != null)
					continue;

				GameObject connectionGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
				connectionGO.transform.parent = parent.transform;
				connectionGO.name = cityCenterLayoutAngle[center.Value.posId].ToString() + cityCenterLayoutAngle[centers[connection.Key].posId].ToString();
				connectionGO.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1f, 0), (cityCenterLayout[centers[connection.Key].posId] - cityCenterLayout[center.Value.posId]).normalized );

				float centSize1 = (float)center.Value.count / 15f;
				float centSize2 = (float)centers[connection.Key].count / 15f;

				Vector3 direction = (cityCenterLayout[centers[connection.Key].posId] - cityCenterLayout[center.Value.posId]).normalized;

				Vector3 startvec1 = cityCenterLayout[center.Value.posId]            + direction * centSize1;
				Vector3 startvec2 = cityCenterLayout[centers[connection.Key].posId] - direction * centSize2;

				startvec1 = Vector3.Scale(startvec1, scaleVec);
				startvec2 = Vector3.Scale(startvec2, scaleVec);

				connectionGO.transform.position = Vector3.Lerp(startvec1, startvec2, 0.5f);
				connectionGO.transform.localScale = new Vector3((float)connection.Value * scaleX, (startvec1 - startvec2).magnitude + 0.5f, 1f);

				goList.Add(connectionGO);
			}
		}
	}
}
