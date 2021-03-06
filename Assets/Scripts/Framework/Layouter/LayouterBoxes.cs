﻿using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class LayouterBoxes : Layouter
{
	public string mainParameter = "";
	public float  mMinValue = 0;
	public float  mMaxValue = 0;
	public int    mCount    = 0;
	public float  mDelta    = 0;

	public string secParameter = "";
	public float  sMinValue = 0;
	public float  sMaxValue = 0;
	public int    sCount    = 0;
	public float  sDelta    = 0;

	public Ensemble ensemble;
	public float    cameraXEndPos = 0;
	public float    cameraYEndPos = 0;

	int   camOffset  = 0;
	int   itemOffset = 1;
	int   dimSizeY   = 5;
	int   dimSizeX   = 6;
	
	float itemSizeX = 0;
	float itemSizeY = 0;
	
	float xPosStart = 0;
	float yPosStart = 0;

	public override Vector3 GetRequiredSpatialDimension()
	{
		return new Vector3 (0.5f, 0.5f, 0f);
	}

	public override void PlacePreview(int numOfElements)
	{
		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}

		previewEncapsulateGO = new GameObject ("Composition Preview");

		dimSizeX = numOfElements;
		dimSizeY = numOfElements;

		Camera cam        = Camera.main;

		float camSize = cam.orthographicSize;
		
		string[] res = UnityStats.screenRes.Split('x');
		float aspect = (float.Parse (res [0]) / float.Parse (res [1]));
		
		if(aspect >= 1)
			camSize *= (float.Parse (res [0]) / float.Parse (res [1]));

		itemSizeX = ((2f * (camSize - camOffset)) - ((float)dimSizeX - 1)*itemOffset) / (float)dimSizeX;
		itemSizeY = itemSizeX; 

		xPosStart = cam.gameObject.transform.position.x - camSize + (float)camOffset + itemSizeX / 2f;
		yPosStart = cam.gameObject.transform.position.y - camSize + (float)camOffset + itemSizeY / 2f;

		previewInstanceGO = new GameObject("Instance");
		PrimitivesGenerator.Box (previewInstanceGO, itemSizeX, itemSizeY, 0);
		previewInstanceGO.transform.parent = previewEncapsulateGO.transform;

		for (int i = 0; i < dimSizeX; i++) 
		{
			for (int j = 0; j < dimSizeY; j++) 
			{
				GameObject elementGO = GameObject.Instantiate(previewInstanceGO) as GameObject;
				elementGO.name = "Instance";
				elementGO.transform.parent = previewEncapsulateGO.transform;

				elementGO.transform.position = new Vector3(xPosStart + i * (itemSizeX + itemOffset), yPosStart + j * (itemSizeY + itemOffset), 0);
			}
		}

		elementSize = new Vector3 (itemSizeX, itemSizeY, 0);

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
		for (int i = 0; i < axisParameters.Count; i++) {
				paramsValues.Add (new KeyValuePair<string, float> (axisParameters [i].name, axisParameters [i].value));
		}

		ParameterInfo xPI = ensemble.parametersInfo [axisParameters [0].name];
		float xDelta = (xPI.maxValue - xPI.minValue) / (float)(xPI.count - 1);
		ParameterInfo yPI = ensemble.parametersInfo [axisParameters [1].name];
		float yDelta = (yPI.maxValue - yPI.minValue) / (float)(yPI.count - 1);

		List<float> xDeltas = new List<float> ();
		for (float i = xPI.minValue; i <= xPI.maxValue; i += xDelta)
		{
			xDeltas.Add(i);
		}
		
		if (xDeltas.Count > dimSizeX)
		{
			int diffCount = xDeltas.Count - dimSizeX;
			for(int i = 0; i < diffCount; i++)
			{
				xDeltas.RemoveAt(Random.Range(0, xDeltas.Count - 1));
			}
		}
		
		List<float> yDeltas = new List<float> ();
		for (float i = yPI.minValue; i <= yPI.maxValue; i += yDelta)
		{
			yDeltas.Add(i);
		}

		if (yDeltas.Count > dimSizeY)
		{
			int diffCount = yDeltas.Count - dimSizeY;
			for(int i = 0; i < diffCount; i++)
			{
				yDeltas.RemoveAt(Random.Range(0, yDeltas.Count - 1));
			}
		}
		
		foreach(float xValue in xDeltas)
		{
			foreach(float yValue in yDeltas)
			{
				paramsValues[0] = new KeyValuePair<string, float>(paramsValues[0].Key, xValue);
				paramsValues[1] = new KeyValuePair<string, float>(paramsValues[1].Key, yValue);
				resultStructures.Add(ensemble.GetStructure(paramsValues));
			}
		}

		yPosStart -= itemSizeY / 2f;

		int val = 0;
		foreach (Structure structure in		 resultStructures)
		{
			(structure.representation as RepresentationMesh).gameObject.SetActive(true);
			RepresentationMesh newRep = abstraction.Process(structure.representation as RepresentationMesh) as RepresentationMesh;
			(structure.representation as RepresentationMesh).gameObject.SetActive(false);
			
			newRep.gameObject.transform.parent  = compositionGameObject.transform;

			float valX = (float)(val / dimSizeY);
			float valY = (float)(val % dimSizeY);

			newRep.gameObject.transform.position = new Vector3(
				xPosStart + valX * (itemSizeX + itemOffset) + newRep.offset.x, 
				yPosStart + valY * (itemSizeY + itemOffset), 
				0);

			val++;
		}

		return resultMesh;
	}
	
	public void CameraPositionChangedX(float value)
	{
		ParameterInfo pi = ensemble.parametersInfo[axisParameters [0].name];
		
		float normalizedValue = (value - pi.minValue) / (pi.maxValue - pi.minValue);
		
		Vector3 cameraPosition = Camera.main.gameObject.transform.position;
		cameraPosition.x = normalizedValue * cameraXEndPos;
		Camera.main.gameObject.transform.position = cameraPosition;
	}

	public void CameraPositionChangedY(float value)
	{
		ParameterInfo pi = ensemble.parametersInfo[axisParameters [1].name];
		
		float normalizedValue = (value - pi.minValue) / (pi.maxValue - pi.minValue);
		
		Vector3 cameraPosition = Camera.main.gameObject.transform.position;
		cameraPosition.y = normalizedValue * cameraYEndPos;
		Camera.main.gameObject.transform.position = cameraPosition;
	}
}
