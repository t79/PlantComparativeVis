using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur")]
public class Blur : PostEffectsBase
{
	[Range(0.0f, 10.0f)]
	public float blurSize = 1.0f;
	
	public Shader    blurShader;
	private Material blurMaterial = null;
	
	public override bool CheckResources ()
	{
		CheckSupport (false);	
		
		blurMaterial = CheckShaderAndCreateMaterial (blurShader, blurMaterial);

		return isSupported;
	}
	
	public void OnDisable()
	{
		if(blurMaterial)
			DestroyImmediate (blurMaterial);
	}
	
	public void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		if(CheckResources() == false)
		{
			Graphics.Blit (source, destination);
			return;
		}
		
		blurMaterial.SetFloat ("_Parameter", blurSize);

		Graphics.Blit (source, destination, blurMaterial);
		
		// vertical blur
		RenderTexture rt2 = RenderTexture.GetTemporary (source.width, source.height, 0, source.format);
		rt2.filterMode = FilterMode.Bilinear;
		Graphics.Blit (source, rt2, blurMaterial, 0);
		
		// horizontal blur
		Graphics.Blit (rt2, destination, blurMaterial, 1);
		RenderTexture.ReleaseTemporary (rt2);
	}	
}