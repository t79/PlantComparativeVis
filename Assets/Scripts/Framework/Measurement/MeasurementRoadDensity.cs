using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeasurementRoadDensity : Measurement
{

	public override float Evaluate(Representation representation)
	{
		RepresentationMesh repre = representation as RepresentationMesh;
		
		if (repre == null)
			return 0;

		Vector4 structMinMax   = getStructureSize (repre.goList);
		
		Vector2 minPoint       = new Vector2 (structMinMax.x, structMinMax.y);
		Vector2 maxPoint       = new Vector2 (structMinMax.z, structMinMax.w);
		Vector2 structSize     = maxPoint - minPoint;
		float   cameraSize     = Mathf.Max (structSize.x, structSize.y) / 2f;
		Vector3 cameraPosition = (maxPoint + minPoint) / 2f;
		cameraPosition.z       = -10f;

		int   texSize = 64;
		RenderTexture renderTexture     = new RenderTexture(texSize, texSize, 0);
		RenderTexture renderTextureTemp = null;
		
		GameObject camera         = new GameObject("Camera");
		camera.transform.position = cameraPosition;
		
		camera.AddComponent<Camera> ();
		camera.GetComponent<Camera> ().clearFlags       = CameraClearFlags.Color;
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
		
		RenderTexture.active = renderTextureTemp;
		GameObject.DestroyImmediate (camera);
		
		return (float)TextureCount.boxCount / (float)(texSize * texSize);
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
