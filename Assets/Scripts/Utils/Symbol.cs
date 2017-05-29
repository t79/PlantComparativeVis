using System;
using UnityEngine;
//using UnityEditor;
using System.Collections.Generic;

[Serializable]
public class Symbol
{
	public int    symId;
	public string symName;
	
	public Dictionary<string, Parameter> parameters;

	public Symbol(string nSymName)
	{
		symName = nSymName;

		parameters = new Dictionary<string, Parameter> ();
	}

	public Symbol(Symbol nSymbol)
	{
		symName = nSymbol.symName;

		parameters = new Dictionary<string, Parameter> ();

		foreach(KeyValuePair<string, Parameter> param in nSymbol.parameters)
		{
			parameters[param.Key] = param.Value.Clone();
		}
	}

	public static bool operator ==(Symbol x, Symbol y) 
	{
		if (x.symName == null || y.symName == null) return false;
		return x.symName == y.symName;
	}

	public static bool operator !=(Symbol x, Symbol y) 
	{
		if (x.symName == null || y.symName == null) return false;
		return x.symName != y.symName;
	}

	public override bool Equals(System.Object obj)
	{
		if (obj == null)
		{
			return false;
		}

		Symbol p = obj as Symbol;
		if ((System.Object)p == null)
		{
			return false;
		}

		return (symName == p.symName);
	}
	
	public override string ToString()
	{
		string result = symName;
		bool   first  = true;

		result += "[";

		foreach (KeyValuePair<string, Parameter> parameter in parameters)
		{
			if(!first)
			{
				result += ", " + parameter.Value.ToString();
			}
			else
			{
				result += parameter.Value.ToString();
				first = !first;
			}

		}

		result += "]";

		return result;
	}

	public override int GetHashCode()
	{
		return symName.GetHashCode ();
	}
}