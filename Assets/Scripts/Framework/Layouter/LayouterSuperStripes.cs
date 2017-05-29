using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class LayouterSuperStripes : Layouter
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
	public SPType   type;
	public float cameraXEndPos;
	
	int   camOffset  = 0;
	int   itemOffset = 2;
	int   dimSizeX   = 5;
	int   dimSizeZ   = 10;
	
	float itemSizeX = 0;
	float itemSizeY = 0;
	
	float xPosStart = 0;
	float yPosStart = 0;
	float zPosStart = 0;

	public override Vector3 GetRequiredSpatialDimension()
	{
		return new Vector3 (0.5f, 1f, 0f);
	}

	public override void PlacePreview(int numOfElements)
	{
		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}
		
		previewEncapsulateGO = new GameObject("Composition Preview");

		dimSizeX = numOfElements;

		Camera cam        = Camera.main;

		float camSize = cam.orthographicSize;
		
		string[] res = UnityStats.screenRes.Split('x');
		float aspect = (float.Parse (res [0]) / float.Parse (res [1]));
		
		if(aspect >= 1)
			camSize *= (float.Parse (res [0]) / float.Parse (res [1]));
		
		itemSizeX = ((2f * (camSize - camOffset)) - ((float)dimSizeX - 1)*itemOffset) / (float)dimSizeX;
		itemSizeY = itemSizeX;

		xPosStart = cam.gameObject.transform.position.x - camSize + (float)camOffset + itemSizeX / 2f;
		yPosStart = cam.gameObject.transform.position.y;
		
		previewInstanceGO = new GameObject("Instance");
		PrimitivesGenerator.Box (previewInstanceGO, itemSizeX, itemSizeY, 0);
		previewInstanceGO.transform.parent = previewEncapsulateGO.transform;

		for (int i = 0; i < dimSizeX; i++) 
		{
			for(int j = 0; j < dimSizeZ; j++)
			{
				GameObject elementGO = GameObject.Instantiate(previewInstanceGO) as GameObject;
				elementGO.name = "Instance";
				elementGO.transform.parent = previewEncapsulateGO.transform;

				elementGO.transform.position = new Vector3(xPosStart + i * (itemSizeX + itemOffset), yPosStart, j * 10f);
			}
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
		
		ParameterInfo xPI    = ensemble.parametersInfo [axisParameters [0].name];
		float         xDelta = (xPI.maxValue - xPI.minValue) / (float)(xPI.count - 1);
		ParameterInfo zPI    = ensemble.parametersInfo [axisParameters [1].name];
		float         zDelta = (zPI.maxValue - zPI.minValue) / (float)(zPI.count - 1);

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

		List<float> zDeltas = new List<float> ();
		for (float i = zPI.minValue; i <= zPI.maxValue; i += zDelta)
		{
			zDeltas.Add(i);
		}

		foreach(float xValue in xDeltas)
		{
			foreach(float zValue in zDeltas)
			{
				paramsValues[0] = new KeyValuePair<string, float>(paramsValues[0].Key, xValue);
				paramsValues[1] = new KeyValuePair<string, float>(paramsValues[1].Key, zValue);
				resultStructures.Add(ensemble.GetStructure(paramsValues));
			}
		}
		
		Camera cam = Camera.main;
		yPosStart  = cam.gameObject.transform.position.y - cam.orthographicSize/2f + (float)camOffset;

		int val = 0;

		if (abstraction.GetType () == typeof(AbstractionScale1D))
		{
			xPosStart += itemSizeX / 2f;
		}

		foreach (Structure structure in resultStructures)
		{
			(structure.representation as RepresentationMesh).gameObject.SetActive(true);
			RepresentationMesh newRep = abstraction.Process(structure.representation as RepresentationMesh) as RepresentationMesh;
			(structure.representation as RepresentationMesh).gameObject.SetActive(false);
			
			newRep.gameObject.transform.parent  = compositionGameObject.transform;

			float valX = (float)(val / dimSizeZ);
			float valZ = (float)(val % dimSizeZ);

			newRep.gameObject.transform.parent  = compositionGameObject.transform;
			
			newRep.gameObject.transform.position = new Vector3(xPosStart + valX * (itemSizeX + itemOffset) + newRep.offset.x, yPosStart, zPosStart + valZ);
			val++;

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
		
		GameObject uiGO     = GameObject.Find ("UI") as GameObject;
		GameObject xPanel   = uiGO.transform.Find ("X Axis Panel").gameObject;
		GameObject sliderGO = xPanel.transform.Find ("Slider").gameObject;
		sliderGO.SetActive (true);
		Slider     slider = sliderGO.GetComponent<Slider> ();
		
		slider.minValue = xPI.minValue;
		slider.maxValue = xPI.maxValue;
		slider.value    = xPI.minValue;
		
		slider.onValueChanged.RemoveAllListeners ();
		slider.onValueChanged.AddListener (CameraPositionChanged);

		cameraXEndPos = xPosStart + ((float)xPI.count - 0.5f) * itemSizeX + ((float)xPI.count - 1f) * itemOffset - cam.orthographicSize + camOffset; 
		
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
