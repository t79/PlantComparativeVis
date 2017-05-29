using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/GrayscaleBinary")]
public class GrayscaleBinary : ImageEffectBase
{
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit (source, destination, material);
	}
}
