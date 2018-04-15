using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DFVolume;

public class Set3DValue : MonoBehaviour {
  
  public VolumeData _data;

  private Material m;
	// Use this for initialization
	void Start () {
		m = GetComponent<MeshRenderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		m.SetTexture("_MainTex", _data.texture);
	}
}
