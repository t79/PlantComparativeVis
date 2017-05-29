using UnityEngine;

[RequireComponent (typeof(Camera))]
[AddComponentMenu("")]
		
public class PostEffectsBase : MonoBehaviour
{	
	protected bool supportHDRTextures = true;
	protected bool supportDX11        = false;
	protected bool isSupported        = true;
	
	public Material CheckShaderAndCreateMaterial (Shader s, Material m2Create)
	{
		if (!s)
		{
			Debug.Log("Missing shader in " + this.ToString ());
			enabled = false;
			return null;
		}
		
		if (s.isSupported && m2Create && m2Create.shader == s) 
			return m2Create;
		
		if (!s.isSupported)
		{
			NotSupported ();
			Debug.Log("The shader " + s.ToString() + " on effect " + this.ToString() + " is not supported on this platform!");
			return null;
		}
		else
		{
			m2Create = new Material (s);	
			m2Create.hideFlags = HideFlags.DontSave;		
			if (m2Create) 
				return m2Create;
			else return null;
		}
	}
	
	public Material CreateMaterial (Shader s, Material m2Create)
	{
		if (!s)
		{ 
			Debug.Log ("Missing shader in " + this.ToString ());
			return null;
		}
		
		if (m2Create && (m2Create.shader == s) && (s.isSupported)) 
			return m2Create;
		
		if (!s.isSupported)
		{
			return null;
		}
		else
		{
			m2Create = new Material (s);	
			m2Create.hideFlags = HideFlags.DontSave;		
			if (m2Create) 
				return m2Create;
			else return null;
		}
	}
	
	public void OnEnable()
	{
		isSupported = true;
	}	
	
	public virtual bool CheckResources ()
	{
		Debug.LogWarning ("CheckResources () for " + this.ToString() + " should be overwritten.");
		return isSupported;
	}
	
	public void Start ()
	{
		CheckResources ();
	}	
	
	public bool CheckSupport (bool needDepth)
	{
		isSupported = true;
		supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
		supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
		
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
		{
			NotSupported ();
			return false;
		}		
		
		if(needDepth && !SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth))
		{
			NotSupported ();
			return false;
		}
		
		if(needDepth)
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;	
		
		return true;
	}
	
	public bool Dx11Support()
	{
		return supportDX11;
	}
		
	public void NotSupported ()
	{
		enabled = false;
		isSupported = false;
		return;
	}
}