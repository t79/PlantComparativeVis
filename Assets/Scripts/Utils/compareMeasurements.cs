using UnityEngine;
using System.Collections;
using System.Xml;
using System;

public class compareMeasurements : MonoBehaviour
{
	public string originalPath = @"Ensemble\ensembleMeasurements.xml";
	public string squeezedPath = @"Ensemble\ensembleSqueezeMeasurements";
	public string holeSnapPath = @"Ensemble\ensembleHoleSnapMeasurements.xml";

	private GameObject originalCoverage = null;
	private GameObject originalHoles    = null;
	private GameObject originalSymmetry = null;

	private GameObject squeezedCoverage = null;
	private GameObject squeezedHoles    = null;
	private GameObject squeezedSymmetry = null;

	private GameObject holeSnapCoverage = null;
	private GameObject holeSnapHoles    = null;
	private GameObject holeSnapSymmetry = null;

	private GameObject coverageLegend   = null;
	private GameObject holesLegend      = null;
	private GameObject symmetryLegend   = null;

	private Texture2D squeezedCoverageTex     = null;
	private Texture2D squeezedCoverageTexDiff = null;
	private Texture2D squeezedHolesTex        = null;
	private Texture2D squeezedHolesTexDiff    = null;
	private Texture2D squeezedSymmetryTex     = null;
	private Texture2D squeezedSymmetryTexDiff = null;
	
	private Texture2D holeSnapCoverageTex     = null;
	private Texture2D holeSnapCoverageTexDiff = null;
	private Texture2D holeSnapHolesTex        = null;
	private Texture2D holeSnapHolesTexDiff    = null;
	private Texture2D holeSnapSymmetryTex     = null;
	private Texture2D holeSnapSymmetryTexDiff = null;

	private bool showAbsolute = true;

	Color coverageColorEncoding(float coverageValue)
	{
		float grey = (coverageValue - 1.5f) * 2f;
		grey = Mathf.Clamp (grey, 0f, 1f);

		return new Color (1f, 1f - grey, 1f - grey);
	}

	Color holesColorEncoding(float holesValue)
	{
		float grey = holesValue / 40f;
		grey = Mathf.Clamp (grey, 0f, 1f);
		
		return new Color (1f - grey, 1f, 1f - grey);
	}

	Color symmetryColorEncoding(float symmetryValue)
	{
		float grey = symmetryValue / 100f;
		grey = Mathf.Clamp (grey, 0f, 1f);
		
		return new Color (1f - grey, 1f - grey, 1f);
	}

