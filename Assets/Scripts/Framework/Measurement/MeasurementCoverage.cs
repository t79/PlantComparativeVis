using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementCoverage : Measurement
{
	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;

		Vector4 structSize = getStructureSize (repre.goList);

		Vector2 minPoint       = new Vector2 (structSize.x, structSize.y);
		Vector2 maxPoint       = new Vector2 (structSize.z, structSize.w);
		Vector2 ivySize        = maxPoint - minPoint;
		float   cameraSize     = Mathf.Max (ivySize.x, ivySize.y) / 2f;
		Vector3 cameraPosition = (maxPoint + minPoint) / 2f;
		cameraPosition.z       = -10f;
		
		int   texSize = 1024;
		float boxSize = 1f / (float)texSize;
		
		RenderTexture renderTexture     = new RenderTexture(texSize, texSize, 0);
		RenderTexture renderTextureTemp = null;

		GameObject camera         = new GameObject("Camera");
		camera.transform.position = cameraPosition;
		
		camera.AddComponent<Camera> ();
		camera.GetComponent<Camera> ().backgroundColor  = Color.white;
		camera.GetComponent<Camera> ().orthographic     = true;
		camera.GetComponent<Camera> ().orthographicSize = cameraSize;
		camera.GetComponent<Camera> ().targetTexture    = renderTexture;

		camera.AddComponent<GrayscaleBinary> ();
		camera.GetComponent<GrayscaleBinary> ().shader = Shader.Find("Hidden/GrayscaleBinary");

		renderTextureTemp    = RenderTexture.active;
		RenderTexture.active = renderTexture;
		camera.GetComponent<Camera> ().Render ();
		
		Texture2D texture  = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode   = TextureWrapMode.Clamp;
		
		texture.ReadPixels( new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();

		TextureCount.Count (texture);
		List<Vector2> logValues = new List<Vector2> ();
		logValues.Add (new Vector2 (Mathf.Log10 (1f / boxSize), Mathf.Log10 ((float)TextureCount.boxCount)));
		
		while (texture.width > 8)
		{
			TextureScale.Half(texture);
			boxSize *= 2;

			if(TextureScale.newBoxCount == 0)
			{
				UnityEditor.EditorApplication.isPlaying = false;
			}

			logValues.Add (new Vector2 (Mathf.Log10 (1f / boxSize), Mathf.Log10 ((float)TextureScale.newBoxCount)));
		}

		float averageSlope = 0f;
		for (int i = 0; i < logValues.Count - 1; i++)
		{
			averageSlope += (logValues[i].y - logValues[i + 1].y) / (logValues[i].x - logValues[i + 1].x);
		}
		
		averageSlope /= (logValues.Count - 1);

		RenderTexture.active = renderTextureTemp;
		GameObject.DestroyImmediate (camera);

		return averageSlope;
	}

	Vector4 getStructureSize (List<GameObject> goList)
	{
		Vector4 minMaxPoint = new Vector4 (999f, 999f, -999f, -999f);

		for (int i = 0; i < goList.Count; i++)
		{
			if(goList[i].GetComponent<Renderer>() != null)
			{
				if(goList[i].GetComponent<Renderer>().bounds.min.x < minMaxPoint.x)
					minMaxPoint.x = goList[i].GetComponent<Renderer>().bounds.min.x;
				
				if(goList[i].GetComponent<Renderer>().bounds.min.y < minMaxPoint.y)
					minMaxPoint.y = goList[i].GetComponent<Renderer>().bounds.min.y;
				
				if(goList[i].GetComponent<Renderer>().bounds.max.x > minMaxPoint.z)
					minMaxPoint.z = goList[i].GetComponent<Renderer>().bounds.max.x;
				
				if(goList[i].GetComponent<Renderer>().bounds.max.y > minMaxPoint.w)
					minMaxPoint.w = goList[i].GetComponent<Renderer>().bounds.max.y;
			}
		}

		return minMaxPoint;
	}
}
