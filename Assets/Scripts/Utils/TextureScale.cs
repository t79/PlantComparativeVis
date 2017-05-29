using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScale
{
	public class ThreadData
	{
		public int start;
		public int end;
		public ThreadData (int s, int e) {
			start = s;
			end = e;
		}
	}
	
	private static Color[] texColors;
	private static Color[] newColors;
	private static int width; // w
	private static int newWidth; // w2
	private static int newHeight;
	private static float ratioX;
	private static float ratioY;

	private static int finishCount;
	public static int oldBoxCount;
	public static int newBoxCount;
	private static Mutex mutex;

	public static void Half (Texture2D tex)
	{
		width = tex.width;
		newWidth  = (int)(tex.width / 2);
		newHeight = (int)(tex.height / 2);

		texColors = tex.GetPixels();
		newColors = new Color[newWidth * newHeight];

		var cores = Mathf.Min(SystemInfo.processorCount, newHeight);

		var slice = newHeight / cores;
		
		finishCount = 0;
		oldBoxCount = 0;
		newBoxCount = 0;
		if (mutex == null)
		{
			mutex = new Mutex(false);
		}

		if (cores > 1)
		{
			int i = 0;
			ThreadData threadData;
			for (i = 0; i < cores-1; i++)
			{
				threadData = new ThreadData(slice * i, slice * (i + 1));
				ParameterizedThreadStart ts = new ParameterizedThreadStart(HalfScale);
				Thread thread = new Thread(ts);
				thread.Start(threadData);
			}
			threadData = new ThreadData(slice * i, newHeight);
			HalfScale(threadData);

			while (finishCount < cores)
			{
				Thread.Sleep(1);
			}
		}
		else
		{
			ThreadData threadData = new ThreadData(0, newHeight);
			HalfScale(threadData);
		}
		
		tex.Resize(newWidth, newHeight);
		tex.SetPixels(newColors);
		tex.Apply();
	}
	
	public static void HalfScale (System.Object obj)
	{
		int threadNewBoxCount = 0;
		int threadOldBoxCount = 0;
		ThreadData threadData = (ThreadData) obj;

		for (var y = threadData.start; y < threadData.end; y++)
		{
			int oldY = (int)(2 * y) * width;
			int newY = y * newWidth;

			for (int x = 0; x < newWidth; x++)
			{
				int oldX = 2 * x;

				bool isBlack = false;

				if(texColors[oldY + oldX] == Color.black)
				{
					threadOldBoxCount++;
					isBlack = true;
				}

				if(texColors[oldY + oldX + 1] == Color.black)
				{
					threadOldBoxCount++;
					isBlack = true;
				}

				if(texColors[oldY + width + oldX] == Color.black)
				{
					threadOldBoxCount++;
					isBlack = true;
				}

				if(texColors[oldY + width + oldX + 1] == Color.black)
				{
					threadOldBoxCount++;
					isBlack = true;
				}

				if(isBlack)
				{
					newColors[newY + x] = Color.black;
					threadNewBoxCount++;
				}
				else
				{
					newColors[newY + x] = Color.white;
				}
			}
		}
		
		mutex.WaitOne();
		finishCount++;
		oldBoxCount += threadOldBoxCount;
		newBoxCount += threadNewBoxCount;
		mutex.ReleaseMutex();
	}
}