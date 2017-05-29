using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Treshold")]
public class Treshold : ImageEffectBase
{
	public Texture2D texture;
	public float     treshold;

	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetTexture ( "_texture",  texture  );
		material.SetFloat   ( "_treshold", treshold );

		Graphics.Blit (source, destination, material);
	}
}
