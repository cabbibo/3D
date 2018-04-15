using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVertBuffer : VertBuffer {

  public int numVerts;

  struct Vert{

    public float life;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 nor;
    public Vector2 uv;
    public Vector3 targetPos;
    public Vector3 debug;

  };


  public override void CreateBuffer(){

    vertCount = numVerts;
    structSize = 1 + 3 + 3 + 3+2 + 3+ 3;

    _buffer = new ComputeBuffer( vertCount , structSize * sizeof(float) );
    values = new float[ structSize * vertCount ];

    int index = 0;

    Vector3 t1;
    Vector3 t2;

    for( int i = 0; i <vertCount; i++ ){

      // used 
      values[ index++ ] = Random.Range( 0.01f, .99f);


      // positions
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

      // vel
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

      // normals
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

      // uvs
      values[ index++ ] = (float)i/(float)vertCount;
      values[ index++ ] = i;


      // target pos
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;


      // Debug
      values[ index++ ] = Random.Range( 0.7f, .99f);
      values[ index++ ] = 0;
      values[ index++ ] = 1;

    } 
    
    _buffer.SetData(values);

  
  }



}