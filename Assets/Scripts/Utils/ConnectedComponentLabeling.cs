using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConnectedComponentLabeling
{
	private int[,] _board;
	private Texture2D _input;
	private int _width;
	private int _height;

	public List<Vector2> holesAveragePositions = new List<Vector2> ();
	public List<Vector4> holesBBox             = new List<Vector4> ();
	
	public int Process(Texture2D input, Texture2D output, bool colorify = true, bool colorifyAverage = false)
	{
		_input  = input;
		_width  = input.width;
		_height = input.height;
		_board  = new int[_width, _height];
		
		Dictionary<int, List<Pixel>> patterns = Find();

		holesAveragePositions = AveragePositions (patterns);

		if(colorify)
		{
			foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
			{
				Colorify(output, pattern.Value);
			}
			output.Apply ();
		}

		if(colorifyAverage)
		{
			ColorifyAverage(output);
			output.Apply ();
		}

		return patterns.Count;
	}

	static int CompareHoles(KeyValuePair<int, Vector2> a, KeyValuePair<int, Vector2> b)
	{
		return -1 * a.Key.CompareTo (b.Key);
	}

	List<Vector2> AveragePositions (Dictionary<int, List<Pixel>> patterns)
	{
		List<KeyValuePair<int, Vector2>> positionsToSort = new List<KeyValuePair<int, Vector2>> ();

		List<Vector2> positions = new List<Vector2> ();

		foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
		{
			Vector2 averagePosition = Vector2.zero;
			
			foreach (Pixel pix in pattern.Value)
			{
				averagePosition += pix.Position;
			}
			
			averagePosition /= pattern.Value.Count;
			averagePosition+= new Vector2(0.5f, 0.5f);

			positionsToSort.Add(new KeyValuePair<int, Vector2> (pattern.Value.Count, averagePosition));
		}

		positionsToSort.Sort (CompareHoles);

		foreach (KeyValuePair<int, Vector2> hole in positionsToSort)
		{
			positions.Add(hole.Value);
		}

		return positions;
	}
	
	private Dictionary<int, List<Pixel>> Find()
	{
		int labelCount = 1;
		var allLabels = new Dictionary<int, Label>();
		
		for (int i = 0; i < _height; i++)
		{
			for (int j = 0; j < _width; j++)
			{
				Color currentColor = _input.GetPixel(j, i);
				
				if (currentColor == Color.black)
				{
					continue;
				}
				
				IEnumerable<int> neighboringLabels = GetNeighboringLabels(j, i);
				int currentLabel;
				
				if (!neighboringLabels.Any())
				{
					currentLabel = labelCount;
					allLabels.Add(currentLabel, new Label(currentLabel));
					labelCount++;
				}
				else
				{
					currentLabel = neighboringLabels.Min(n => allLabels[n].GetRoot().Name);
					Label root = allLabels[currentLabel].GetRoot();
					
					foreach (var neighbor in neighboringLabels)
					{
						if (root.Name != allLabels[neighbor].GetRoot().Name)
						{
							allLabels[neighbor].Join(allLabels[currentLabel]);
						}
					}
				}
				
				_board[j, i] = currentLabel;
			}
		}
		
		
		Dictionary<int, List<Pixel>> patterns = AggregatePatterns(allLabels);
		
		return patterns;
	}
	
	private IEnumerable<int> GetNeighboringLabels(int x, int y)
	{
		var neighboringLabels = new List<int>();
		
		for (int i = y - 1; i <= y + 1 && i < _height - 1; i++)
		{
			for (int j = x - 1; j <= x + 1 && j < _width - 1; j++)
			{
				if (i > -1 && j > -1 && _board[j, i] != 0)
				{
					neighboringLabels.Add(_board[j, i]);
				}
			}
		}
		
		return neighboringLabels;
	}
	
	private Dictionary<int, List<Pixel>> AggregatePatterns(Dictionary<int, Label> allLabels)
	{
		var patterns = new Dictionary<int, List<Pixel>>();
		
		for (int i = 0; i < _height; i++)
		{
			for (int j = 0; j < _width; j++)
			{
				int patternNumber = _board[j, i];
				
				if (patternNumber != 0)
				{
					patternNumber = allLabels[patternNumber].GetRoot().Name;
					
					if (!patterns.ContainsKey(patternNumber))
					{
						patterns[patternNumber] = new List<Pixel>();
					}

				Color col = Color.black;
				switch(patternNumber % 7)
				{
				case 0: 
					col = Color.blue;
					break;
				case 1: 
					col = Color.cyan;
					break;
				case 2: 
					col = Color.gray;
					break;
				case 3: 
					col = Color.green;
					break;
				case 4: 
					col = Color.magenta;
					break;
				case 5: 
					col = Color.red;
					break;
				case 6: 
					col = Color.yellow;
					break;
				}
					
				patterns[patternNumber].Add(new Pixel(new Vector2(j, i), col));
				}
			}
		}
		
		return patterns;
	}

	private void Colorify(Texture2D tex, List<Pixel> pattern)
	{
		foreach (Pixel pix in pattern)
		{
			tex.SetPixel((int)pix.Position.x, (int)pix.Position.y, pix.color);
		}
		
		return;
	}

	private void ColorifyAverage(Texture2D tex)
	{
		foreach (Vector2 position in holesAveragePositions)
		{
			tex.SetPixel((int)position.x, (int)position.y, Color.red);
		}
		
		return;
	}
}