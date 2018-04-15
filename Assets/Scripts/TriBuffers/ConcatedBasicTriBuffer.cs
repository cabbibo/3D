using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcatedBasicTriBuffer : TriangleBuffer {

  public GameObject[] meshes;

	public override void GetMesh(){} 


  public override void CreateBuffer(){

    triCount = 0;

    int baseVal = 0; 
    for( int i = 0; i < meshes.Length; i++ ){
      int[] tris = meshes[i].GetComponent<MeshFilter>().mesh.triangles;
      triCount += tris.Length;
    }

    triangles = new int[ triCount ];//triData.ToArray();

    int index = 0;

    for( int i = 0; i < meshes.Length; i++ ){

      int[] tris = meshes[i].GetComponent<MeshFilter>().mesh.triangles;

      for( int j = 0; j < tris.Length; j++ ){
        triangles[index++] = tris[j] + baseVal;
      }


      baseVal += meshes[i].GetComponent<MeshFilter>().mesh.vertices.Length;
      
    }

    _buffer = new ComputeBuffer( triCount , sizeof(int) ); 
    _buffer.SetData(triangles);


  }
}
