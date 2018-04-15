using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicVertBuffer : VertBuffer {

  public bool rotateMesh;
  public bool scaleMesh;
  public bool translateMesh;

  struct Vert{

    public float used;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 nor;
    public Vector2 uv;

    public Vector3 targetPos;

    public Vector3 debug;

  };


  public override void CreateBuffer(){

    structSize = 1 + 3 + 3 + 3+2 + 3+ 3;

    if( mesh == null ){
      mesh = gameObject.GetComponent<MeshFilter>().mesh;
    }

    
    vertices = mesh.vertices;
    uvs = mesh.uv;
    normals = mesh.normals;

    vertCount = vertices.Length;


    _buffer = new ComputeBuffer( vertCount , structSize * sizeof(float) );
    values = new float[ structSize * vertCount ];

    int index = 0;


    Vector3 t1;
    Vector3 t2;
    for( int i = 0; i < vertCount; i++ ){


      if( rotateMesh == true ){
        t1 = transform.TransformPoint( vertices[i] );
        t2 = transform.TransformDirection( normals[i] );
      }else{
        t1 = vertices[i];
        t2 = normals[i];
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
      values[ index++ ] = uvs[i].x;
      values[ index++ ] = uvs[i].y;


      // target pos
      values[ index++ ] = t1.x;
      values[ index++ ] = t1.y;
      values[ index++ ] = t1.z;


      // Debug
      values[ index++ ] = 1;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

    } 
    
    _buffer.SetData(values);

  
  }



}