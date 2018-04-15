
using UnityEngine;
using System.Collections;

public class SkinnedVertBuffer : VertBuffer{

  public BoneWeight[] weights;
  public Vector4[] tangents;

  struct Vert{

    public float used;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 nor;
    public Vector3 tan;
    public Vector2 uv;

    public Vector3 targetPos;

    public Vector3 bindPos;
    public Vector3 bindNor;
    public Vector3 bindTan;

    public Vector4 boneWeights;
    public Vector4 boneIDs;
    public Vector3 debug;

  };

  public override void CreateBuffer(){

    structSize = 1 + 3 + 3 + 3+3+2 + 3 +3 +3+3+4 +4 + 3;
    
    if( mesh == null ){
      mesh = gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    vertices = mesh.vertices;
    uvs = mesh.uv;
    normals = mesh.normals;
    tangents = mesh.tangents;
    weights = mesh.boneWeights;
    vertCount = vertices.Length;

    _buffer = new ComputeBuffer( vertCount , structSize * sizeof(float) );

    values = new float[ structSize * vertCount ];

    int index = 0;
    for( int i = 0; i < vertCount; i++ ){

      // used 
      values[ index++ ] = 1;

      // positions
      values[ index++ ] = vertices[i].x * .99f;
      values[ index++ ] = vertices[i].y * .99f;
      values[ index++ ] = vertices[i].z * .99f;

      // vel
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

      // normals
      values[ index++ ] = normals[i].x;
      values[ index++ ] = normals[i].y;
      values[ index++ ] = normals[i].z;


      // normals
      values[ index++ ] = tangents[i].x* tangents[i].w;
      values[ index++ ] = tangents[i].y* tangents[i].w;
      values[ index++ ] = tangents[i].z* tangents[i].w;

      // uvs
      values[ index++ ] = uvs[i].x;
      values[ index++ ] = uvs[i].y;


      // target pos
      values[ index++ ] = vertices[i].x;
      values[ index++ ] = vertices[i].y;
      values[ index++ ] = vertices[i].z;



      // bindPositions
      values[ index++ ] = vertices[i].x;
      values[ index++ ] = vertices[i].y;
      values[ index++ ] = vertices[i].z;


      // bindNor
      values[ index++ ] = normals[i].x;
      values[ index++ ] = normals[i].y;
      values[ index++ ] = normals[i].z;

      // bindNor
      values[ index++ ] = tangents[i].x * tangents[i].w;
      values[ index++ ] = tangents[i].y * tangents[i].w;
      values[ index++ ] = tangents[i].z * tangents[i].w;

      // bone weights
      values[ index++ ] = weights[i].weight0;
      values[ index++ ] = weights[i].weight1;
      values[ index++ ] = weights[i].weight2;
      values[ index++ ] = weights[i].weight3;

      // bone indices
      values[ index++ ] = weights[i].boneIndex0;
      values[ index++ ] = weights[i].boneIndex1;
      values[ index++ ] = weights[i].boneIndex2;
      values[ index++ ] = weights[i].boneIndex3;

      // Debug
      values[ index++ ] = 1;
      values[ index++ ] = 0;
      values[ index++ ] = 0;

    } 
    
    _buffer.SetData(values);

  
  }


  
}