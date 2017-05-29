using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionScale1D : Abstraction
{
	public enum Axis {X, Y, Z};

	private bool  structured = false;
	public  Axis  scaleAxis  = Axis.X;
	public  float scaleValue = 0.5f; 

	float minX =  999f;
	float maxX = -999f;

	float minZ =  999f;
	float maxZ = -999f;

	public AbstractionScale1D()
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

	public AbstractionScale1D(Axis axis)
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

	public AbstractionScale1D(float scale)
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
		
		this.scale = scale;
	}

	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;

		if (repre.isStructural ())
			structured = true;

		result.dimension  = 2;
		result.type       = "mesh";
		result.gameObject = new GameObject (repre.gameObject.name);

		switch (scaleAxis)
		{
		case Axis.X:
		{
			if (objectSize.x != 0)
			{
				if (objectSize.x < (repre.size.x * 0.5f))
				{
					scaleValue = objectSize.x / repre.size.x;
				}
				else
				{
					scaleValue = scale;
					//scaleValue = 0.6f;
				}
			}
			else
			{
				scaleValue = scale;
			}
			break;
		}
		case Axis.Z:
		{
			if (objectSize.z != 0)
			{
				if (objectSize.z < (repre.size.z * 0.5f))
				{
					scaleValue = objectSize.z / repre.size.z;
				}
				else
				{
					//scaleValue = 0.5f;
					scaleValue = scale;
				}
			}
			else
			{
				scaleValue = scale;
			}
			break;
		}
		}

		scaleValue = scale;

		transform (result.gameObject, result.goList, repre.goList);

		result.structure = copyStructure (null, repre.structure as LSystemGraph, result.gameObject);

		result.offset = new Vector3 (-(maxX + minX) / 2, 0, 0);

		result.objectMin = new Vector3 (minX, 0, 0);
		result.objectMax = new Vector3 (maxX, 0, 0);

		return result;
	}

	void transform (GameObject go, List<GameObject> resGO, List<GameObject> repGO)
	{
		minX =  999f;
		maxX = -999f;

		for(int i = 0; i < repGO.Count; i++)
		{
			GameObject resObject     = GameObject.Instantiate(repGO[i].gameObject) as GameObject;
			Mesh       mesh          = Mesh.Instantiate(resObject.GetComponent<MeshFilter>().sharedMesh) as Mesh;
			Vector3[]  vertices      = mesh.vertices;
			
			int j = 0;
			while (j < vertices.Length)
			{
				if(structured)
					vertices[j] = resObject.transform.rotation * Vector3.Scale(vertices[j], resObject.transform.localScale) + resObject.transform.position;

				switch(scaleAxis)
				{
					case Axis.X:
					{
						vertices[j].x *= scaleValue;
						break;
					}
					case Axis.Y:
					{
						vertices[j].y *= scaleValue;
						break;
					}
					case Axis.Z:
					{
						vertices[j].z *= scaleValue;
						break;
					}
				}

				if(vertices[j].x < minX)
				{
					minX = vertices[j].x;
				}

				if(vertices[j].x > maxX)
				{
					maxX = vertices[j].x;
				}

				if(vertices[j].z < minZ)
				{
					minZ = vertices[j].z;
				}
				
				if(vertices[j].z > maxZ)
				{
					maxZ = vertices[j].z;
				}

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
			resGO.Add(resObject);
		}
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
		resNode.orientation = node.orientation;

		GameObject resObject = go.transform.GetChild (node.id).gameObject;
		Mesh       mesh          = Mesh.Instantiate(resObject.GetComponent<MeshFilter>().sharedMesh) as Mesh;
		Vector3[]  vertices      = mesh.vertices;

		Vector3 boxMin = new Vector3 ( 999f,  999f,  999f);
		Vector3 boxMax = new Vector3 (-999f, -999f, -999f);
		Vector3 resPosition = new Vector3 ();
		int j = 0;
		while (j < vertices.Length)
		{
			Vector3 vertexPos = resObject.transform.rotation * Vector3.Scale(vertices[j], resObject.transform.localScale) + resObject.transform.position;

			switch(scaleAxis)
			{
			case Axis.X:
			{
				vertexPos.x *= scaleValue;
				break;
			}
			case Axis.Y:
			{
				vertexPos.y *= scaleValue;
				break;
			}
			case Axis.Z:
			{
				vertexPos.z *= scaleValue;
				break;
			}
			}
			
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

			resPosition += vertexPos;
			j++;
		}
		resNode.position = resPosition / vertices.Length;
		resNode.size = boxMax - boxMin;

		foreach (LSystemGraph child in node.neighbour)
		{
			resNode.neighbour.Add(copyStructure(resNode, child, go));
		}

		return resNode;
	}
}
