using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class Pixel
{
	#region Public Properties
	
	public Vector2 Position { get; set; }
	public Color color { get; set; }
	
	#endregion
	
	#region Constructor
	
	public Pixel(Vector2 Position, Color color)
	{
		this.Position = Position;
		this.color    = color;
	}
	
	#endregion
}
