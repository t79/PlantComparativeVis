using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodFill
{
	public static void Fill (Texture2D tex, int x, int y)
	{
		Vector2 pt = new Vector2 (x, y);
		Queue<Vector2> q = new Queue<Vector2>();
		q.Enqueue(pt);

		Color targetColor      = Color.white;
		Color replacementColor = Color.black;

		while (q.Count > 0)
		{
			Vector2 n = q.Dequeue();

			if (tex.GetPixel((int)n.x, (int)n.y) != targetColor)
				continue;

			Vector2 w = n, e = new Vector2(n.x + 1, n.y);
			while ((w.x > -1) && (tex.GetPixel((int)w.x, (int)w.y) == targetColor))
			{
				tex.SetPixel((int)w.x, (int)w.y, replacementColor);

				if ((w.y > 0) && (tex.GetPixel((int)w.x, (int)w.y - 1) == targetColor))
					q.Enqueue(new Vector2(w.x, w.y - 1));

				if ((w.y < tex.height - 1) && (tex.GetPixel((int)w.x, (int)w.y + 1) == targetColor))
					q.Enqueue(new Vector2(w.x, w.y + 1));

				w.x--;
			}

			while ((e.x < tex.width ) && (tex.GetPixel((int)e.x, (int)e.y) == targetColor))
			{
				tex.SetPixel((int)e.x, (int)e.y, replacementColor);

				if ((e.y > 0) && (tex.GetPixel((int)e.x, (int)e.y - 1) == targetColor))
					q.Enqueue(new Vector2(e.x, e.y - 1));

				if ((e.y < tex.height - 1) && (tex.GetPixel((int)e.x, (int)e.y + 1) == targetColor))
					q.Enqueue(new Vector2(e.x, e.y + 1));

				e.x++;
			}
		}
		tex.Apply();
	}
}
