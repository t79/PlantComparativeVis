using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructureParameter
{
	public float value    = 0;
	public int   priority = 0;

	public StructureParameter(float nValue, int nPriority)
	{
		value    = nValue;
		priority = nPriority;
	}
}

public class Structure
{
	public string                                 name;
	public Dictionary<string, StructureParameter> parameters;
	public Representation                         representation;

	public Structure()
	{
		name           = "";
		parameters     = new Dictionary<string, StructureParameter> ();
		representation = null;
	}

	public Structure(string nName, Representation nRepresentation)
	{
		name           = nName;
		representation = nRepresentation;
		parameters     = new Dictionary<string, StructureParameter> ();
	}
}
