using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RepresentationMesh : Representation
{
	public GameObject                     gameObject = null;
	public List<GameObject>               goList     = new List<GameObject> ();
	public Graph                          structure  = null;
	public Dictionary<string, Texture2D>  texList    = new Dictionary<string, Texture2D> ();
	public Dictionary<int, GraphNode>     graph      = new Dictionary<int, GraphNode> ();

	//public List<Vector3> additionalInformation = new List<Vector3> ();
	
	public RepresentationMesh ()
	{
		type = "mesh";
	}
	
	public RepresentationMesh (int nDimension)
	{
		type      = "mesh";
		dimension = nDimension;
	}

	public RepresentationMesh (GameObject nGo, int nDimension)
	{
		type      = "mesh";
		dimension = nDimension;

		goList.Add(nGo);
	}

	public RepresentationMesh (List<GameObject> nGoList, int nDimension)
	{
		type      = "mesh";
		dimension = nDimension;
		goList    = nGoList;
	}

	public RepresentationMesh (List<GameObject> nGoList, Graph nStructure, int nDimension)
	{
		type      = "mesh";
		dimension = nDimension;
		goList    = nGoList;
		structure = nStructure;
	}

	public bool isStructural()
	{
		return (structure != null);
	}

	public void SetMaterialTransparency(float transparency)
	{
		int childCount = gameObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject childGO = gameObject.transform.GetChild(i).gameObject;

			if(childGO.name != "Path" && childGO.name != "PathShadow")
			{
				Material childMaterial = Material.Instantiate(childGO.GetComponent<Renderer> ().sharedMaterial) as Material;
				
				Color childColor = childMaterial.color;
				childColor.a = transparency;
				childMaterial.color = childColor;
				
				childGO.GetComponent<Renderer> ().material = childMaterial;
			}
		}
	}

	public void SetMaterialColor(Color nColor)
	{
		int childCount = gameObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject childGO = gameObject.transform.GetChild(i).gameObject;
			//Material childMaterial = Material.Instantiate(childGO.GetComponent<Renderer> ().sharedMaterial) as Material;
			//Material childMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			Material childMaterial = new Material(Shader.Find("Unlit/Color"));

			childMaterial.color = nColor;

			childMaterial.mainTexture = null;
			
			childGO.GetComponent<Renderer> ().material = childMaterial;
		}
	}

	public void BlendMaterialColor(Color nColor)
	{
		int childCount = gameObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject childGO = gameObject.transform.GetChild(i).gameObject;
			Material childMaterial = Material.Instantiate(childGO.GetComponent<Renderer> ().sharedMaterial) as Material;
			
			childMaterial.color = nColor;
			
			childGO.GetComponent<Renderer> ().material = childMaterial;
		}
	}

	public void SetMaterialColorTransparency (Color nColor, float transparency)
	{
		int childCount = gameObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject childGO = gameObject.transform.GetChild(i).gameObject;
			Material childMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
			
			if(childGO.name != "Path" && childGO.name != "PathShadow")
			{
				Color childColor = nColor;
				childColor.a = transparency;
				
				childMaterial.color = childColor;
				
				childMaterial.mainTexture = null;

				childGO.GetComponent<Renderer> ().material = childMaterial;
			}
			else if(childGO.name == "Path")
			{
				childMaterial.color = nColor;
				
				childMaterial.mainTexture = null;
				childGO.GetComponent<Renderer> ().material = childMaterial;
			}
		}
	}
}
