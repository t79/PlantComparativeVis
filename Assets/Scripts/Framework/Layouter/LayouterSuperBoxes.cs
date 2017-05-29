using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class LayouterSuperBoxes : Layouter
{
	public string mainParameter  = "";
	public float  mMinValue      = 0;
	public float  mMaxValue      = 0;
	public int    mCount         = 0;
	public float  mDelta         = 0;
	
	public string secParameter   = "";
	public float  sMinValue      = 0;
	public float  sMaxValue      = 0;
	public int    sCount         = 0;
	public float  sDelta         = 0;

	public string thirdParameter = "";
	public float  tMinValue      = 0;
	public float  tMaxValue      = 0;
	public int    tCount         = 0;
	public float  tDelta         = 0;

	public Ensemble ensemble;
	public SPType   type;
	public float    cameraXEndPos = 0;
	public float    cameraYEndPos = 0;

	int   camOffset  = 0;
	int   itemOffset = 1;
	int   dimSize    = 5;
	
	float itemSizeX = 0;
	float itemSizeY = 0;
	
	float xPosStart = 0;
	float yPosStart = 0;
//	float zPosStart = 0;

	int   dimSizeY   = 5;
	int   dimSizeX   = 5;

	public override Vector3 GetRequiredSpatialDimension()
	{
		return new Vector3 (0.5f, 0.5f, 0);
	}

	public override void PlacePreview(int numOfElements)
	{
		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}
		
		previewEncapsulateGO = new GameObject("Composition Preview");

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
				for (int k = 0; k < dimSize; k++)
				{
					GameObject elementGO = GameObject.Instantiate(previewInstanceGO) as GameObject;
					elementGO.name = "Instance";
					elementGO.transform.parent = previewEncapsulateGO.transform;
					
					elementGO.transform.position = new Vector3(xPosStart + i * (itemSizeX + itemOffset), yPosStart + j * (itemSizeY + itemOffset), k * 10f);
				}

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
		
		ParameterInfo xPI     = ensemble.parametersInfo [axisParameters [0].name];
		double         xDelta = (xPI.maxValue - xPI.minValue) / (float)(xPI.count - 1);
		ParameterInfo yPI     = ensemble.parametersInfo [axisParameters [1].name];
		double         yDelta = (yPI.maxValue - yPI.minValue) / (float)(yPI.count - 1);
		ParameterInfo zPI     = ensemble.parametersInfo [axisParameters [2].name];
		double         zDelta = (zPI.maxValue - zPI.minValue) / (float)(zPI.count - 1);


		for (double i = xPI.minValue; i <= xPI.maxValue; i += xDelta)
		{
			for(double j = yPI.minValue; j <= yPI.maxValue; j += yDelta)
			{
				for(double k = zPI.minValue; k <= zPI.maxValue; k += zDelta)
				{
					paramsValues[0] = new KeyValuePair<string, float>(paramsValues[0].Key, (float)i);
					paramsValues[1] = new KeyValuePair<string, float>(paramsValues[1].Key, (float)j);
					paramsValues[2] = new KeyValuePair<string, float>(paramsValues[2].Key, (float)k);
					resultStructures.Add(ensemble.GetStructure(paramsValues));
				}
			}
		}
		
		Camera cam = Camera.main;
		//yPosStart -= itemSizeY / 2f;
		float val = 0;

		List<GameObject> gObjects = new List<GameObject> ();

		foreach (Structure structure in resultStructures)
		{
			(structure.representation as RepresentationMesh).gameObject.SetActive(true);
			RepresentationMesh newRep = abstraction.Process(structure.representation as RepresentationMesh) as RepresentationMesh;
			(structure.representation as RepresentationMesh).gameObject.SetActive(false);
			
			newRep.gameObject.transform.parent  = compositionGameObject.transform;

			newRep.gameObject.AddComponent<StructureProperties> ();
			newRep.gameObject.GetComponent<StructureProperties> ().size   = newRep.size;
			newRep.gameObject.GetComponent<StructureProperties> ().offset = newRep.offset;

			float valZ = Mathf.Floor(val / (dimSize * dimSizeY));
			float vaal = (float)(val % (dimSize * dimSizeY));

			float valY = (float)(vaal % dimSizeY);
			float valX = Mathf.Floor(vaal / dimSizeY); 

			valX = 4 - valX;
			
			newRep.gameObject.transform.position = new Vector3(
				xPosStart + valX * (itemSizeX + itemOffset), 
				yPosStart + valY * (itemSizeY + itemOffset) - 345, 
				/*valZ * -20f*/0);

			val++;

			switch(type)
			{
				case SPType.None:
				{
					break;
				}
				case SPType.Transparent:
				{
					newRep.SetMaterialTransparency(0.20f);
					break;
				}
				case SPType.Colorify:
				{
					float zVal = valZ * (1f / (zPI.count - 1));
					Color zColor = Color.white;

					if(zVal <= 0.5f)
					{
						zColor = new Color(1f, zVal * 2f, zVal * 2f);
					}
					else
					{
						zColor = new Color(2f - 2f * zVal, 2f - 2f * zVal, 1f);
					}

					//zColor = colors[(int)valZ];
					
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
					float zVal = valZ / (zPI.count - 1);
					Color zColor = Color.white;
					
					if(zVal <= 0.5f)
					{
						zColor = new Color(1f, zVal * 2f, zVal * 2f);
					}
					else
					{
						zColor = new Color(2f - 2f * zVal, 2f - 2f * zVal, 1f);
					}
					
					newRep.SetMaterialColorTransparency(zColor, 0.15f);
					break;
				}
				default:
				{
					newRep.SetMaterialTransparency(0.20f);
					break;
				}
			}
			gObjects.Add(newRep.gameObject);
		} 
		 
		ReorderEverything (gObjects);
		
		// X slider
		GameObject uiGO      = GameObject.Find ("UI") as GameObject;
		GameObject xPanel    = uiGO.transform.Find ("X Axis Panel").gameObject;
		GameObject xSliderGO = xPanel.transform.Find ("Slider").gameObject;
		xSliderGO.SetActive (true);
		Slider     xSlider = xSliderGO.GetComponent<Slider> ();
		
		xSlider.minValue = xPI.minValue;
		xSlider.maxValue = xPI.maxValue;
		xSlider.value    = xPI.minValue;
		
		xSlider.onValueChanged.RemoveAllListeners ();
		xSlider.onValueChanged.AddListener (CameraPositionChangedX);

		cameraXEndPos = xPosStart + ((float)xPI.count - 0.5f) * itemSizeX + ((float)xPI.count - 1f) * itemOffset - cam.orthographicSize + camOffset; 
		
		// Y slider
		GameObject yPanel    = uiGO.transform.Find ("Y Axis Panel").gameObject;
		GameObject ySliderGO = yPanel.transform.Find ("Slider").gameObject;
		ySliderGO.SetActive (true);
		Slider     ySlider = ySliderGO.GetComponent<Slider> ();
		
		ySlider.minValue = yPI.minValue;
		ySlider.maxValue = yPI.maxValue;
		ySlider.value    = yPI.minValue;
		
		ySlider.onValueChanged.RemoveAllListeners ();
		ySlider.onValueChanged.AddListener (CameraPositionChangedY);

		cameraYEndPos = yPosStart + ((float)yPI.count) * itemSizeY + ((float)yPI.count - 1f) * itemOffset - cam.orthographicSize + camOffset; 

		cam.transform.position = new Vector3 (0, 0, 300f);
		cam.transform.rotation = Quaternion.Euler (new Vector3 (0, 180f, 0));

		return resultMesh;
	}

	void ReorderEverything(List<GameObject> gObjects)
	{
		List<GameObject> selection = new List<GameObject> ();
		for (int i = 0; i < 25; i++)
		{
			selection.Add(gObjects[i +   0]);
			selection.Add(gObjects[i +  25]);
			selection.Add(gObjects[i +  50]);
			selection.Add(gObjects[i +  75]);
			selection.Add(gObjects[i + 100]);

			ReorderSelection(selection);
			ReorderPaths(selection);

			selection.Clear();
		}
	}

	void ReorderSelection(List<GameObject> gObjects)
	{
		for (int i = 0; i < 5; i++)
		{
			string nameObj = i.ToString();

			SortedDictionary<float, GameObject> sortSpheres = new SortedDictionary<float, GameObject>();
			for(int j = 0; j < gObjects.Count; j++)
			{
				Transform go = gObjects[j].transform.Find(nameObj);
				if(go != null)
				{
					sortSpheres[go.localScale.x] = go.gameObject;
				}
			}

			int index = 0;
			foreach(KeyValuePair<float, GameObject> sphere in sortSpheres)
			{
				sphere.Value.transform.position += new Vector3(0, 0, index * -10f);
				index++;
			}
		}
	}

	void ReorderPaths(List<GameObject> gObjects)
	{
		for (int i = 0; i < 5; i++)
		{
			for(int j = i; j < 5; j++)
			{
				string one = i.ToString() + j.ToString();
				string two = j.ToString() + i.ToString();

				SortedDictionary<float, GameObject> sortPaths = new SortedDictionary<float, GameObject>();
				for(int k = 0; k < gObjects.Count; k++)
				{
					Transform go1 = gObjects[k].transform.Find(one);
					if(go1 != null)
					{
						sortPaths[go1.localScale.x] = go1.gameObject;
					}
					else
					{
						go1 = gObjects[k].transform.Find(two);

						if(go1 != null)
						{
							sortPaths[go1.localScale.x] = go1.gameObject;
						}
					}
				}
				
				int index = 0;
				foreach(KeyValuePair<float, GameObject> path in sortPaths)
				{
					path.Value.transform.position += new Vector3(0, 0, 50f + index * -10f);
					index++;
				}
			}


		}
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
