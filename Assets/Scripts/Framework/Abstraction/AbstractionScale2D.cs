using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionScale2D : Abstraction
{	
	private bool  structured = false;
	public  float scaleX     = 0.5f;
	public  float scaleY     = 0.5f;

	float minX =  999f;
	float maxX = -999f;

	float minY =  999f;
	float maxY = -999f;

	public AbstractionScale2D()
	{
		dimension = new Vector3 (0.5f, 0.5f, 0);

		scale = 1f;
	}

	public AbstractionScale2D(float scale)
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
		
		if (repre.isStructural ())
			structured = true;
		
		result.dimension  = 1;
		result.type       = "mesh";
		result.gameObject = new GameObject (repre.gameObject.name);

		if (objectSize.x != 0)
		{
			if (objectSize.x < (repre.size.x * 0.5f))
			{
				scaleX = objectSize.x / repre.size.x;
			}
			else
			{
				//scaleX = scale;
				scaleX = 0.5f;
			}
		}
		else
		{
			scaleX = scale;
		}

		if (objectSize.y != 0)
		{
			if (objectSize.y < (repre.size.y * 0.5f))
			{
				scaleY = objectSize.y / repre.size.y;
			}
			else
			{
				//scaleY = scale;
				scaleY = 0.5f;
			}
		}
		else
		{
			scaleY = scale;
		}

		scaleX = scale;
		scaleY = scale;

		transform (result.gameObject, result.goList, repre.goList);
		
		result.structure = copyStructure (null, repre.structure as LSystemGraph, result.gameObject);

		result.offset = new Vector3 (-(maxX + minX) / 2, -(maxY + minY) / 2, 0);

		return result;
	}
	
	void transform (GameObject go, List<GameObject> resGO, List<GameObject> repGO)
	{
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

				vertices[j].x *= scaleX;
				vertices[j].y *= scaleY;

				if(vertices[j].x < minX)
				{
					minX = vertices[j].x;
				}
				
				if(vertices[j].x > maxX)
				{
					maxX = vertices[j].x;
				}

				if(vertices[j].y < minY)
				{
					minY = vertices[j].y;
				}
				
				if(vertices[j].y > maxY)
				{
					maxY = vertices[j].y;
				}

				j++;
			}
			
			mesh.vertices = vertices;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			
			resObject.GetComponent<MeshFilter>().mesh = mesh;
			
			resObject.transform.parent     = go.transform;
			resObject.transform.position   = Vector3.zero;
			resObject.transform.rotation   = Quaternion.identity;
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

		GameObject resObject = go.transform.GetChild (node.id).gameObject;
		Mesh       mesh      = Mesh.Instantiate(resObject.GetComponent<MeshFilter>().sharedMesh) as Mesh;
		Vector3[]  vertices  = mesh.vertices;
		
		Vector3 boxMin = new Vector3 ( 999f,  999f,  999f);
		Vector3 boxMax = new Vector3 (-999f, -999f, -999f);
		Vector3 resPosition = new Vector3 ();
		int j = 0;
		while (j < vertices.Length)
		{
			Vector3 vertexPos = resObject.transform.rotation * Vector3.Scale(vertices[j], resObject.transform.localScale) + resObject.transform.position;

			vertexPos.x *= scaleX;
			vertexPos.y *= scaleY;
			
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
