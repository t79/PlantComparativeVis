﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleCity : MonoBehaviour
{
	public string path = @"Ensemble.city\ensemble3D.xml";
	
	public CVType comparativeType   = CVType.Hybrid;
	public SPType superpositionType = SPType.Transparent;
	public int    visualSpaceDim    = 2;
	public int    parametersDim     = 3;
	
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

		composition.abstractions ["graphSimplifier"]   = new AbstractionGraphSimplifier();
		composition.characteristics.Add(new Characteristic("roadDensity",             new MeasurementRoadDensity(), 0.5f));
		 
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
		Camera.main.transform.rotation = Quaternion.identity;
		
		composition.isInitialized = true;
	}
	
	public void PreloadDeformationsAndMeasurements()
	{
		if (composition != null && composition.isInitialized)
		{
			composition.GenerateSample(10);
			composition.PreloadDeformations();
			composition.PreloadMeasurements();
			
			composition.isLoaded = true;
		}
	}
	
	public void UpdateComposition()
	{
		composition.UpdateLayouter (comparativeType, superpositionType, visualSpaceDim, parametersDim, parameters);
	}
	
	public void FinalComposition()
	{
		composition.compose();
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

}

