using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class LayouterStripes : Layouter
{
	public float  minValue      = 0;
	public float  maxValue      = 0;
	public int    count         = 0;
	public float  delta         = 0;
	public string mainParameter = "";

	public Ensemble ensemble = null;
	public float cameraXEndPos = 0;

	int   camOffset  = 0;
	int   itemOffset = 1;
	int   itemCount  = 6;
	
	float itemSizeX = 0;
	float itemSizeY = 0;
	
	float xPosStart = 0;
	float yPosStart = 0;

	public override Vector3 GetRequiredSpatialDimension()
	{
		return new Vector3 (0.5f, 1.0f, 0);
	}

	public override void PlacePreview(int numOfElements)
	{
		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}

		previewEncapsulateGO = new GameObject("Composition Preview");

		itemCount = numOfElements;

		Camera cam        = Camera.main;

		float camSize = cam.orthographicSize;

		string[] res = UnityStats.screenRes.Split('x');
		float aspect = (float.Parse (res [0]) / float.Parse (res [1]));

		if(aspect >= 1)
			camSize *= (float.Parse (res [0]) / float.Parse (res [1]));

		itemSizeX = ((2f * (camSize - camOffset)) - ((float)itemCount - 1) * itemOffset) / (float)itemCount;
		itemSizeY = itemSizeX;

		xPosStart = cam.gameObject.transform.position.x - camSize + (float)camOffset + itemSizeX / 2f;
		yPosStart = cam.gameObject.transform.position.y;

		previewInstanceGO = new GameObject("Instance");
		PrimitivesGenerator.Box (previewInstanceGO, itemSizeX, itemSizeY, 0);
		previewInstanceGO.transform.parent = previewEncapsulateGO.transform;

		for (int i = 0; i < itemCount; i++) 
		{
			GameObject elementGO = GameObject.Instantiate(previewInstanceGO) as GameObject;
			elementGO.name = "Instance";
			elementGO.transform.parent = previewEncapsulateGO.transform;

			elementGO.transform.position = new Vector3(xPosStart + i * (itemSizeX + itemOffset), yPosStart, 5f);
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
		GameObject uiGO   = GameObject.Find ("UI") as GameObject;
		GameObject xPanel = uiGO.transform.Find ("X Axis Panel").gameObject;
		GameObject yPanel = uiGO.transform.Find ("Y Axis Panel").gameObject;
		GameObject zPanel = uiGO.transform.Find ("Z Axis Panel").gameObject;

		xPanel.SetActive(false);
		yPanel.SetActive(false);
		zPanel.SetActive(false);

		return;
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
		
		ParameterInfo pi     = ensemble.parametersInfo [axisParameters [0].name];
		float         xDelta = (pi.maxValue - pi.minValue) / (float)(pi.count - 1);

		List<float> xDeltas = new List<float> ();

		for (float i = pi.minValue; i <= pi.maxValue; i += xDelta)
		{
			xDeltas.Add(i);
		}

		if (xDeltas.Count > itemCount)
		{
			int diffCount = xDeltas.Count - itemCount;
			for(int i = 0; i < diffCount; i++)
			{
				xDeltas.RemoveAt(Random.Range(0, xDeltas.Count - 1));
			}
		}

		foreach (float value in xDeltas)
		{
			paramsValues[0] = new KeyValuePair<string, float>(paramsValues[0].Key, value);
			resultStructures.Add(ensemble.GetStructure(paramsValues));
		}

		Camera cam        = Camera.main;
		yPosStart = cam.gameObject.transform.position.y - cam.orthographicSize + (float)camOffset;

		int val = 0;

		foreach (Structure structure in resultStructures)
		{
			(structure.representation as RepresentationMesh).gameObject.SetActive(true);
			RepresentationMesh newRep = abstraction.Process(structure.representation as RepresentationMesh) as RepresentationMesh;
			(structure.representation as RepresentationMesh).gameObject.SetActive(false);
			
			newRep.gameObject.transform.parent  = compositionGameObject.transform;

			newRep.gameObject.transform.position = new Vector3(xPosStart + val * (itemSizeX + itemOffset) + newRep.offset.x, yPosStart, 0);
			val++;
		}

		GameObject uiGO     = GameObject.Find ("UI") as GameObject;
		GameObject xPanel   = uiGO.transform.Find ("X Axis Panel").gameObject;
		GameObject sliderGO = xPanel.transform.Find ("Slider").gameObject;
		sliderGO.SetActive (true);
		Slider     slider = sliderGO.GetComponent<Slider> ();

		slider.minValue = pi.minValue;
		slider.maxValue = pi.maxValue;
		slider.value    = pi.minValue;

		slider.onValueChanged.RemoveAllListeners ();
		slider.onValueChanged.AddListener (CameraPositionChanged);

		cameraXEndPos = xPosStart + ((float)resultStructures.Count - 0.5f) * itemSizeX + ((float)resultStructures.Count - 1f) * itemOffset - cam.orthographicSize + camOffset; 

		cam.transform.position = new Vector3 (0, yPosStart/2f, -300f);

		return resultMesh;
	}

	public void CameraPositionChanged(float value)
	{
		ParameterInfo pi = ensemble.parametersInfo[axisParameters [0].name];

		float normalizedValue = (value - pi.minValue) / (pi.maxValue - pi.minValue);

		Vector3 cameraPosition = Camera.main.gameObject.transform.position;
		cameraPosition.x = normalizedValue * cameraXEndPos;
		Camera.main.gameObject.transform.position = cameraPosition;
	}
}
