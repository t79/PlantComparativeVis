using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LayouterSuperPosition : Layouter
{
	public float  minValue = 0;
	public float  maxValue = 0;
	public int    count    = 0;
	public float  delta    = 0;
	public string mainParameter = "";

	int   itemOffset = 5;
	float itemSizeZ  = 0;

	public Ensemble ensemble;
	public SPType   type;

	float xPosStart = 0;
	float yPosStart = 0;
	float zPosStart = 0;

	public override Vector3 GetRequiredSpatialDimension()
	{
		return new Vector3 (1f, 1f, 0.5f);
	}

	public override void PlacePreview(int numOfElements)
	{
		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}
		
		previewEncapsulateGO = new GameObject("Composition Preview");

		Camera cam        = Camera.main;
		int    camOffset  = 20;
		int    dimSize    = 5;

		float itemSizeX = 2f * (cam.orthographicSize - camOffset);
		float itemSizeY = itemSizeX;
		
		xPosStart = cam.gameObject.transform.position.x;
		yPosStart = cam.gameObject.transform.position.y;

		itemSizeZ = 15;
		
		previewInstanceGO = new GameObject("Instance");
		PrimitivesGenerator.Box (previewInstanceGO, itemSizeX, itemSizeY, 0);
		previewInstanceGO.transform.parent = previewEncapsulateGO.transform;

		for (int i = 0; i < dimSize; i++) 
		{
			GameObject elementGO = GameObject.Instantiate(previewInstanceGO) as GameObject;
			elementGO.name = "Instance";
			elementGO.transform.parent = previewEncapsulateGO.transform;
			
			elementGO.transform.position = new Vector3(xPosStart, yPosStart, i * 10f);
		}

		elementSize = new Vector3 (itemSizeX, itemSizeY, 15);

		GameObject.DestroyImmediate (previewInstanceGO);
	}

	public override void DestroyGameObjects()
	{
		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}
	}

	public override void SetGUIPreview()
	{
		GameObject uiGO = GameObject.Find ("UI") as GameObject;
		uiGO.SetActive (true);
		
		GameObject xPanel = uiGO.transform.Find ("X Axis Panel").gameObject;
		GameObject yPanel = uiGO.transform.Find ("Y Axis Panel").gameObject;
		GameObject zPanel = uiGO.transform.Find ("Z Axis Panel").gameObject;
		
		xPanel.SetActive(false);
		yPanel.SetActive(false);
		zPanel.SetActive(false);
	}

	public override List<RepresentationMesh> PlaceRepresentations (Ensemble ensemble, Abstraction abstraction, GameObject compositionGameObject)
	{
		List<RepresentationMesh> resultMesh = new List<RepresentationMesh> ();
		List<Structure> resultStructures = new List<Structure> ();
		this.ensemble = ensemble;
		
		List<KeyValuePair<string, float>> paramsValues = new List<KeyValuePair<string, float>> ();
		for (int i = 0; i < axisParameters.Count; i++)
		{
			paramsValues.Add(new KeyValuePair<string, float>(axisParameters[i].name, axisParameters[i].value));
		}
		
		ParameterInfo zPI     = ensemble.parametersInfo [axisParameters [0].name];
		float         zDelta = (zPI.maxValue - zPI.minValue) / (float)(zPI.count - 1);
		
		for (float i = zPI.minValue; i <= zPI.maxValue; i += zDelta)
		{
			paramsValues[0] = new KeyValuePair<string, float>(paramsValues[0].Key, i);
			resultStructures.Add(ensemble.GetStructure(paramsValues));
		}
		
		Camera cam        = Camera.main;
		yPosStart  = cam.gameObject.transform.position.y - cam.orthographicSize;

		/*
		float xPosStart = cam.gameObject.transform.position.x;
		float yPosStart = cam.gameObject.transform.position.y - cam.orthographicSize;
		float zPosStart = 0;
		*/
		
		foreach (Structure structure in resultStructures)
		{
			(structure.representation as RepresentationMesh).gameObject.SetActive(true);
			RepresentationMesh newRep = abstraction.Process(structure.representation as RepresentationMesh) as RepresentationMesh;
			(structure.representation as RepresentationMesh).gameObject.SetActive(false);
			
			newRep.gameObject.transform.parent  = compositionGameObject.transform;
			
			float valZ = (structure.parameters [axisParameters[0].name].value - zPI.minValue) / zDelta;
			
			newRep.gameObject.transform.position = new Vector3(xPosStart, yPosStart, zPosStart + valZ * (itemSizeZ + itemOffset));

			switch(type)
			{
				case SPType.None:
				{
					break;
				}
				case SPType.Transparent:
				{
					newRep.SetMaterialTransparency(0.25f);
					break;
				}
				case SPType.Colorify:
				{
					float zVal = valZ / zPI.count;
					Color zColor = Color.white;

					if(zVal <= 0.5f)
					{
						zColor = new Color(1f, zVal * 2f, zVal * 2f);
					}
					else
					{
						zColor = new Color(2f - 2f * zVal, 2f - 2f * zVal, 1f);
					}

					newRep.SetMaterialColor(zColor);
					break;
				}
				case SPType.BlendColor:
				{
					float zVal = valZ / zPI.count;
					Color zColor = Color.white;
					
					if(zVal <= 0.5f)
					{
						zColor = new Color(1f, zVal * 2f, zVal * 2f);
					}
					else
					{
						zColor = new Color(2f - 2f * zVal, 2f - 2f * zVal, 1f);
					}
					
					newRep.BlendMaterialColor(zColor);
					break;
				}
				case SPType.ColorTransparent:
				{
					float zVal = valZ / zPI.count;
					Color zColor = Color.white;
					
					if(zVal <= 0.5f)
					{
						zColor = new Color(1f, zVal * 2f, zVal * 2f);
					}
					else
					{
						zColor = new Color(2f - 2f * zVal, 2f - 2f * zVal, 1f);
					}

					newRep.SetMaterialColorTransparency(zColor, 0.25f);
					break;
				}
				default:
				{
					newRep.SetMaterialTransparency(0.25f);
					break;
				}
			}
		}

		Vector3 target = new Vector3(xPosStart, yPosStart, zPosStart + (zPI.count / 2) * (itemSizeZ + itemOffset));
		Camera.main.gameObject.GetComponent<MouseCameraControl> ().yaw.target   = target;
		Camera.main.gameObject.GetComponent<MouseCameraControl> ().pitch.target = target;
		
		return resultMesh;
	}
}
