using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MeasurementSymmetryAngular : Measurement
{
	public override float Evaluate(Representation representation)
	{
		int boxSize = 1;
		int texSize = 512;
		
		RenderTexture renderTexture = new RenderTexture(texSize, texSize, 0);
		RenderTexture renderTextureTemp = RenderTexture.active;

		GameObject camera         = new GameObject("Camera");
		camera.transform.position = new Vector3 (0, 150f, -290f);
		
		camera.AddComponent<Camera> ();
		camera.GetComponent<Camera> ().backgroundColor  = Color.white;
		camera.GetComponent<Camera> ().orthographic     = true;
		camera.GetComponent<Camera> ().orthographicSize = 150;
		camera.GetComponent<Camera> ().targetTexture    = renderTexture;
		camera.AddComponent<GrayscaleBinary> ();
		Shader shd = Shader.Find("Hidden/GrayscaleBinary");
		camera.GetComponent<GrayscaleBinary> ().shader = shd;

		RenderTexture.active = camera.GetComponent<Camera> ().targetTexture;
		camera.GetComponent<Camera> ().Render ();
		
		
		Texture2D texture  = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode   = TextureWrapMode.Clamp;
		
		texture.ReadPixels( new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();

		GameObject debugPlane = GameObject.Find ("DebugPlane") as GameObject;
		debugPlane.GetComponent<Renderer> ().material.mainTexture = texture;

		for(int i = 0; i < 3; i++ )
		{
			TextureScale.Half(texture);
			
			boxSize *= 2;
		}
		
		// symmetry - new
		float minTreshold = 0.0f;
		float maxTreshold = 100.0f;
		float tryTreshold = 50.0f;
		for(int j = 0; j < 8; j++)
		{
			tryTreshold = (minTreshold + maxTreshold) / 2;
			
			TextureSymmertyDetector tsd = new TextureSymmertyDetector();
			tsd.scoreThreshold = 0.8f + (tryTreshold / 500.0f);
			Symmetry symmetry = tsd.detectSymmetry(texture);
			
			bool tryTresholdValue = symmetry.getAngles().Length > 0;
			
			if(tryTresholdValue)
			{
				minTreshold = tryTreshold;
			}
			else
			{
				maxTreshold = tryTreshold;
			}
		}

		RenderTexture.active = renderTextureTemp;
		GameObject.DestroyImmediate (camera);
		return tryTreshold / 100f;
	}
}