	void setMeasurementsObjects (Vector3 position, string path, string name, GameObject coverage, GameObject holes, GameObject symmetry)
	{
		coverage                      = GameObject.CreatePrimitive (PrimitiveType.Plane);
		coverage.name                 = name + "Coverage";
		coverage.transform.position   = position + new Vector3(0f, 0f, 0f);
		coverage.transform.localScale = new Vector3 (10f, 1f, 10f);
		coverage.transform.parent     = transform;

		holes                      = GameObject.CreatePrimitive (PrimitiveType.Plane);
		holes.name                 = name + "Holes";
		holes.transform.position   = position + new Vector3(-100f, 0f, 0f);
		holes.transform.localScale = new Vector3 (10f, 1f, 10f);
		holes.transform.parent     = transform;

		symmetry                      = GameObject.CreatePrimitive (PrimitiveType.Plane);
		symmetry.name                 = name + "Symmetry";
		symmetry.transform.position   = position + new Vector3(-200f, 0f, 0f);
		symmetry.transform.localScale = new Vector3 (10f, 1f, 10f);
		symmetry.transform.parent     = transform;

		Texture2D coverageTexture  = new Texture2D (10, 11);
		coverageTexture.filterMode = FilterMode.Point;
		coverageTexture.wrapMode   = TextureWrapMode.Clamp;
		Color[] coverageColor      = new Color[coverageTexture.width * coverageTexture.height];

		Texture2D holesTexture  = new Texture2D (10, 11);
		holesTexture.filterMode = FilterMode.Point;
		holesTexture.wrapMode   = TextureWrapMode.Clamp;
		Color[] holesColor      = new Color[holesTexture.width * holesTexture.height];

		Texture2D symmetryTexture  = new Texture2D (10, 11);
		symmetryTexture.filterMode = FilterMode.Point;
		symmetryTexture.wrapMode   = TextureWrapMode.Clamp;
		Color[] symmetryColor      = new Color[symmetryTexture.width * symmetryTexture.height];

		XmlDocument doc = new XmlDocument ();
		doc.Load (path);
		XmlNode measurements = doc.LastChild;
		if(measurements != null)
		{
			foreach(XmlNode measurementXml in measurements.ChildNodes)
			{
				int structureId = Convert.ToInt32(measurementXml.Attributes["structure"].Value) - 1;

				XmlNode coverageNode = measurementXml.FirstChild;
				float coverageValue = Convert.ToSingle(coverageNode.Attributes["value"].Value);
				coverageColor[structureId] = coverageColorEncoding(coverageValue);

				XmlNode holesNode = coverageNode.NextSibling;
				float holesValue = Convert.ToSingle(holesNode.Attributes["value"].Value);
				holesColor[structureId] = holesColorEncoding(holesValue);

				XmlNode symmetryNode = holesNode.NextSibling;
				float symmetryValue = Convert.ToSingle(symmetryNode.Attributes["value"].Value);
				symmetryColor[structureId] = symmetryColorEncoding(symmetryValue);
			}
		}

		coverageTexture.SetPixels (coverageColor);
		coverageTexture.Apply ();
		coverage.GetComponent<Renderer>().material             = Resources.Load("Materials/defaultMat") as Material;
		coverage.GetComponent<Renderer>().material.mainTexture = coverageTexture;

		holesTexture.SetPixels (holesColor);
		holesTexture.Apply ();
		holes.GetComponent<Renderer>().material             = Resources.Load("Materials/defaultMat") as Material;
		holes.GetComponent<Renderer>().material.mainTexture = holesTexture;

		symmetryTexture.SetPixels (symmetryColor);
		symmetryTexture.Apply ();
		symmetry.GetComponent<Renderer>().material             = Resources.Load("Materials/defaultMat") as Material;
		symmetry.GetComponent<Renderer>().material.mainTexture = symmetryTexture;
	}

	void setLegendObjects ()
	{
		int textureRes = 10;

		Texture2D coverageLegendTexture  = new Texture2D (textureRes, 1);
		coverageLegendTexture.filterMode = FilterMode.Point;
		coverageLegendTexture.wrapMode   = TextureWrapMode.Clamp;

		Color[] coverageLegendColors = new Color[coverageLegendTexture.width];
		for(int i = 0; i < coverageLegendColors.Length; i++)
			coverageLegendColors[i] = new Color (1f, 1f - (float)i / (float)coverageLegendColors.Length, 1f - (float)i / (float)coverageLegendColors.Length);
		coverageLegendTexture.SetPixels (coverageLegendColors);
		coverageLegendTexture.Apply ();

		coverageLegend                               = GameObject.CreatePrimitive (PrimitiveType.Plane);
		coverageLegend.name                          = "Coverage";
		coverageLegend.transform.position            = new Vector3 (0f, 0f, -56f);
		coverageLegend.transform.localScale          = new Vector3 (10f, 1f, 1f);
		coverageLegend.transform.parent              = transform;
		coverageLegend.GetComponent<Renderer>().material             = Resources.Load("Materials/defaultMat") as Material;
		coverageLegend.GetComponent<Renderer>().material.mainTexture = coverageLegendTexture;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		Texture2D holesLegendTexture = new Texture2D (textureRes, 1);
		holesLegendTexture.filterMode = FilterMode.Point;
		holesLegendTexture.wrapMode   = TextureWrapMode.Clamp;

		Color[] holesLegendColors = new Color[holesLegendTexture.width];
		for(int i = 0; i < holesLegendColors.Length; i++)
			holesLegendColors[i] = new Color (1f - (float)i / (float)holesLegendColors.Length, 1f,  1f - (float)i / (float)holesLegendColors.Length);
		holesLegendTexture.SetPixels (holesLegendColors);
		holesLegendTexture.Apply ();
		
		holesLegend                               = GameObject.CreatePrimitive (PrimitiveType.Plane);
		holesLegend.name                          = "Holes";
		holesLegend.transform.position            = new Vector3 (-100f, 0f, -56f);
		holesLegend.transform.localScale          = new Vector3 (10f, 1f, 1f);
		holesLegend.transform.parent              = transform;
		holesLegend.GetComponent<Renderer>().material             = Resources.Load("Materials/defaultMat") as Material;
		holesLegend.GetComponent<Renderer>().material.mainTexture = holesLegendTexture;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		Texture2D symmetryLegendTexture = new Texture2D (textureRes, 1);
		symmetryLegendTexture.filterMode = FilterMode.Point;
		symmetryLegendTexture.wrapMode   = TextureWrapMode.Clamp;
		
		Color[] symmetryLegendColors = new Color[symmetryLegendTexture.width];
		for(int i = 0; i < symmetryLegendColors.Length; i++)
			symmetryLegendColors[i] = new Color (1f - (float)i / (float)symmetryLegendColors.Length, 1f - (float)i / (float)symmetryLegendColors.Length, 1f);
		symmetryLegendTexture.SetPixels (symmetryLegendColors);
		symmetryLegendTexture.Apply ();
		
		symmetryLegend                               = GameObject.CreatePrimitive (PrimitiveType.Plane);
		symmetryLegend.name                          = "Symmetry";
		symmetryLegend.transform.position            = new Vector3 (-200f, 0f, -56f);
		symmetryLegend.transform.localScale          = new Vector3 (10f, 1f, 1f);
		symmetryLegend.transform.parent              = transform;
		symmetryLegend.GetComponent<Renderer>().material             = Resources.Load("Materials/defaultMat") as Material;
		symmetryLegend.GetComponent<Renderer>().material.mainTexture = symmetryLegendTexture;
	}

