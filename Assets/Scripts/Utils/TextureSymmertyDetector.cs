using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Wintellect.PowerCollections;

public class TextureSymmertyDetector
{
	private Color  backgroundValue = Color.white; //0
	private int    resolution      = 64; // angular resolution
	private int    radiusCount     = 10;
	public float   scoreThreshold  = 0.95f;
	private double angularAliasing = Math.PI/18; //10 degrees

	public double maxRadius;
	
	public Symmetry detectSymmetry(Texture2D image)
	{
		int width  = image.width;
		int height = image.height;

		Color[] pixels = image.GetPixels();
		
		return detectSymmetry(width, height, pixels);
	}

	public Symmetry detectSymmetry(int width, int height, Color[] pixels)
	{	
		
		//find the centroid
		double centerX;
		double centerY;
		{
			int count = 0;
			int sumX  = 0;
			int sumY  = 0;
			{
				int offset = 0;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						Color b = pixels[offset]; 
						if (b != backgroundValue)
						{
							sumX += x;
							sumY += y;
							count++;
						}
						offset++;
					}
				}
			}
			centerX = sumX / (double) count;
			centerY = sumY / (double) count;
		}
		
		//compute the maximum radius if necessary
		maxRadius = 0;
		{
			int cx = (int) System.Math.Round(centerX);
			int cy = (int) System.Math.Round(centerY);
			int furthestD = 0;
			//TODO this could be heavily optimized
			int offset = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color b = pixels[offset]; 
					if (b != backgroundValue)
					{
						int d = (x-cx)*(x-cx)+(y-cy)*(y-cy);
						if (d > furthestD)
						{
							furthestD = d;
						}
					}
					offset++;
				}
			}
			maxRadius = System.Math.Sqrt((double)furthestD);
		}
		
		//choose the radii
		double[] radii = new double[radiusCount];
		double radiusQuotient = radiusCount + 1;
		for (int i = 0; i < radiusCount; i++)
		{
			radii[i] = (i + 1) * maxRadius / radiusQuotient;
		}
		
		//identify the angles
		double[] angles = judge(width, height, pixels, centerX, centerY, radii);
		
		//return an object
		Symmetry symmetry = new Symmetry(centerX, centerY, angles);
		
		return symmetry;
	}
	
	private double[] judge(int width, int height, Color[] pixels, double centerX, double centerY, double[] radii) {
		
		//establish state and constants
		int[]   scores  = new int[resolution];
		Color[] samples = new Color[resolution * 2];
		double  alpha   = System.Math.PI / resolution;
		double  beta    = alpha / 2.0;
		
		//take samples at each radius
		foreach (double radius in radii)
		{
			for (int i = -resolution; i < resolution; i++)
			{
				double theta = beta + alpha * i;
				double x = centerX + radius * System.Math.Cos(theta);
				double y = centerY - radius * System.Math.Sin(theta);
				int coordX = (int) System.Math.Round(x);
				int coordY = (int) System.Math.Round(y);
				if (coordX < 0 || coordX >= width || coordY < 0 || coordY >= height)
				{
					samples[i + resolution] = backgroundValue;
				}
				else
				{
					int offset = coordX + width * coordY;
					Color b = pixels[offset]; 
					samples[i + resolution] = b;
				}
			}
			
			score(samples, scores);
		}
		
		//identify candidates
		int benchmark = (int)Mathf.Round(scoreThreshold * 2 * resolution * radii.Count());
		List<int> candidates = new List<int>();
		for (int i = 0; i < scores.Count(); i++)
		{
			int j = i == 0 ? scores.Count() - 1 : i -1;
			int k = i == scores.Count() - 1 ? 0 : i + 1;
			int score = scores[i];
			if (score >= benchmark && score >= scores[j] && score >= scores[k])
			{
				candidates.Add(i);
			}
		}
		
		//merge adjacent angles
		//TODO address merging of 'wrap-around' adjacent angles
		OrderedSet<AngularIndex> indices = new OrderedSet<AngularIndex>();
		//SortedSet<AngularIndex> indices = new TreeSet<AngularIndex>();
		int space = (int) Math.Ceiling(angularAliasing / alpha);
		int first = -1;
		int weightedSum = -1;
		int directSum = -1;
		int count = -1;
		foreach(int index in candidates)
		//for (Iterator<Integer> i = candidates.iterator(); i.hasNext();)
		{
			//int index = i.next();
			if (first != -1 && index - first < space)
			{
				int score = scores[index];
				weightedSum += score * index;
				directSum += score;
				count += 1;
			}
			else
			{
				if (first != -1)
				{
					AngularIndex angularIndex = new AngularIndex(first, weightedSum / directSum, directSum / count);
					indices.Add( angularIndex );
				}
				first = index;
				int score = scores[index];
				weightedSum = score * index;
				directSum = score;
				count = 1;
			}
		}
		if (first != -1) {
			AngularIndex angularIndex = new AngularIndex(first, weightedSum / directSum, directSum / count);
			indices.Add( angularIndex );
		}
		
		//convert to radians
		double[] angles = new double[ indices.Count() ];
		List<AngularIndex> listIndices = indices.ToList ();

		for (int i = 0; i < angles.Count(); i++)
		{
			AngularIndex index = listIndices[i];
			angles[i] = beta + alpha * (index.index - resolution);
		}
		
		return angles;
	}
	
	private void score(Color[] samples, int[] scores)
	{
		for (int j = 0; j < scores.Count(); j++)
		{
			for (int i = 0; i < samples.Count(); i++)
			{
				int f = j + i;
				if (f >= samples.Count()) f -= samples.Count();
				int b = j - i - 1;
				if (b < 0) b += samples.Count();
				if (samples[f] == samples[b])  scores[j]++;
			}
		}
	}
}
