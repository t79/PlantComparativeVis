using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public static class TestLoaders
{
	public static Structure LoadIvy(int id, int dimension, string type, XmlNode structureXml)
	{
		Structure structure = new Structure ();
		RepresentationMesh representation = new RepresentationMesh (dimension);

		structure.name = "Ivy_" + id;
		representation.gameObject = new GameObject(structure.name);

		int priority = 1;
		XmlNode structureParametersXml = structureXml.FirstChild;
		foreach (XmlNode parameterXml in structureParametersXml)
		{
			structure.parameters[parameterXml.Attributes ["name"].Value] = new StructureParameter (System.Convert.ToSingle (parameterXml.Attributes ["value"].Value), priority);
			priority++;
		}
		
		// structure state
		XmlNode stringNode = structureXml.LastChild.FirstChild;
		string stringValue = stringNode.InnerText.Trim();
		
		string[] stringPairs = stringValue.Split(new char[] { '[', ']' }, System.StringSplitOptions.RemoveEmptyEntries);
		List<KeyValuePair<string, string>> symbolsStrings = new List<KeyValuePair<string, string>>();
		
		int index = 0;
		while(index < stringPairs.Length)
		{
			if(stringPairs[index].Length == 1)
			{
				if(stringPairs[index] == "E")
				{
					symbolsStrings.Add(new KeyValuePair<string, string>("E", ""));
					index++;
					continue;
				}
				else
				{
					symbolsStrings.Add(new KeyValuePair<string, string>(stringPairs[index], stringPairs[index + 1]));
					index+=2;
					continue;
				}
			}
			else
			{
				index++;
			}
		}


		int  symbolId    = 0;
		bool isBranching = false;

		Interpreter        current = new Interpreter (Quaternion.identity, representation.gameObject.transform.position);
		Stack<Interpreter> stack   = new Stack<Interpreter> ();
		stack.Push(current);

		LSystemGraph        node      = new LSystemGraph();
		node.neighbour                = new List<Graph> ();
		Stack<LSystemGraph> nodeStack = new Stack<LSystemGraph>();
		representation.structure      = node;
		nodeStack.Push(node);


		foreach(KeyValuePair<string, string> symbolStrings in symbolsStrings)
		{
			Dictionary<string, float> structureParameters = new Dictionary<string, float> ();

			string[] parameters = symbolStrings.Value.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
			
			foreach(string parameterStr in parameters)
			{
				string[] keyValue = parameterStr.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
				
				if(keyValue.Length != 2)
					continue;
				
				structureParameters[keyValue[0].Trim()] = System.Convert.ToSingle(keyValue[1]);
			}

			switch(symbolStrings.Key)
			{
			case "S":
			{
				current.direction = current.direction * Quaternion.AngleAxis(structureParameters["angle"], Vector3.forward);

				LSystemGraph tempNode    = new LSystemGraph();
				tempNode.id              = symbolId;
				tempNode.symbol          = symbolStrings.Key;
				tempNode.position        = current.position;
				tempNode.orientation     = current.direction;
				tempNode.angle           = structureParameters["angle"];
				tempNode.length          = structureParameters["length"];
				tempNode.isBranchingNode = isBranching;
				tempNode.parent          = node;

				node.neighbour.Add(tempNode);

				node = tempNode;
				
				current.position = current.position + current.direction * (4.0f * structureParameters["length"] * Vector3.up);
				isBranching      = false;
				symbolId++;
				break;
			}	
			case "B":
			{
				stack.Push(current);
				current = new Interpreter (current);

				nodeStack.Push(node);
				isBranching = true;
				break;
			}	
			case "L":
			{
				Interpreter tempInter = new Interpreter(current.direction * Quaternion.AngleAxis(structureParameters["angle"], Vector3.forward), current.position);

				LSystemGraph tempNode    = new LSystemGraph();
				tempNode.id              = symbolId;
				tempNode.symbol          = symbolStrings.Key;
				tempNode.position        = tempInter.position;
				tempNode.orientation     = tempInter.direction;
				tempNode.angle           = structureParameters["angle"];
				tempNode.length          = structureParameters["length"];
				tempNode.isBranchingNode = true;
				tempNode.parent          = node;
				
				node.neighbour.Add(tempNode);
				symbolId++;
				break;
			}
			case "T":
			{
				LSystemGraph tempNode = new LSystemGraph();
				tempNode.id           = symbolId;
				tempNode.symbol       = symbolStrings.Key;
				tempNode.position     = current.position;
				tempNode.orientation  = current.direction;
				tempNode.angle        = structureParameters["angle"];
				tempNode.parent       = node;
				
				node.neighbour.Add(tempNode);
				
				current = stack.Pop();
				node    = nodeStack.Pop();
				symbolId++;
				break;
			}
			case "E":
				current = stack.Pop();
				node    = nodeStack.Pop();
				break;
				
			default:
				break;
			}

		}
		representation.structure = representation.structure.neighbour [0];

		putIvyMesh (representation.structure, representation.gameObject, representation.goList);

		representation.gameObject.SetActive (false);

		structure.representation = representation;

		Vector3 minPosSize = new Vector3 ();
		Vector3 maxPosSize = new Vector3 ();

		for (int i = 0; i < representation.gameObject.transform.childCount; i++)
		{
			GameObject childGO = representation.gameObject.transform.GetChild(i).gameObject;

			Bounds childBounds = childGO.GetComponent<Renderer> ().bounds;

			if( childBounds.max.x > maxPosSize.x)
			{
				maxPosSize.x = childBounds.max.x;
			}
			else if (childBounds.min.x < minPosSize.x)
			{
				minPosSize.x = childBounds.min.x;
			}

			if( childBounds.max.y > maxPosSize.y)
			{
				maxPosSize.y = childBounds.max.y;
			}
			else if (childBounds.min.y < minPosSize.y)
			{
				minPosSize.y = childBounds.min.y;
			}

			if( childBounds.max.z > maxPosSize.z)
			{
				maxPosSize.z = childBounds.max.z;
			}
			else if (childBounds.min.z < minPosSize.z)
			{
				minPosSize.z = childBounds.min.z;
			}
		}
		structure.representation.size = maxPosSize - minPosSize;

		return structure;
	}

	static void putIvyMesh (Graph node, GameObject parent, List<GameObject> goList)
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
					putIvyMesh (gNode, parent, goList);
				
				return;
			}
		}
		
		GameObject go = GameObject.Instantiate (prefab) as GameObject;
		go.transform.localPosition = lNode.position;
		go.transform.rotation      = lNode.orientation;
		go.transform.localScale    = new Vector3 (width, length, go.transform.localScale.z);
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
			
			j++;
		}
		
		lNode.size = boxMax - boxMin;

		goList.Add (go);
		foreach (Graph gNode in node.neighbour)
			putIvyMesh (gNode, parent, goList);
	}

	public static Structure LoadParp(int id, int dimension, string type, XmlNode structureXml)
	{
		Structure structure = new Structure ();
		RepresentationMesh representation = new RepresentationMesh (dimension);
		
		structure.name = "Parp_" + id;
		representation.gameObject = new GameObject(structure.name);
		
		int priority = 1;
		XmlNode structureParametersXml = structureXml.FirstChild;
		foreach (XmlNode parameterXml in structureParametersXml)
		{
			structure.parameters[parameterXml.Attributes ["name"].Value] = new StructureParameter (System.Convert.ToSingle (parameterXml.Attributes ["value"].Value), priority);
			priority++;
		}
		
		// structure state
		XmlNode stringNode = structureXml.LastChild.FirstChild;
		string stringValue = stringNode.InnerText.Trim();
		
		string[] stringPairs = stringValue.Split(new char[] { '[', ']' }, System.StringSplitOptions.RemoveEmptyEntries);
		List<KeyValuePair<string, string>> symbolsStrings = new List<KeyValuePair<string, string>>();
		
		int index = 0;
		while(index < stringPairs.Length)
		{
			if(stringPairs[index].Length == 1)
			{
				if(stringPairs[index] == "e")
				{
					symbolsStrings.Add(new KeyValuePair<string, string>("e", ""));
					index++;
					continue;
				}
				else
				{
					symbolsStrings.Add(new KeyValuePair<string, string>(stringPairs[index], stringPairs[index + 1]));
					index+=2;
					continue;
				}
			}
			else
			{
				index++;
			}
		}
		
		
		int  symbolId    = 0;
		bool isBranching = false;
		
		Interpreter        current = new Interpreter (Quaternion.identity, representation.gameObject.transform.position);
		Stack<Interpreter> stack   = new Stack<Interpreter> ();
		stack.Push(current);
		
		LSystemGraph        node      = new LSystemGraph();
		node.neighbour                = new List<Graph> ();
		Stack<LSystemGraph> nodeStack = new Stack<LSystemGraph>();
		representation.structure      = node;
		nodeStack.Push(node);
		
		
		foreach(KeyValuePair<string, string> symbolStrings in symbolsStrings)
		{
			Dictionary<string, float> structureParameters = new Dictionary<string, float> ();
			
			string[] parameters = symbolStrings.Value.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
			
			foreach(string parameterStr in parameters)
			{
				string[] keyValue = parameterStr.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
				
				if(keyValue.Length != 2)
					continue;
				
				structureParameters[keyValue[0].Trim()] = System.Convert.ToSingle(keyValue[1]);
			}
			
			switch(symbolStrings.Key)
			{
			case "m":
			{
				Vector3 monomerAngles   = new Vector3(structureParameters["angle_x"], structureParameters["angle_y"], structureParameters["angle_z"]);
				Vector3 monomerPosition = new Vector3(structureParameters["pos_x"], structureParameters["pos_y"], structureParameters["pos_z"]);

				if(!isBranching)
				{
					current.direction = current.direction * Quaternion.Euler(monomerAngles);
				}
				
				LSystemGraph tempNode    = new LSystemGraph();
				tempNode.id              = symbolId;
				tempNode.symbol          = symbolStrings.Key;
				tempNode.position        = current.position;
				tempNode.orientation     = current.direction;
				tempNode.spaceAngle      = monomerAngles;
				tempNode.spacePos        = monomerPosition;

				tempNode.isBranchingNode = isBranching;
				tempNode.parent          = node;
				
				node.neighbour.Add(tempNode);
				
				node = tempNode;
				
				current.position = current.position + current.direction * monomerPosition;
				isBranching      = false;
				symbolId++;
				break;
			}	
			case "b":
			{
				stack.Push(current);
				current = new Interpreter (current);

				Vector3 branchAngles   = new Vector3(structureParameters["angle_x"], structureParameters["angle_y"], structureParameters["angle_z"]);
				Vector3 branchPosition = new Vector3(structureParameters["pos_x"], structureParameters["pos_y"], structureParameters["pos_z"]);
				
				current.position = current.position + current.direction * branchPosition;
				current.direction = current.direction * Quaternion.Euler(branchAngles);

				nodeStack.Push(node);
				isBranching = true;
				break;
			}
			case "G":
			case "e":
			{
				current = stack.Pop();
				node    = nodeStack.Pop();
				break;
			}
				
			default:
				break;
			}
			
		}
		representation.structure = representation.structure.neighbour [0];
		
		putParpMesh (representation.structure, representation.gameObject, representation.goList, id);
		
		representation.gameObject.SetActive (false);
		
		structure.representation = representation;
		
		Vector3 minPosSize = new Vector3 ();
		Vector3 maxPosSize = new Vector3 ();
		
		for (int i = 0; i < representation.gameObject.transform.childCount; i++)
		{
			GameObject childGO = representation.gameObject.transform.GetChild(i).gameObject;
			
			Bounds childBounds = childGO.GetComponent<Renderer> ().bounds;
			
			if( childBounds.max.x > maxPosSize.x)
			{
				maxPosSize.x = childBounds.max.x;
			}
			else if (childBounds.min.x < minPosSize.x)
			{
				minPosSize.x = childBounds.min.x;
			}
			
			if( childBounds.max.y > maxPosSize.y)
			{
				maxPosSize.y = childBounds.max.y;
			}
			else if (childBounds.min.y < minPosSize.y)
			{
				minPosSize.y = childBounds.min.y;
			}
			
			if( childBounds.max.z > maxPosSize.z)
			{
				maxPosSize.z = childBounds.max.z;
			}
			else if (childBounds.min.z < minPosSize.z)
			{
				minPosSize.z = childBounds.min.z;
			}
		}
		structure.representation.size = maxPosSize - minPosSize;
		
		return structure;
	}

	static void putParpMesh (Graph node, GameObject parent, List<GameObject> goList, int id)
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
				putParpMesh (gNode, parent, goList, id);
			
			return;
		}
		}
		
		GameObject go = GameObject.Instantiate (prefab) as GameObject;
		go.transform.localPosition = lNode.position;
		go.transform.rotation      = lNode.orientation;
		go.name                    = lNode.symbol;
		go.transform.parent        = parent.transform;

		//lNode.size = go.GetComponent<Renderer> ().bounds.size;
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

		lNode.size = boxMax - boxMin;

		goList.Add (go);
		foreach (Graph gNode in node.neighbour)
			putParpMesh (gNode, parent, goList, id);
	}

	public static Structure LoadCity2(int id, int dimension, string type, XmlNode structureXml)
	{
		Structure structure = new Structure ();
		RepresentationMesh representation = new RepresentationMesh (dimension);
		
		structure.name = "City_" + id;
		representation.gameObject = new GameObject(structure.name);
		
		int priority = 1;
		XmlNode structureParametersXml = structureXml.FirstChild;
		foreach (XmlNode parameterXml in structureParametersXml)
		{
			structure.parameters[parameterXml.Attributes ["name"].Value] = new StructureParameter (System.Convert.ToSingle (parameterXml.Attributes ["value"].Value), priority);
			priority++;
		}
		
		// city centers
		XmlNode centerNode = structureXml.FirstChild.NextSibling.FirstChild.FirstChild;
		
		while(centerNode != null)
		{
			string[] centerPosStr = centerNode.Attributes["position"].Value.Split(',');
			representation.additionalInformation.Add(new Vector3(float.Parse(centerPosStr[0]), float.Parse(centerPosStr[1]), 0));
			
			centerNode = centerNode.NextSibling;
		}

		XmlNode nodesNode = structureXml.FirstChild.NextSibling.FirstChild.NextSibling;
		string[] nodesSTR = nodesNode.Attributes["info"].Value.Split(new char[] {';'}, System.StringSplitOptions.RemoveEmptyEntries);
		
		for(int j = 0; j < nodesSTR.Length; j++)
		{
			string[] nodeParamsSTR = nodesSTR[j].Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);
			
			int nodeId = int.Parse(nodeParamsSTR[0]);

			representation.graph[nodeId] = new GraphNode();
			representation.graph[nodeId].id = int.Parse(nodeParamsSTR[0]);
			representation.graph[nodeId].symbol = nodeParamsSTR[1];
			representation.graph[nodeId].position = new Vector3();
			representation.graph[nodeId].position.x = float.Parse(nodeParamsSTR[2]);
			representation.graph[nodeId].position.y = float.Parse(nodeParamsSTR[3]);
			representation.graph[nodeId].centerID = int.Parse(nodeParamsSTR[4]);
			representation.graph[nodeId].distance = -1;
			representation.graph[nodeId].neighbours = new List<GraphNode>();

			if(representation.graph[nodeId].centerID != -1)
			{
				representation.graph[nodeId].distance = 0;
			}
		}
		
		XmlNode edgesNode = nodesNode.NextSibling;
		string[] edgesSTR = edgesNode.Attributes["info"].Value.Split(new char[] {';'}, System.StringSplitOptions.RemoveEmptyEntries);
		
		for(int j = 0; j < edgesSTR.Length; j++)
		{
			string[] edgeParamsSTR = edgesSTR[j].Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);
			
			int index1 = int.Parse(edgeParamsSTR[0]);
			int index2 = int.Parse(edgeParamsSTR[1]);
			
			if(!representation.graph[index1].neighbours.Contains(representation.graph[index2]))
			{
				representation.graph[index1].neighbours.Add(representation.graph[index2]);
				representation.graph[index2].neighbours.Add(representation.graph[index1]);
			}
		}
		
		// city density image
		XmlNode imageDenNode = edgesNode.NextSibling.NextSibling;
		string  imageDenStr  = imageDenNode.Attributes["src"].Value;
		
		string directoryStr = "Ensemble.city";
		
		if (File.Exists(directoryStr + @"\" + imageDenStr + ".png"))
		{
			byte[] fileData;
			fileData = File.ReadAllBytes(directoryStr + @"\" + imageDenStr + ".png");
			
			Texture2D imgTemp  = new Texture2D(2, 2);
			imgTemp.filterMode = FilterMode.Point;
			imgTemp.wrapMode   = TextureWrapMode.Clamp;
			
			imgTemp.LoadImage(fileData);
			
			representation.texList["density"] = imgTemp;
		}

		createCityMesh (representation.graph, representation.gameObject, representation.goList);
		
		representation.gameObject.SetActive (false);
		
		structure.representation = representation;

		Vector3 minPosSize = new Vector3 ();
		Vector3 maxPosSize = new Vector3 ();

		Bounds childBounds = representation.gameObject.transform.GetChild (0).gameObject.GetComponent<Renderer> ().bounds;

		minPosSize = childBounds.min;
		maxPosSize = childBounds.max;

		structure.representation.size = maxPosSize - minPosSize;
		
		return structure;
	}

	static void createCityMesh (Dictionary<int, GraphNode> nodes, GameObject parent, List<GameObject> goList)
	{
		List<Vector3> vertices = new List<Vector3> ();
		List<int>     indices  = new List<int> ();

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

				Vector3 v0 = startPoint + binormal;
				Vector3 v1 = startPoint - binormal;
				Vector3 v2 = endPoint   + binormal;
				Vector3 v3 = endPoint   - binormal;

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

		GameObject go = new GameObject ();
		go.transform.parent = parent.transform;
		go.AddComponent<MeshFilter> ();
		go.GetComponent<MeshFilter> ().mesh = mesh;
		go.AddComponent<MeshRenderer> ();
		go.GetComponent<MeshRenderer> ().material = Resources.Load<Material>("Materials/SegmentMaterial");

		goList.Add (go);
	}
}
