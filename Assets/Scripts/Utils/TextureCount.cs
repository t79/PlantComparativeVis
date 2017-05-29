using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureCount
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
	private static int     width;
	private static int     finishCount;
	public  static int     boxCount;
	private static Mutex   mutex;
	
	public static void Count (Texture2D tex)
	{
		texColors = tex.GetPixels();
		width = tex.width;
		int cores = Mathf.Min(SystemInfo.processorCount, tex.height);
		
		int slice = tex.height / cores;
		
		finishCount = 0;
		boxCount = 0;
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
				ParameterizedThreadStart ts = new ParameterizedThreadStart(_Count);
				Thread thread = new Thread(ts);
				thread.Start(threadData);
			}
			threadData = new ThreadData(slice*i, tex.height);
			_Count(threadData);
			
			while (finishCount < cores)
			{
				Thread.Sleep(1);
			}
		}
		else
		{
			ThreadData threadData = new ThreadData(0, tex.height);
			_Count(threadData);
		}
	}
	
	private static void _Count (System.Object obj)
	{
		int        threadBoxCount = 0;
		ThreadData threadData     = (ThreadData) obj;
		
		for (var y = threadData.start; y < threadData.end; y++)
		{
			int imgY = (int)y * width;
			
			for (int x = 0; x < width; x++)
			{	
				if(texColors[imgY + x] == Color.black)
				{
					threadBoxCount++;
				}
			}
		}
		
		mutex.WaitOne();
		finishCount++;
		boxCount += threadBoxCount;
		mutex.ReleaseMutex();
	}
}