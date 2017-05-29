using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum CVType { Juxtaposition, Superposition, Hybrid }
public enum SPType { None, Transparent, BlendColor, Colorify, ColorTransparent }

public class Composition
{
	//INPUT
	public Ensemble                        ensemble;
	public List<Characteristic>            characteristics;
	public Dictionary<string, Abstraction> abstractions;
	public Layouter                        layouter;

	//OPEN PARAMETERS
	public CVType comparativeType;
	public int    visualSpaceDim;
	public int    parametersDim;

	//SAMPLING
	private List<int>                       samplesIDs              = new List<int> ();
	private List<RepresentationMesh>        samplesRepre            = new List<RepresentationMesh> ();
	private List<GameObject>                samplesRepreGO          = new List<GameObject> ();
	private Dictionary<string, List<float>> samplesAbsoluteMeasures = new Dictionary<string, List<float>> ();
	private List<List<float>>               samplesDeltaMeasures    = new List<List<float>> ();
	public  Dictionary<string, List<float>> samplesDeltaMeasuresDic = new Dictionary<string, List<float>> ();

	//UNITY GAME OBJECT WRAPPER
	private GameObject compositionGameObject = null;

	//UNITY EDITOR VARIABLES
	public bool isInitialized        = false;
	public bool isLoaded             = false;
	public bool wasLayoutChanged     = false;
	public bool isCompositionCreated = false;

	public Composition()
	{
		abstractions    = new Dictionary<string, Abstraction> ();
		characteristics = new List<Characteristic> ();
	}

	public void GenerateSample( int samplesCount )
	{
		samplesIDs.Clear();

		for (int i = 0; i < samplesCount; i++)
		{
			bool added = false;
			while(added == false)
			{
				int num = UnityEngine.Random.Range (0, ensemble.structures.Count - 1);
				
				if(!samplesIDs.Contains(num))
				{
					samplesIDs.Add(num);
					added = true;
				}
			}
		}
		samplesIDs.Sort ();
	}

	public void PreloadDeformations()
	{
		samplesRepre.Clear ();
		samplesRepreGO.Clear ();

		GameObject origWrapper = new GameObject("original");

		AbstractionCopy ac = new AbstractionCopy ();

		for (int i = 0; i < samplesIDs.Count; i++)
		{
			samplesRepre.Add(ac.Process(ensemble.structures [samplesIDs[i]].representation) as RepresentationMesh);
			samplesRepre[samplesRepre.Count - 1].gameObject.transform.parent = origWrapper.transform;
			samplesRepre[samplesRepre.Count - 1].gameObject.SetActive(false);
		}

		samplesRepreGO.Add (origWrapper);

		foreach (KeyValuePair<string, Abstraction> abstraction in abstractions)
		{
			string abstractionName = "";
			if (abstraction.Key != "")
			{
				abstractionName = abstraction.Key;
				GameObject repreWrapper = new GameObject(abstractionName);
				
				for (int i = 0; i < samplesIDs.Count; i++)
				{
					samplesRepre.Add(abstraction.Value.Process(ensemble.structures [samplesIDs[i]].representation) as RepresentationMesh);
					samplesRepre[samplesRepre.Count - 1].gameObject.transform.parent = repreWrapper.transform;
					samplesRepre[samplesRepre.Count - 1].gameObject.SetActive(false);
				}

				samplesRepreGO.Add (repreWrapper);
			}
		}
	}

