using UnityEngine;
using System.Collections;

[AddComponentMenu("Image Effects/Density")]
public class Density : ImageEffectBase
{
	public Texture2D density;
	public Vector4   position;
	public float     scale;
	public float     size;
	public float     ratio;
	public float     angle;
	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetTexture ("_density",   density);
		material.SetVector  ("_position",  position);
		material.SetFloat   ("_scale",     scale);
		material.SetFloat   ("_size",      size);
		material.SetFloat   ("_ratio",     ratio);
		material.SetFloat   ("_angle",     angle);

		Graphics.Blit (source, destination, material);
	}
}