	void setDifferenceTextures ()
	{
		// Coverage - g
		GameObject defaultCoverageGameObject = GameObject.Find("defaultCoverage");
		Texture2D  defaultCoverageTex        = defaultCoverageGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    defaultCoverageCol        = defaultCoverageTex.GetPixels ();

		GameObject squeezedCoverageGameObject = GameObject.Find("squeezedCoverage");
		           squeezedCoverageTex        = squeezedCoverageGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    squeezedCoverageCol        = squeezedCoverageTex.GetPixels ();
		Color[]    squeezedCoverageColDif     = new Color[squeezedCoverageCol.Length];

		GameObject holeSnapCoverageGameObject = GameObject.Find("holeSnapCoverage");
		           holeSnapCoverageTex        = holeSnapCoverageGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    holeSnapCoverageCol        = holeSnapCoverageTex.GetPixels ();
		Color[]    holeSnapCoverageColDif     = new Color[holeSnapCoverageCol.Length];

		for (int i = 0; i < defaultCoverageCol.Length; i++)
		{
			float valueDefault  = 1f - defaultCoverageCol[i].g;
			float valueSqueezed = 1f - squeezedCoverageCol[i].g;
			float valueHoleSnap = 1f - holeSnapCoverageCol[i].g;

			squeezedCoverageColDif[i] = new Color(1f - Mathf.Abs(valueDefault - valueSqueezed), 1f - Mathf.Abs(valueDefault - valueSqueezed), 1f - Mathf.Abs(valueDefault - valueSqueezed));
			holeSnapCoverageColDif[i] = new Color(1f - Mathf.Abs(valueDefault - valueHoleSnap), 1f - Mathf.Abs(valueDefault - valueHoleSnap), 1f - Mathf.Abs(valueDefault - valueHoleSnap));
		}

		// Holes - r
		GameObject defaultHolesGameObject = GameObject.Find("defaultHoles");
		Texture2D  defaultHolesTex        = defaultHolesGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    defaultHolesCol        = defaultHolesTex.GetPixels ();
		
		GameObject squeezedHolesGameObject = GameObject.Find("squeezedHoles");
		           squeezedHolesTex        = squeezedHolesGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    squeezedHolesCol        = squeezedHolesTex.GetPixels ();
		Color[]    squeezedHolesColDif     = new Color[squeezedHolesCol.Length];

		GameObject holeSnapHolesGameObject = GameObject.Find("holeSnapHoles");
		           holeSnapHolesTex        = holeSnapHolesGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    holeSnapHolesCol        = holeSnapHolesTex.GetPixels ();
		Color[]    holeSnapHolesColDif     = new Color[holeSnapHolesCol.Length];
		
		for (int i = 0; i < defaultHolesCol.Length; i++)
		{
			float valueDefault  = 1f - defaultHolesCol[i].r;
			float valueSqueezed = 1f - squeezedHolesCol[i].r;
			float valueHoleSnap = 1f - holeSnapHolesCol[i].r;
			
			squeezedHolesColDif[i] = new Color(1f - Mathf.Abs(valueDefault - valueSqueezed), 1f - Mathf.Abs(valueDefault - valueSqueezed), 1f - Mathf.Abs(valueDefault - valueSqueezed));
			holeSnapHolesColDif[i] = new Color(1f - Mathf.Abs(valueDefault - valueHoleSnap), 1f - Mathf.Abs(valueDefault - valueHoleSnap), 1f - Mathf.Abs(valueDefault - valueHoleSnap));
		}

		// Symmetry - r
		GameObject defaultSymmetryGameObject = GameObject.Find("defaultSymmetry");
		Texture2D  defaultSymmetryTex        = defaultSymmetryGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    defaultSymmetryCol        = defaultSymmetryTex.GetPixels ();
		
		GameObject squeezedSymmetryGameObject = GameObject.Find("squeezedSymmetry");
		           squeezedSymmetryTex        = squeezedSymmetryGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    squeezedSymmetryCol        = squeezedSymmetryTex.GetPixels ();
		Color[]    squeezedSymmetryColDif     = new Color[squeezedSymmetryCol.Length];
		
		GameObject holeSnapSymmetryGameObject = GameObject.Find("holeSnapSymmetry");
		           holeSnapSymmetryTex        = holeSnapSymmetryGameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
		Color[]    holeSnapSymmetryCol        = holeSnapSymmetryTex.GetPixels ();
		Color[]    holeSnapSymmetryColDif     = new Color[holeSnapSymmetryCol.Length];
		
		for (int i = 0; i < defaultSymmetryCol.Length; i++)
		{
			float valueDefault  = 1f - defaultSymmetryCol[i].r;
			float valueSqueezed = 1f - squeezedSymmetryCol[i].r;
			float valueHoleSnap = 1f - holeSnapSymmetryCol[i].r;

			squeezedSymmetryColDif[i] = new Color(1f - Mathf.Abs(valueDefault - valueSqueezed), 1f - Mathf.Abs(valueDefault - valueSqueezed), 1f - Mathf.Abs(valueDefault - valueSqueezed));
			holeSnapSymmetryColDif[i] = new Color(1f - Mathf.Abs(valueDefault - valueHoleSnap), 1f - Mathf.Abs(valueDefault - valueHoleSnap), 1f - Mathf.Abs(valueDefault - valueHoleSnap));
		}

		// set final textures
		squeezedCoverageTexDiff            = new Texture2D (squeezedCoverageTex.width, squeezedCoverageTex.height);
		squeezedCoverageTexDiff.filterMode = FilterMode.Point;
		squeezedCoverageTexDiff.wrapMode   = TextureWrapMode.Clamp;
		squeezedCoverageTexDiff.SetPixels(squeezedCoverageColDif);squeezedCoverageTexDiff.Apply ();

		squeezedHolesTexDiff            = new Texture2D (squeezedHolesTex.width, squeezedHolesTex.height);
		squeezedHolesTexDiff.filterMode = FilterMode.Point;
		squeezedHolesTexDiff.wrapMode   = TextureWrapMode.Clamp;
		squeezedHolesTexDiff.SetPixels(squeezedHolesColDif);squeezedHolesTexDiff.Apply ();

		squeezedSymmetryTexDiff            = new Texture2D (squeezedSymmetryTex.width, squeezedSymmetryTex.height);
		squeezedSymmetryTexDiff.filterMode = FilterMode.Point;
		squeezedSymmetryTexDiff.wrapMode   = TextureWrapMode.Clamp;
		squeezedSymmetryTexDiff.SetPixels(squeezedSymmetryColDif);squeezedSymmetryTexDiff.Apply ();

		holeSnapCoverageTexDiff            = new Texture2D (holeSnapCoverageTex.width, holeSnapCoverageTex.height);
		holeSnapCoverageTexDiff.filterMode = FilterMode.Point;
		holeSnapCoverageTexDiff.wrapMode   = TextureWrapMode.Clamp;
		holeSnapCoverageTexDiff.SetPixels (holeSnapCoverageColDif); holeSnapCoverageTexDiff.Apply ();

		holeSnapHolesTexDiff            = new Texture2D (holeSnapHolesTex.width, holeSnapHolesTex.height);
		holeSnapHolesTexDiff.filterMode = FilterMode.Point;
		holeSnapHolesTexDiff.wrapMode   = TextureWrapMode.Clamp;
		holeSnapHolesTexDiff.SetPixels (holeSnapHolesColDif); holeSnapHolesTexDiff.Apply ();

		holeSnapSymmetryTexDiff            = new Texture2D (holeSnapSymmetryTex.width, holeSnapSymmetryTex.height);
		holeSnapSymmetryTexDiff.filterMode = FilterMode.Point;
		holeSnapSymmetryTexDiff.wrapMode   = TextureWrapMode.Clamp;
		holeSnapSymmetryTexDiff.SetPixels (holeSnapSymmetryColDif); holeSnapSymmetryTexDiff.Apply ();
	}

