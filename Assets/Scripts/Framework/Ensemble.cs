using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ParameterInfo
{
	public string name     = "";
	public int    priority = 0;
	public float  minValue = 0;
	public float  maxValue = 0;
	public int    count    = 0;
	public int    offset   = 0;
}

public class Ensemble
{
	public string                            dataPath       = "";
	public string                            phenomenon     = "none";
	public List<Structure>                   structures     = new List<Structure> ();
	public Dictionary<string, ParameterInfo> parametersInfo = new Dictionary<string, ParameterInfo> ();
	public Vector3                           structuresSize = new Vector3 ();
	public GameObject                        gameObject     = new GameObject ("Ensemble structures");

	public Ensemble (string nDataPath)
	{
		dataPath = nDataPath;
	}

	public void loadEnsemble()
	{
		int count = 0;
		
		XmlDocument doc = new XmlDocument ();
		doc.Load (dataPath);
		XmlNode ensemble = doc.LastChild;
		phenomenon    = ensemble.Attributes["phenomenon"].Value;
		int dimension = System.Convert.ToInt32 (ensemble.Attributes ["dimension"].Value);
		string type   = ensemble.Attributes["type"].Value;
		string test   = ensemble.Attributes["test"].Value;

		XmlNode ensembleParametersXml = ensemble.FirstChild;
		int priority = 1;
		int parameterOffset = 1;
		foreach (XmlNode parameterXml in ensembleParametersXml)
		{
			string parameterName = parameterXml.Attributes["name"].Value;

			parametersInfo[parameterName] = new ParameterInfo();
			parametersInfo[parameterName].name     = parameterName;
			parametersInfo[parameterName].minValue = System.Convert.ToSingle(parameterXml.Attributes["minValue"].Value);
			parametersInfo[parameterName].maxValue = System.Convert.ToSingle(parameterXml.Attributes["maxValue"].Value);
			parametersInfo[parameterName].count    = System.Convert.ToInt32(parameterXml.Attributes["count"].Value);
			parametersInfo[parameterName].priority = priority;
			parametersInfo[parameterName].offset   = parameterOffset;
			priority++;
			parameterOffset *= parametersInfo[parameterName].count;
		}

		if (test == "ivy" && ensemble != null)
		{
			foreach(XmlNode structureXml in ensemble.LastChild.ChildNodes)
			{
				Structure structure = TestLoaders.LoadIvy(count, dimension, type, structureXml);
				structures.Add(structure);
				(structure.representation as RepresentationMesh).gameObject.transform.parent = gameObject.transform;
				count++;

				if(structuresSize.x < structure.representation.size.x)
				{
					structuresSize.x = structure.representation.size.x;
				}

				if(structuresSize.y < structure.representation.size.y)
				{
					structuresSize.y = structure.representation.size.y;
				}

				if(structuresSize.z < structure.representation.size.z)
				{
					structuresSize.z = structure.representation.size.z;
				}
			}
		}


		if (test == "parp" && ensemble != null)
		{
			foreach(XmlNode structureXml in ensemble.LastChild.ChildNodes)
			{
				Structure structure = TestLoaders.LoadParp(count, dimension, type, structureXml);
				structures.Add(structure);
				(structure.representation as RepresentationMesh).gameObject.transform.parent = gameObject.transform;
				count++;
				
				if(structuresSize.x < structure.representation.size.x)
				{
					structuresSize.x = structure.representation.size.x;
				}
				
				if(structuresSize.y < structure.representation.size.y)
				{
					structuresSize.y = structure.representation.size.y;
				}
				
				if(structuresSize.z < structure.representation.size.z)
				{
					structuresSize.z = structure.representation.size.z;
				}
			}
		}

		if (test == "city" && ensemble != null)
		{
			foreach(XmlNode structureXml in ensemble.LastChild.ChildNodes)
			{
				Structure structure = TestLoaders.LoadCity2(count, dimension, type, structureXml);
				structures.Add(structure);
				(structure.representation as RepresentationMesh).gameObject.transform.parent = gameObject.transform;
				count++;
				
				if(structuresSize.x < structure.representation.size.x)
				{
					structuresSize.x = structure.representation.size.x;
				}
				
				if(structuresSize.y < structure.representation.size.y)
				{
					structuresSize.y = structure.representation.size.y;
				}
				
				if(structuresSize.z < structure.representation.size.z)
				{
					structuresSize.z = structure.representation.size.z;
				}
			}
		}
	}

	public string getParameter(int priority)
	{
		foreach (KeyValuePair<string, ParameterInfo> paramInfo in parametersInfo)
		{
			if(paramInfo.Value.priority == priority)
			{
				return paramInfo.Value.name;
			}
		}
		return "";
	}

	public Structure GetStructure(List<KeyValuePair<string, float>> paramsValues)
	{
		List<KeyValuePair<string, int>> indexParams = new List<KeyValuePair<string, int>> ();

		foreach (KeyValuePair<string, float> param in paramsValues)
		{
			ParameterInfo pi = parametersInfo[param.Key];

			double delta = ((double)pi.maxValue - (double)pi.minValue) / (double)(pi.count - 1);
			double index = ((double)param.Value - (double)pi.minValue) / delta;

			indexParams.Add(new KeyValuePair<string, int>(param.Key, Convert.ToInt32(index)));
		}

		return GetStructure (indexParams);
	}

	public Structure GetStructure(List<KeyValuePair<string, int>> paramsCoords)
	{
		int ID = 0;

		foreach (KeyValuePair<string, int> paramCoord in paramsCoords)
		{
			ID += paramCoord.Value * parametersInfo[paramCoord.Key].offset;
		}

		return structures [ID];
	}

	public void Clear()
	{
		dataPath       = "";
		phenomenon     = "none";

		structures     = new List<Structure> ();
		parametersInfo = new Dictionary<string, ParameterInfo> ();
		structuresSize = new Vector3 ();
		GameObject.DestroyImmediate(gameObject);
	}
}
