using UnityEngine;
using System.Collections;

public class BasicTriangleBuffer : TriangleBuffer {

  public override void GetMesh(){
    if( mesh == null){
      mesh = gameObject.GetComponent<MeshFilter>().mesh;
    }
  }

}