	public void PreloadMeasurements()
	{
		for(int j = 0; j < samplesRepreGO.Count; j++)
		{
			List<float> charList = new List<float> ();
			for (int i = 0; i < characteristics.Count; i++)
			{
				charList.Add(measureRepresentationCharacteristic (j, i, samplesIDs.Count));
			}
			
			samplesAbsoluteMeasures[samplesRepreGO[j].name] = charList;
		}
		
		foreach (KeyValuePair<string, List<float>> item in samplesAbsoluteMeasures)
		{
			if(item.Key == "original")
				continue;
			
			List<float> measuresDeltas    = new List<float> ();
			List<float> measuresDeltasDic = new List<float> ();
			for(int i = 0; i < item.Value.Count; i++)
			{
				measuresDeltas.Add( Mathf.Abs( samplesAbsoluteMeasures["original"][i] - samplesAbsoluteMeasures[item.Key][i] ));
				measuresDeltasDic.Add(Mathf.Abs( samplesAbsoluteMeasures["original"][i] - samplesAbsoluteMeasures[item.Key][i] ));
			}
			
			samplesDeltaMeasures.Add(measuresDeltas);
			samplesDeltaMeasuresDic[item.Key] = measuresDeltasDic;
		}
		
		for (int i = 0; i < characteristics.Count; i++)
		{
			List<float> sList = new List<float> ();
			
			for(int j = 0; j < samplesDeltaMeasures.Count; j++)
			{
				if(!sList.Contains(samplesDeltaMeasures[j][i]))
					sList.Add(samplesDeltaMeasures[j][i]);
			}
			
			sList.Sort();
			sList.Reverse();
			
			for(int j = 0; j < samplesDeltaMeasures.Count; j++)
			{
				samplesDeltaMeasures[j][i] = sList.IndexOf(samplesDeltaMeasures[j][i]) + 1;
			}
		}
	}
	
	public string pickupAbstraction()
	{
		List<float> finalValues = new List<float> ();
		List<float> finalValuesTmp = new List<float> ();
		
		for(int i = 0; i < samplesDeltaMeasures.Count; i++)
		{
			float val = 0;
			for(int j = 0; j < characteristics.Count; j++)
			{
				float increment = samplesDeltaMeasures[i][j] * characteristics[j].weight * abstractions[abstractions.Keys.ToList()[i]].DimensionWeight(layouter.GetRequiredSpatialDimension());
				
				val += increment;
			}
			
			finalValues.Add(val);
			finalValuesTmp.Add(val);
		}
		
		finalValues.Sort ();
		finalValues.Reverse ();
		
		int resultIndex = finalValuesTmp.IndexOf (finalValues [0]);
		
		return abstractions.Keys.ToList () [resultIndex];
	}

	public void PreloadMeasurementsNew()
	{
		for(int j = 0; j < samplesRepreGO.Count; j++)
		{
			List<float> charList = new List<float> ();
			for (int i = 0; i < characteristics.Count; i++)
			{
				charList.Add(measureRepresentationCharacteristic (j, i, samplesIDs.Count));
			}
			
			samplesAbsoluteMeasures[samplesRepreGO[j].name] = charList;
		}
		
		foreach (KeyValuePair<string, List<float>> item in samplesAbsoluteMeasures)
		{
			if(item.Key == "original")
				continue;
			
			List<float> measuresDeltas    = new List<float> ();
			List<float> measuresDeltasDic = new List<float> ();
			for(int i = 0; i < item.Value.Count; i++)
			{
				measuresDeltas.Add( Mathf.Abs( samplesAbsoluteMeasures["original"][i] - samplesAbsoluteMeasures[item.Key][i] ));
				measuresDeltasDic.Add(Mathf.Abs( samplesAbsoluteMeasures["original"][i] - samplesAbsoluteMeasures[item.Key][i] ));
			}
			
			samplesDeltaMeasures.Add(measuresDeltas);
			samplesDeltaMeasuresDic[item.Key] = measuresDeltasDic;
		}
		
		for (int i = 0; i < characteristics.Count; i++)
		{
			float minValue = float.MaxValue;
			float maxValue = float.MinValue;

			for(int j =0; j < samplesDeltaMeasures.Count; j++)
			{
				if(samplesDeltaMeasures[j][i] < minValue)
					minValue = samplesDeltaMeasures[j][i];

				if(samplesDeltaMeasures[j][i] > maxValue)
					maxValue = samplesDeltaMeasures[j][i];
			}
			
			for(int j = 0; j < samplesDeltaMeasures.Count; j++)
			{
				samplesDeltaMeasures[j][i] = (samplesDeltaMeasures[j][i] - minValue) / (maxValue - minValue);
			}
		}
	}

