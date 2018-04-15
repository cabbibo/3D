using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The Array of meshes must be the same in Vert and Tri buffer!!!!
public class ConcatedBasicVertBuffer : VertBuffer {

  public bool rotateMesh;
  public bool scaleMesh;
  public bool translateMesh;

  public GameObject[] meshes;


  public override void CreateBuffer(){

   List<Structs.Vertex> verticesData = new List<Structs.Vertex>();

   int totalVerts = 0;

   Vector3 t1;
   Vector3 t2;



   for( int i = 0; i < meshes.Length; i++ ){

    totalVerts +=  meshes[i].GetComponent<MeshFilter>().mesh.vertices.Length;

  }


  int index = 0;

values = new float[ totalVerts *  Structs.GetSizeOf(typeof (Structs.Vertex))];

   for( int i = 0; i < meshes.Length; i++ ){


    
    GameObject go = meshes[i];
    Mesh mesh = go.GetComponent<MeshFilter>().mesh;
    vertices = mesh.vertices;
    uvs = mesh.uv;
    normals = mesh.normals;

    vertCount = vertices.Length;

  


    for(  int j = 0; j < vertCount; j++ ){


    t1 = vertices[j];
    t2 = normals[j];

    if( translateMesh == true ){
      t1 = go.transform.TransformPoint( t1 );
      t2 = go.transform.TransformDirection( t2 );
    }else{
      t1 = vertices[j];
      t2 = normals[j];
    }




         // used 
      values[ index++ ] = 1;

      // positions
      values[ index++ ] = t1.x * 1;
      values[ index++ ] = t1.y * 1;
      values[ index++ ] = t1.z * 1;

      // vel
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

      // normals
      values[ index++ ] = t2.x;
      values[ index++ ] = t2.y;
      values[ index++ ] = t2.z;

      // uvs
      values[ index++ ] = uvs[j].x;
      values[ index++ ] = uvs[j].y;


      // target pos
      values[ index++ ] = t1.x;
      values[ index++ ] = t1.y;
      values[ index++ ] = t1.z;


      // Debug
      values[ index++ ] = 1;
      values[ index++ ] = 0;
      values[ index++ ] = 0;
    
    }
  }



 

  _buffer = new ComputeBuffer (totalVerts, Structs.GetSizeOf( typeof (Structs.Vertex)));
  _buffer.SetData (values);


 }
}