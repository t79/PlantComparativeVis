using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Layouter
{
	public GameObject               previewEncapsulateGO;
	public GameObject               previewInstanceGO;
	public Vector3                  elementSize;
	public List<ParameterNameValue> axisParameters;

	public virtual Vector3 Place (Structure structure)
	{
		return new Vector3();
	}

	public virtual void PlacePreview(int numOfElements)
	{
		previewEncapsulateGO = new GameObject("PreviewGO");
	}

	public virtual void DestroyGameObjects()
	{
		return;
	}

	public virtual void SetGUIPreview()
	{
		return;
	}

	public virtual List<RepresentationMesh> PlaceRepresentations (Ensemble ensemble, Abstraction abstraction, GameObject compositionGameObject)
	{
		return new List<RepresentationMesh> ();
	}

	public virtual Vector3 GetRequiredSpatialDimension()
	{
		return new Vector3 (1f, 1f, 1f);
	}
}
