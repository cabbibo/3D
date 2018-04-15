using UnityEngine;
using System.Collections;

public class SkinnedTriangleBuffer : TriangleBuffer {

  public override void GetMesh(){
    if( mesh == null){
      mesh = gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }
  }
}