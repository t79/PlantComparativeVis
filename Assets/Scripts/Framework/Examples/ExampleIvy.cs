using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class ParameterNameValue
{
	public string name = "";
	public float value = 0;

	public ParameterNameValue(string nName, float nValue)
	{
		name  = nName;
		value = nValue;
	}
}

public class ExampleIvy : MonoBehaviour
{
	public string path = @"Ensemble.ivy\ensemble2D.xml";

	public CVType comparativeType   = CVType.Juxtaposition;
	public SPType superpositionType = SPType.Transparent;
	public int    visualSpaceDim    = 2;
	public int    parametersDim     = 1;


	public int[] parametersOrder;

	public Composition              composition = null;
	public List<ParameterNameValue> parameters  = new List<ParameterNameValue> ();

	void Start ()
	{
	}

	public void Init()
	{
		composition = new Composition ();
		composition.ensemble = new Ensemble (path);
		composition.ensemble.loadEnsemble ();

		for (int i = 0; i < 10; i++)
		{
			composition.abstractions ["squeeze" + i.ToString()]      = new AbstractionSqueeze     (0.8f - (float)i * 0.05f);
			composition.abstractions ["scale1D" + i.ToString()]      = new AbstractionScale1D     (0.8f - (float)i * 0.05f);
			composition.abstractions ["scale2D" + i.ToString()]      = new AbstractionScale2D     (0.8f - (float)i * 0.05f);
			composition.abstractions ["squeezeScale" + i.ToString()] = new AbstractionSqueezeScale(0.8f - (float)i * 0.05f);

			composition.abstractions ["squeeze"      + i.ToString()].name = "squeeze"      + i.ToString();
			composition.abstractions ["scale1D"      + i.ToString()].name = "scale1D"      + i.ToString();
			composition.abstractions ["scale2D"      + i.ToString()].name = "scale2D"      + i.ToString();
			composition.abstractions ["squeezeScale" + i.ToString()].name = "squeezeScale" + i.ToString();
		}

		composition.abstractions ["holeSnap"] = new AbstractionHoleSnap ();
		composition.abstractions ["holeSnap"].name = "holeSnap";

		composition.characteristics.Add(new Characteristic("coverage",           new MeasurementCoverage(),     0.6f));
		composition.characteristics.Add(new Characteristic("holesCount",         new MeasurementHolesCount(),   0.3f));
		composition.characteristics.Add(new Characteristic("symmetrySpatial", new MeasurementSymmetrySpatial(), 0.1f));

		parametersOrder = new int[composition.ensemble.parametersInfo.Count];
		for (int i = 0; i < parametersOrder.Length; i++)
		{
			parametersOrder[i] = i;
		}

		foreach (KeyValuePair<string, ParameterInfo> param in composition.ensemble.parametersInfo)
		{
			parameters.Add(new ParameterNameValue(param.Key, param.Value.minValue));
		}

		GameObject uiGO   = GameObject.Find ("UI") as GameObject;
		GameObject xPanel = uiGO.transform.Find ("X Axis Panel").gameObject;
		GameObject yPanel = uiGO.transform.Find ("Y Axis Panel").gameObject;
		GameObject zPanel = uiGO.transform.Find ("Z Axis Panel").gameObject;
		
		xPanel.SetActive(false);
		yPanel.SetActive(false);
		zPanel.SetActive(false);

		Camera.main.transform.position = new Vector3 (0, 0, -300);

		composition.isInitialized = true;
	}

	public void PreloadDeformationsAndMeasurements()
	{
		if (composition != null && composition.isInitialized)
		{
			composition.GenerateSample(10);
			composition.PreloadDeformations();
			composition.PreloadMeasurementsNew();

			composition.isLoaded = true;
		}
	}

	public void UpdateComposition()
	{
		composition.UpdateLayouter (comparativeType, superpositionType, visualSpaceDim, parametersDim, parameters);
	}

	public void FinalComposition()
	{
		composition.composeNew();
	}

	public string[] AvailableParameters()
	{
		string[] result = new string[composition.ensemble.parametersInfo.Count];
		int i = 0;
		foreach (KeyValuePair<string, ParameterInfo> pi in composition.ensemble.parametersInfo)
		{
			result[i] = pi.Value.name;
			i++;
		}
		return result;
	}

	public void MoveUp(int i)
	{
		if (i == 0)
			return;

		ParameterNameValue item = parameters [i];

		parameters.RemoveAt (i);
		parameters.Insert (i - 1, item);
	}

	public void MoveDown(int i)
	{
		if (i == parameters.Count - 1)
			return;

		ParameterNameValue item = parameters [i];
		
		parameters.RemoveAt (i);
		parameters.Insert (i + 1, item);
	}

	public void Clear()
	{
		composition.Clear ();
	}

	public void ExportMeasurementValues()
	{ 
		string csvFile = "AbstractionName,ScaleValue,Coverage,Holes,Symmetry\n";

		foreach (KeyValuePair<string, List<float>> item in composition.samplesDeltaMeasuresDic)
		{
			string line = item.Key;

			int floatIndex = -1;

			if(int.TryParse(line[line.Length - 1].ToString(), out floatIndex))
			{
				line += "," + (0.8f - (floatIndex * 0.05f)).ToString() + ",";
			}

			foreach(float value in item.Value)
			{
				line += value.ToString() + ",";
			}

			line = line.Remove(line.Length-1);
			line += '\n';

			csvFile += line;
		}

		StreamWriter writer; 
		FileInfo t = new FileInfo("measurements.csv");
		
		if(!t.Exists) 
		{ 
			writer = t.CreateText(); 
		} 
		else 
		{ 
			t.Delete(); 
			writer = t.CreateText(); 
		} 
		writer.Write(csvFile);
		writer.Close();
	}
}