	void Start ()
	{
		setLegendObjects ();

		Vector3 pos = new Vector3 ();
		setMeasurementsObjects (pos, originalPath, "default", originalCoverage, originalHoles, originalSymmetry);

		pos += new Vector3(0f, 0f, 101f);
		setMeasurementsObjects (pos, squeezedPath, "squeezed", squeezedCoverage, squeezedHoles, squeezedSymmetry);

		pos += new Vector3(0f, 0f, 101f);
		setMeasurementsObjects (pos, holeSnapPath, "holeSnap", holeSnapCoverage, holeSnapHoles, holeSnapSymmetry);

		setDifferenceTextures ();
	}

	public void SwitchTextures()
	{
		showAbsolute = !showAbsolute;

		if(showAbsolute)
		{
			GameObject squeezedCoverageGameObject = GameObject.Find("squeezedCoverage");
			squeezedCoverageGameObject.GetComponent<Renderer>().material.mainTexture = squeezedCoverageTex;
			
			GameObject holeSnapCoverageGameObject = GameObject.Find("holeSnapCoverage");
			holeSnapCoverageGameObject.GetComponent<Renderer>().material.mainTexture = holeSnapCoverageTex;

			GameObject squeezedHolesGameObject = GameObject.Find("squeezedHoles");
			squeezedHolesGameObject.GetComponent<Renderer>().material.mainTexture = squeezedHolesTex;
			
			GameObject holeSnapHolesGameObject = GameObject.Find("holeSnapHoles");
			holeSnapHolesGameObject.GetComponent<Renderer>().material.mainTexture = holeSnapHolesTex;

			GameObject squeezedSymmetryGameObject = GameObject.Find("squeezedSymmetry");
			squeezedSymmetryGameObject.GetComponent<Renderer>().material.mainTexture = squeezedSymmetryTex;
			
			GameObject holeSnapSymmetryGameObject = GameObject.Find("holeSnapSymmetry");
			holeSnapSymmetryGameObject.GetComponent<Renderer>().material.mainTexture = holeSnapSymmetryTex;
		}
		else
		{
			GameObject squeezedCoverageGameObject = GameObject.Find("squeezedCoverage");
			squeezedCoverageGameObject.GetComponent<Renderer>().material.mainTexture = squeezedCoverageTexDiff;
			
			GameObject holeSnapCoverageGameObject = GameObject.Find("holeSnapCoverage");
			holeSnapCoverageGameObject.GetComponent<Renderer>().material.mainTexture = holeSnapCoverageTexDiff;
			
			GameObject squeezedHolesGameObject = GameObject.Find("squeezedHoles");
			squeezedHolesGameObject.GetComponent<Renderer>().material.mainTexture = squeezedHolesTexDiff;
			
			GameObject holeSnapHolesGameObject = GameObject.Find("holeSnapHoles");
			holeSnapHolesGameObject.GetComponent<Renderer>().material.mainTexture = holeSnapHolesTexDiff;
			
			GameObject squeezedSymmetryGameObject = GameObject.Find("squeezedSymmetry");
			squeezedSymmetryGameObject.GetComponent<Renderer>().material.mainTexture = squeezedSymmetryTexDiff;
			
			GameObject holeSnapSymmetryGameObject = GameObject.Find("holeSnapSymmetry");
			holeSnapSymmetryGameObject.GetComponent<Renderer>().material.mainTexture = holeSnapSymmetryTexDiff;
		}
	}
}
