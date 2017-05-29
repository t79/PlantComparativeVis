using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Framework : MonoBehaviour
{
	public enum Composition { juxtaposition, superposition, mix };

	public string ensemblePath;

	public int embeddingDim;
	public Composition compType;

	public int ensembleDim;
	public int dataDim;

	public Dictionary<string, string> characteristics;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
