using UnityEngine;
using System.Collections;

public class Symmetry
{	
	private double   centerX;
	private double   centerY;
	private double[] angles;
	
	public Symmetry(double centerX, double centerY, double[] angles)
	{
		this.centerX = centerX;
		this.centerY = centerY;
		this.angles  = angles;
	}

	public double getCenterX()
	{
		return centerX;
	}

	public double getCenterY()
	{
		return centerY;
	}

	public double[] getAngles()
	{
		return angles;
	}

	public override string ToString()
	{
		string str = "(";
		str += centerX.ToString("F") + ", " + centerY.ToString("F") + ") [";

		for (int i = 0; i < angles.Length; i++)
		{
			if (i != 0) str += ", ";
			str += angles[i].ToString("F4");
		}
		str += "]";

		return str;
	}
}