using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Shadow")]
public class Shadow : ImageEffectBase
{
	public Texture2D shadowTexture;

	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetTexture ("_ShadowTex", shadowTexture);
		Graphics.Blit (source, destination, material);
	}
}
