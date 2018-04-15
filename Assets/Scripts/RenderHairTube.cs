using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderHairTube : MonoBehaviour {

  public HairOnVertBuffer hair;
  public Material material;

  public Color color1;
  public Color color2;
  public Color color3;


  public int tubeWidth;
  public int tubeLength;
  public int totalVerts;


	// Use this for initialization
	void Start () {

    if( hair == null ){ 
      hair = gameObject.GetComponent<HairOnVertBuffer>();
    }

    material = new Material( material );

    totalVerts = hair.totalHairs * tubeWidth * (tubeLength-1) * 3 * 2;
		
	}

  void OnRenderObject(){

    material.SetPass(0);

    material.SetBuffer("_vertBuffer", hair._buffer );
    material.SetVector("_Color1", color1);
    material.SetVector("_Color2", color2);
    material.SetVector("_Color3", color3);
    material.SetInt("_TubeWidth" , tubeWidth );
    material.SetInt("_NumVertsPerHair" , hair.numVertsPerHair );
    material.SetInt("_TubeLength" , tubeLength );
    material.SetInt("_TotalHair" , hair.totalHairs * hair.numVertsPerHair  );
    material.SetInt("_TotalVerts" , totalVerts );

    Graphics.DrawProcedural(MeshTopology.Triangles, totalVerts );

  }


	
	// Update is called once per frame
	void Update () {
		
	}


}
