using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Grayscale")]
public class Grayscale : ImageEffectBase
{
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit (source, destination, material);
	}
}