	public string pickupAbstractionNew()
	{
		List<float> finalValues = new List<float> ();
		List<float> finalValuesTmp = new List<float> ();
		
		for(int i = 0; i < samplesDeltaMeasures.Count; i++)
		{
			string abstrName = abstractions.Keys.ToList () [i];
			float scaleValue = abstractions[abstrName].scale;

			int numOfItems = Mathf.FloorToInt((Camera.main.orthographicSize * 2f) / (scaleValue * 300f));
			
			float val = 0;
			for(int j = 0; j < characteristics.Count; j++)
			{
				Abstraction abstr = abstractions[abstractions.Keys.ToList()[i]];
				float abstrWeight = abstr.DimensionWeight(layouter.GetRequiredSpatialDimension());

				float increment = samplesDeltaMeasures[i][j] * characteristics[j].weight;

				if(abstrWeight > 0)
				{
					val += increment;
				}
				else
				{
					val = float.MaxValue;
				}
			}

			finalValues.Add((val + (1f/numOfItems) ));
			finalValuesTmp.Add((val + (1f/numOfItems) ));
		}
		
		finalValues.Sort ();
		
		int resultIndex = finalValuesTmp.IndexOf (finalValues [0]);
		
		return abstractions.Keys.ToList () [resultIndex];
	}

	float measureRepresentationCharacteristic (int representationID, int characteristicsID, int samplesCount)
	{
		Characteristic characteristic = characteristics [characteristicsID];
		samplesRepreGO [representationID].SetActive (true);
		
		List<float> sampleMeasures = new List<float> ();
		for(int i = 0; i < samplesCount; i++)
		{
			samplesRepre[representationID * samplesCount + i].gameObject.SetActive(true);
			
			sampleMeasures.Add(characteristic.getMeasurementValue(samplesRepre[representationID * samplesCount + i]));
			
			samplesRepre[representationID * samplesCount + i].gameObject.SetActive(false);
		}
		sampleMeasures.Sort ();

		string strOut = "" + samplesRepreGO [representationID].name + " - " + characteristic.name + ": ";
		float means = 0;
		foreach (float val in sampleMeasures)
		{
			means += val;
			strOut += val.ToString("F6") + " ";
		}
		means /= sampleMeasures.Count;
		
		float deviation = 0;
		foreach (float val in sampleMeasures)
		{
			deviation += Mathf.Pow(val - means, 2f);
		}
		deviation = Mathf.Sqrt(deviation / sampleMeasures.Count);
		
		float median = sampleMeasures [sampleMeasures.Count / 2];
		
		strOut += "\nmeans :" + means + " median :" + median + " standard deviation :" + deviation;
		
		samplesRepreGO [representationID].SetActive (false);

		return means;
	}

