using UnityEngine;

public class Create3DTex : MonoBehaviour
{	
	public Shader    shader;
	public Texture3D tex;
	public int       size = 32;
	public Vector2   boxSize = new Vector2();
	
	void Start ()
	{
		tex = new Texture3D (size, size, size, TextureFormat.ARGB32, true);
		tex.filterMode = FilterMode.Point;

		var   cols = new Color[size * size * size];
		int   idx  = 0;
		Color c    = Color.white;

		for (int z = 0; z < size; ++z)
		{
			for (int y = 0; y < size; ++y)
			{
				for (int x = 0; x < size; ++x, ++idx)
				{
					if(((x) == 0) && ((y) == 0) && ((z) == 0))
					{
						c.r = 0f;
						c.g = 0f;
						c.b = 0f;
					}
					else
					{
						c.r = 1f;
						c.g = 1f;
						c.b = 1f;
					}
					cols[idx] = c;
				}
			}
		}
		tex.SetPixels (cols);
		tex.Apply ();

		gameObject.AddComponent<MeshRenderer> ();
		gameObject.GetComponent<MeshRenderer> ().material = new Material (shader);
		GetComponent<Renderer>().material.SetTexture ("_Volume", tex);
	}

	void Update()
	{
		boxSize.x = GetComponent<Renderer>().bounds.size.x;
		boxSize.y = GetComponent<Renderer>().bounds.size.y;
	}
}