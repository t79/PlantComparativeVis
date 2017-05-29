using System;
using System.Collections.Generic;

[Serializable]
public class Parameter
{
	public string name        = "";
	public string expression  = "";
	public float  value       = 0.0f;

	public Parameter( string nName, float nValue)
	{
		name  = nName;
		value = nValue;
	}

	public Parameter( string nName, float nValue, string nExpression)
	{
		name       = nName;
		value      = nValue;
		expression = nExpression;
	}

	public static bool operator ==(Parameter x, Parameter y) 
	{
		if (x.name == null || y.name == null) return false;
		return x.name == y.name;
	}
	
	public static bool operator !=(Parameter x, Parameter y) 
	{
		if (x.name == null || y.name == null) return false;
		return x.name != y.name;
	}
	
	public override bool Equals(System.Object obj)
	{
		if (obj == null)
		{
			return false;
		}
		
		Parameter p = obj as Parameter;
		if ((System.Object)p == null)
		{
			return false;
		}
		
		return (name == p.name);
	}
	
	public override string ToString()
	{
		return name + "=" + value;
	}
	
	public override int GetHashCode()
	{
		return name.GetHashCode ();
	}

	public Parameter Clone()
	{
		return new Parameter (this.name, this.value, this.expression);
	}
}