	public void UpdateLayouter(CVType nComparativeType, SPType nSuperpositionType, int nVisualSpaceDim, int nParametersDim, List<ParameterNameValue> parameters)
	{
		Camera.main.transform.position = new Vector3 (0, 0, -300);

		if (GameObject.Find ("Composition") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition"));
		}

		comparativeType = nComparativeType;
		visualSpaceDim  = nVisualSpaceDim;
		parametersDim   = nParametersDim;

		switch (comparativeType)
		{
			case CVType.Juxtaposition:
			{
				if(parametersDim == 1)
				{
					layouter = new LayouterStripes();
					(layouter as LayouterStripes).mainParameter = ensemble.getParameter(1);
					(layouter as LayouterStripes).minValue      = ensemble.parametersInfo[(layouter as LayouterStripes).mainParameter].minValue;
					(layouter as LayouterStripes).maxValue      = ensemble.parametersInfo[(layouter as LayouterStripes).mainParameter].maxValue;
					(layouter as LayouterStripes).count         = ensemble.parametersInfo[(layouter as LayouterStripes).mainParameter].count;
					(layouter as LayouterStripes).delta         = ensemble.structuresSize.x * 0.5f;
				}
				else if(parametersDim == 2 && ensemble.parametersInfo.Count >= 2)
				{
					layouter = new LayouterBoxes();
					
					(layouter as LayouterBoxes).mainParameter = ensemble.getParameter(1);
					(layouter as LayouterBoxes).mCount        = ensemble.parametersInfo[(layouter as LayouterBoxes).mainParameter].count;
					(layouter as LayouterBoxes).mMinValue     = ensemble.parametersInfo[(layouter as LayouterBoxes).mainParameter].minValue;
					(layouter as LayouterBoxes).mMaxValue     = ensemble.parametersInfo[(layouter as LayouterBoxes).mainParameter].maxValue;
					(layouter as LayouterBoxes).mDelta        = ensemble.structuresSize.x * 0.5f;
					
					(layouter as LayouterBoxes).secParameter = ensemble.getParameter(2);
					(layouter as LayouterBoxes).sCount       = ensemble.parametersInfo[(layouter as LayouterBoxes).secParameter].count;
					(layouter as LayouterBoxes).sMinValue    = ensemble.parametersInfo[(layouter as LayouterBoxes).secParameter].minValue;
					(layouter as LayouterBoxes).sMaxValue    = ensemble.parametersInfo[(layouter as LayouterBoxes).secParameter].maxValue;
					(layouter as LayouterBoxes).sDelta       = ensemble.structuresSize.y * 0.5f;
				}
				else
				{
					Debug.Log("Not permitted combination! Yet!");
				}
				break;
			}
			case CVType.Superposition:
			{
				if(parametersDim == 1)
				{
					layouter = new LayouterSuperPosition();
					(layouter as LayouterSuperPosition).mainParameter = ensemble.getParameter(1);
					(layouter as LayouterSuperPosition).minValue      = ensemble.parametersInfo[(layouter as LayouterSuperPosition).mainParameter].minValue;
					(layouter as LayouterSuperPosition).maxValue      = ensemble.parametersInfo[(layouter as LayouterSuperPosition).mainParameter].maxValue;
					(layouter as LayouterSuperPosition).count         = ensemble.parametersInfo[(layouter as LayouterSuperPosition).mainParameter].count;
					(layouter as LayouterSuperPosition).delta         = 0.5f;
					(layouter as LayouterSuperPosition).type          = nSuperpositionType;
				}
				else
				{
					Debug.Log("Not permitted combination! Yet!");
				}
				break;
			}
			case CVType.Hybrid:
			{
				if(parametersDim == 2 && ensemble.parametersInfo.Count >= 2)
				{
					layouter = new LayouterSuperStripes();
					
					(layouter as LayouterSuperStripes).mainParameter = ensemble.getParameter(1);
					(layouter as LayouterSuperStripes).mCount        = ensemble.parametersInfo[(layouter as LayouterSuperStripes).mainParameter].count;
					(layouter as LayouterSuperStripes).mMinValue     = ensemble.parametersInfo[(layouter as LayouterSuperStripes).mainParameter].minValue;
					(layouter as LayouterSuperStripes).mMaxValue     = ensemble.parametersInfo[(layouter as LayouterSuperStripes).mainParameter].maxValue;
					(layouter as LayouterSuperStripes).mDelta        = ensemble.structuresSize.x * 0.5f;
					
					(layouter as LayouterSuperStripes).secParameter = ensemble.getParameter(2);
					(layouter as LayouterSuperStripes).sCount       = ensemble.parametersInfo[(layouter as LayouterSuperStripes).secParameter].count;
					(layouter as LayouterSuperStripes).sMinValue    = ensemble.parametersInfo[(layouter as LayouterSuperStripes).secParameter].minValue;
					(layouter as LayouterSuperStripes).sMaxValue    = ensemble.parametersInfo[(layouter as LayouterSuperStripes).secParameter].maxValue;
					(layouter as LayouterSuperStripes).sDelta       = 0.5f;
					(layouter as LayouterSuperStripes).type         = nSuperpositionType;
				}
				else if(parametersDim == 3 && ensemble.parametersInfo.Count >= 3)
				{
					layouter = new LayouterSuperBoxes();
					
					(layouter as LayouterSuperBoxes).mainParameter = ensemble.getParameter(1);
					(layouter as LayouterSuperBoxes).mCount        = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).mainParameter].count;
					(layouter as LayouterSuperBoxes).mMinValue     = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).mainParameter].minValue;
					(layouter as LayouterSuperBoxes).mMaxValue     = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).mainParameter].maxValue;
					(layouter as LayouterSuperBoxes).mDelta        = ensemble.structuresSize.x * 0.5f;
					
					(layouter as LayouterSuperBoxes).secParameter = ensemble.getParameter(2);
					(layouter as LayouterSuperBoxes).sCount       = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).secParameter].count;
					(layouter as LayouterSuperBoxes).sMinValue    = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).secParameter].minValue;
					(layouter as LayouterSuperBoxes).sMaxValue    = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).secParameter].maxValue;
					(layouter as LayouterSuperBoxes).sDelta       = ensemble.structuresSize.y * 0.5f;
					
					(layouter as LayouterSuperBoxes).thirdParameter = ensemble.getParameter(2);
					(layouter as LayouterSuperBoxes).tCount       = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).thirdParameter].count;
					(layouter as LayouterSuperBoxes).tMinValue    = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).thirdParameter].minValue;
					(layouter as LayouterSuperBoxes).tMaxValue    = ensemble.parametersInfo[(layouter as LayouterSuperBoxes).thirdParameter].maxValue;
					(layouter as LayouterSuperBoxes).tDelta       = 0.5f;

					(layouter as LayouterSuperBoxes).type         = nSuperpositionType;
				}
				else
				{
					Debug.Log("Not permitted combination! Yet!");
				}
				break;
			}
		}

		if (visualSpaceDim == 3)
		{
			Camera.main.gameObject.GetComponent<MouseCameraControl> ().pitch.activate = true;
			Camera.main.gameObject.GetComponent<MouseCameraControl> ().yaw.activate   = true;
		}
		else
		{
			Camera.main.gameObject.GetComponent<MouseCameraControl> ().pitch.activate = false;
			Camera.main.gameObject.GetComponent<MouseCameraControl> ().yaw.activate   = false;
		}

		layouter.axisParameters = parameters;

		string abstractionName = pickupAbstractionNew();
		int numOfElements = Mathf.FloorToInt((Camera.main.orthographicSize * 2f) / (abstractions[abstractionName].scale * 300f));

		if (numOfElements == 0) numOfElements = 5;
		layouter.PlacePreview (numOfElements);

		wasLayoutChanged     = true;
		isCompositionCreated = false;
	}

	public void compose()
	{
		if (layouter == null)
			return;

		Camera.main.transform.position = new Vector3 (0, 0, -300);

		if (GameObject.Find ("Composition") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition"));
		}
		compositionGameObject = new GameObject("Composition");
		
		layouter.DestroyGameObjects ();

		layouter.PlaceRepresentations (ensemble, abstractions[pickupAbstraction ()], compositionGameObject);

		wasLayoutChanged     = false;
		isCompositionCreated = true;
	}

	public void composeNew()
	{
		if (layouter == null)
			return;
		
		Camera.main.transform.position = new Vector3 (0, 0, -300);
		
		if (GameObject.Find ("Composition") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition"));
		}
		compositionGameObject = new GameObject("Composition");
		
		layouter.DestroyGameObjects ();

		layouter.PlaceRepresentations (ensemble, abstractions[pickupAbstractionNew ()], compositionGameObject);
		
		wasLayoutChanged     = false;
		isCompositionCreated = true;
	}

	public void Clear()
	{
		// pomazat vsetko
		foreach (GameObject go in samplesRepreGO)
		{
			samplesRepreGO.Remove(go);
			GameObject.DestroyImmediate(go);
		}

		if (GameObject.Find ("Composition Preview") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition Preview"));
		}

		if (GameObject.Find ("Composition") != null)
		{
			GameObject.DestroyImmediate(GameObject.Find ("Composition"));
		}

		// poresetovat vsetko
		ensemble.Clear();

		samplesIDs.Clear ();
		samplesRepre.Clear ();
		samplesRepreGO.Clear ();
		samplesAbsoluteMeasures.Clear();
		samplesDeltaMeasures.Clear();
		samplesDeltaMeasuresDic.Clear ();

		GameObject.DestroyImmediate (compositionGameObject);

		isInitialized        = false;
		isLoaded             = false;
		wasLayoutChanged     = false;
		isCompositionCreated = false;
	}
}
