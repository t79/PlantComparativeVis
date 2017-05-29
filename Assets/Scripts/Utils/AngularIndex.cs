using UnityEngine;
using System;
using System.Collections;

public class AngularIndex : IComparable
{	
	public int    id;
	public double index;
	public double score;
	
	public AngularIndex(int id, double index, double score)
	{
		this.id    = id;
		this.index = index;
		this.score = score;
	}
	
	public int CompareTo(object obj)
	{
		if (obj == null) return 1;

		AngularIndex otherAI = obj as AngularIndex;

		if (otherAI != null)
		{
			if (this.score < otherAI.score) return -1;
			if (this.score > otherAI.score) return  1;
			if (this.id    < otherAI.id)    return -1;
			if (this.id    > otherAI.id)    return  1;
		}
		else 
		{
			throw new ArgumentException("Object is not a Symmetry");
		}

		return 0;
	}
}