using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionHoleSnap : Abstraction
{	
	private int           textureSize = 1024;
	private Texture2D     texture;
	private RenderTexture renderTexture;
	private GameObject    camera;
	
	private GameObject    trianglesMesh;
	private List<Vector2> holesPositions = new List<Vector2> ();
	private List<Vector3> holesTriangles = new List<Vector3> ();

	public AbstractionHoleSnap()
	{
		dimension = new Vector3 (1f, 1f, 0);

		scale = 1f;
	}

	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;

		result.dimension  = 2;
		result.type       = "mesh";
		result.gameObject = new GameObject (repre.gameObject.name);

		texture            = new Texture2D(textureSize, textureSize);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode   = TextureWrapMode.Clamp;
		
		renderTexture = new RenderTexture(textureSize,     textureSize,     0);
		RenderTexture renderTextureTmp = RenderTexture.active;

		camera                    = new GameObject("Camera");
		camera.transform.position = new Vector3 (0, 150f, -290f);

		camera.AddComponent<Camera> ();
		camera.GetComponent<Camera> ().backgroundColor  = Color.white;
		camera.GetComponent<Camera> ().orthographic     = true;
		camera.GetComponent<Camera> ().orthographicSize = 150;
		camera.GetComponent<Camera> ().targetTexture    = renderTexture;
		camera.AddComponent<GrayscaleBinary> ();
		Shader shd = Shader.Find("Hidden/GrayscaleBinary");
		camera.GetComponent<GrayscaleBinary> ().shader = shd;

		holesPositions.Clear ();
		holesTriangles.Clear ();

		verticesToGlobal (result.gameObject, result.goList, repre.goList);
		detectHoles ();
		triangulizePoints ();
		getBarycentricPositions (result.goList);
		snapHolesVerticesToGrid ();
		transformVertices (result.goList);
		result.structure = copyStructure (result.gameObject ,null, repre.structure as LSystemGraph);

		RenderTexture.active = renderTextureTmp;
		GameObject.DestroyImmediate (camera);

		//Debug.Log ("HoleSnap : " + (Time.realtimeSinceStartup - startTime));

		return result;
	}

	void verticesToGlobal (GameObject go, List<GameObject> resGO, List<GameObject> repGO)
	{
		for(int i = 0; i < repGO.Count; i++)
		{
			GameObject resObject     = GameObject.Instantiate(repGO[i].gameObject) as GameObject;
			Mesh       mesh          = Mesh.Instantiate(resObject.GetComponent<MeshFilter>().sharedMesh) as Mesh;
			Vector3[]  vertices      = mesh.vertices;
			
			int j = 0;
			while (j < vertices.Length)
			{
				vertices[j] = resObject.transform.rotation * Vector3.Scale(vertices[j], resObject.transform.localScale) + resObject.transform.position;
				j++;
			}

			mesh.vertices = vertices;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			resObject.GetComponent<MeshFilter>().mesh = mesh;

			resObject.name                 = repGO[i].name;
			resObject.transform.parent     = go.transform;
			resObject.transform.position   = Vector3.zero;
			resObject.transform.rotation   = Quaternion.identity;
			resObject.transform.localScale = Vector3.one;
			resGO.Add(resObject);
		}
	}

	void detectHoles ()
	{
		renderTexture.DiscardContents ();
		RenderTexture.active = camera.GetComponent<Camera> ().targetTexture;
		camera.GetComponent<Camera> ().Render ();

		texture.filterMode = FilterMode.Point;
		texture.wrapMode   = TextureWrapMode.Clamp;

		texture.ReadPixels( new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();

		for(int i = 0; i < 4; i++ )
		{
			TextureScale.Half(texture);
		}


		FloodFill.Fill(texture, 0,                 0);
		FloodFill.Fill(texture, texture.width - 1, 0);
		FloodFill.Fill(texture, texture.width - 1, texture.height - 1);
		FloodFill.Fill(texture, 0,                 texture.height - 1);

		// holes
		ConnectedComponentLabeling ccl = new ConnectedComponentLabeling();
		ccl.Process(texture, texture, false, false);
		holesPositions = ccl.holesAveragePositions;
		
		holesPositions.Add (new Vector2 (0f,                   0f));
		holesPositions.Add (new Vector2 ((float)texture.width, 0f));
		holesPositions.Add (new Vector2 (0f,                   (float)texture.height));
		holesPositions.Add (new Vector2 ((float)texture.width, (float)texture.height));
	}

	void triangulizePoints ()
	{
		Triangulator triangulator = new Triangulator ();
		int[] indices = triangulator.TriangulatePolygon (holesPositions.ToArray ());
		
		for(int i =0; i < indices.Length; i+=3)
		{
			holesTriangles.Add(new Vector3((float)indices[i], (float)indices[i+1], (float)indices[i+2]));
		}
		
		Vector3[] vertices = new Vector3[holesPositions.Count];
		for (int i=0; i < vertices.Length; i++)
		{
			Vector3 worldPos = new Vector3(holesPositions[i].x, holesPositions[i].y, 0);
			
			worldPos.x = (worldPos.x * (2f * 150f) / texture.width) - 150f;
			worldPos.y = worldPos.y * (2f * 150f) / texture.height;
			
			vertices[i] = worldPos;
		}
		
		// Create object
		trianglesMesh = new GameObject("trianglesMesh");
		
		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		
		// Set up game object with mesh;
		trianglesMesh.AddComponent(typeof(MeshRenderer));
		MeshFilter filter = trianglesMesh.AddComponent(typeof(MeshFilter)) as MeshFilter;
		filter.sharedMesh = msh;
		
		trianglesMesh.transform.position += new Vector3 (0f, 0f, -10f);
		trianglesMesh.AddComponent<MeshCollider> ();
	}

	void getBarycentricPositions(List<GameObject> goList)
	{
		for(int i = 0; i < goList.Count; i++)
		{
			GameObject childObject = goList[i].gameObject;
			
			childObject.AddComponent<BarycentricCoordinates>();
			
			
			Mesh mesh = childObject.GetComponent<MeshFilter>().sharedMesh;
			Vector3[] vertices = mesh.vertices;
			
			int j = 0;
			while (j < vertices.Length)
			{
				Vector4 barycentric = new Vector4(0f, 0f, 0f, 0f);
				
				RaycastHit hit;
				Vector3 pos = Camera.main.WorldToScreenPoint(vertices[j]);
				
				if (!Physics.Raycast(Camera.main.ScreenPointToRay(pos), out hit))
				{
					j++;
					childObject.GetComponent<BarycentricCoordinates>().positions.Add(barycentric);
					continue;
				}
				
				MeshCollider meshCollider = hit.collider as MeshCollider;
				if (meshCollider == null || meshCollider.sharedMesh == null)
				{
					j++;
					childObject.GetComponent<BarycentricCoordinates>().positions.Add(barycentric);
					continue;
				}

				barycentric.x = hit.barycentricCoordinate.x;
				barycentric.y = hit.barycentricCoordinate.y;
				barycentric.z = hit.barycentricCoordinate.z;
				barycentric.w = hit.triangleIndex;

				j++;
				childObject.GetComponent<BarycentricCoordinates>().positions.Add(barycentric);
			}
		}
	}

	void snapHolesVerticesToGrid ()
	{
		/*
		GameObject grid = GameObject.Find ("Grid");

		if (grid == null)
		{
			grid = new GameObject("Grid");
		}
		*/

		MeshFilter tmmf = trianglesMesh.GetComponent<MeshFilter> () as MeshFilter;
		Vector3[] vertices = tmmf.sharedMesh.vertices;

		int gridDimension = 6;
		int gridSize = (int)trianglesMesh.GetComponent<Collider>().bounds.size.x;
		
		float unitSize = (float)gridSize / (float)gridDimension;
		
		int j = 0;
		while(j < vertices.Length)
		{
			Vector3 pos = vertices[j];
			
			pos.x = Mathf.Round(pos.x / unitSize) * (float)unitSize;
			pos.y = Mathf.Round(pos.y / unitSize) * (float)unitSize;
			pos.z = Mathf.Round(pos.z / unitSize) * (float)unitSize;
			
			vertices[j] = pos;
			/*
			string gridPoint = pos.x + "x" + pos.y;

			if(GameObject.Find(gridPoint) == null)
			{
				GameObject gridPointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				gridPointObject.name = gridPoint;
				gridPointObject.transform.position = pos;
				gridPointObject.transform.localScale = new Vector3(10f, 10f, 10f);
				gridPointObject.GetComponent<MeshRenderer>().enabled = false;
				gridPointObject.transform.parent = grid.transform;
			}
			*/

			j++;
		}
		
		tmmf.sharedMesh.vertices = vertices;
	}

	void transformVertices (List<GameObject> goList)
	{
		MeshFilter tmmf    = trianglesMesh.GetComponent<MeshFilter> () as MeshFilter;
		Vector3[] vertices = tmmf.sharedMesh.vertices;
		
		for(int i = 0; i < goList.Count; i++)
		{
			GameObject childObject = goList[i];
			
			BarycentricCoordinates barycentricScript = childObject.GetComponent<BarycentricCoordinates>() as BarycentricCoordinates;
			List<Vector4> barycentricInfos = barycentricScript.positions;
			
			Mesh childMesh          = Mesh.Instantiate(childObject.GetComponent<MeshFilter>().sharedMesh) as Mesh;
			Vector3[] childVertices = childMesh.vertices;
			
			int j = 0;
			while (j < childVertices.Length)
			{
				Vector3 newPos = new Vector3();
				
				int triangleIndex = (int)barycentricInfos[j].w;
				Vector3 barycentric = new Vector3(barycentricInfos[j].x, barycentricInfos[j].y, barycentricInfos[j].z);

				Vector3 firstPoint  = barycentric.x * (vertices[(int)holesTriangles[triangleIndex].x]);
				Vector3 secondPoint = barycentric.y * (vertices[(int)holesTriangles[triangleIndex].y]);

				Vector3 thirdPoint  = barycentric.z * (vertices[(int)holesTriangles[triangleIndex].z]);
				
				newPos = firstPoint + secondPoint + thirdPoint;

				childVertices[j] = newPos;

				j++;
			}

			childMesh.vertices = childVertices;
			childMesh.RecalculateBounds();
			childMesh.RecalculateNormals();

			childObject.GetComponent<MeshFilter>().mesh = childMesh;
		}
		
		GameObject.DestroyImmediate (trianglesMesh);
	}

	LSystemGraph copyStructure (GameObject gameObject, LSystemGraph parent, LSystemGraph node)
	{
		if (node == null)
			return null;
		
		LSystemGraph resNode = new LSystemGraph ();
		resNode.id           = node.id;
		resNode.symbol       = node.symbol;
		resNode.parent       = parent;
		resNode.angle        = node.angle;

		GameObject go = gameObject.transform.GetChild (resNode.id).gameObject;

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
		
		resNode.size = boxMax - boxMin;
		
		foreach (LSystemGraph child in node.neighbour)
		{
			resNode.neighbour.Add(copyStructure(gameObject, resNode, child));
		}
		
		return resNode;
	}
}
