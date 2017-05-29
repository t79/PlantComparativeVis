using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbstractionScaleXZ : Abstraction
{
	private bool  structured = false;
	public  float scaleX     = 0.5f;
	public  float scaleY     = 0.5f;
	public  float scaleZ     = 0.5f;
	
	float minX =  999f;
	float maxX = -999f;
	
	float minZ =  999f;
	float maxZ = -999f;
	
	public AbstractionScaleXZ()
	{
		dimension = new Vector3 (0.5f, 1f, 0.5f);

		scale = 1f;
	}
	
	public override Representation Process(Representation representation)
	{
		RepresentationMesh result = new RepresentationMesh ();
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return result;
		
		if (repre.isStructural ())
			structured = true;
		
		result.dimension  = 3;
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
				scaleX = 0.5f;
			}
		}

		if (objectSize.z != 0)
		{
			if (objectSize.z < (repre.size.z * 0.5f))
			{
				scaleZ = objectSize.z / repre.size.z;
			}
			else
			{
				scaleY = 0.5f;
			}
		}
		
		transform (result.gameObject, result.goList, repre.goList);
		
		result.structure = copyStructure (null, repre.structure as LSystemGraph, repre.gameObject);
		
		result.offset = new Vector3 (-(maxX + minX) / 2, 0, -(maxZ + minZ) / 2);
		
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
				vertices[j].z *= scaleZ;
				
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
			
			vertices[j].x *= scaleX;
			vertices[j].z *= scaleZ;
			
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
