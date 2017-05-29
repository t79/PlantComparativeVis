using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementHolesCount : Measurement
{
	public override float Evaluate(Representation representation)
	{
		int texSize = 1024;
		
		RenderTexture renderTexture     = new RenderTexture(texSize, texSize, 0);
		RenderTexture renderTextureTemp = RenderTexture.active;

		GameObject camera         = new GameObject("Camera");
		camera.transform.position = new Vector3 (0, 150f, -300f);

		camera.AddComponent<Camera> ();
		camera.GetComponent<Camera> ().backgroundColor  = Color.white;
		camera.GetComponent<Camera> ().orthographic     = true;
		camera.GetComponent<Camera> ().orthographicSize = 150;
		camera.GetComponent<Camera> ().targetTexture    = renderTexture;
		
		camera.AddComponent<GrayscaleBinary> ();
		camera.GetComponent<GrayscaleBinary> ().shader = Shader.Find("Hidden/GrayscaleBinary");

		RenderTexture.active = renderTexture;
		camera.GetComponent<Camera> ().Render ();
		
		Texture2D texture  = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode   = TextureWrapMode.Clamp;
		
		texture.ReadPixels( new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();


		TextureScale.Half(texture);
		texSize /= 2;

		// texture to copy
		Texture2D tempTex = new Texture2D(texture.width, texture.height);
		tempTex.SetPixels(texture.GetPixels());
		
		// fill
		FloodFill.Fill(tempTex, 0,                 0);
		FloodFill.Fill(tempTex, tempTex.width - 1, 0);
		FloodFill.Fill(tempTex, tempTex.width - 1, tempTex.height - 1);
		FloodFill.Fill(tempTex, 0,                 tempTex.height - 1);
		
		// holes
		ConnectedComponentLabeling ccl = new ConnectedComponentLabeling();
		int holes = ccl.Process(tempTex, texture, false);

		
		RenderTexture.active = renderTextureTemp;
		GameObject.DestroyImmediate (camera);

		return holes * (1f + Mathf.Exp (2f)) / (28.4103f * Mathf.Exp (2f)) + 43.9867f / 28.4103f;
	}
}
