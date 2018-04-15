using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DFVolume;

public class SetVolume : MonoBehaviour {



  public sdfFromVertBuffer sdf;
  private Material mat;

	// Use this for initialization
	void Start () {

    mat = GetComponent<MeshRenderer>().material;
    //mat = new Material(mat);



	}
	
	// Update is called once per frame
	void Update () {

    
    if( sdf._buffer != null ){
      mat.SetInt( "_VolDim" , sdf.dimensions);
      mat.SetBuffer("_volumeBuffer" , sdf._buffer);
    }
		

	}
}
