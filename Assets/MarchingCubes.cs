using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour {


  public float surfaceDistance;
  public int dimensions;
  public ComputeShader cubeShader;
  public sdfFromVertBuffer sdf;
  public Material  material;

  private ComputeBuffer _cubeEdgeBuffer;
  private ComputeBuffer _triangleConnectionBuffer;
  private ComputeBuffer _buffer;

  private int numThreads = 64;
  private int numGroups;
  private int triSize;
  private int total;


  //pos
  //nor 

	// Use this for initialization
	void Start () {

     //These two buffers are just some settings needed by the marching cubes.
     _cubeEdgeBuffer = new ComputeBuffer(256, sizeof(int));
     _cubeEdgeBuffer.SetData(MarchingCubesTables.CubeEdgeFlags);

     print( MarchingCubesTables.CubeEdgeFlags[127]);
     print( MarchingCubesTables.CubeEdgeFlags[255]);
     _triangleConnectionBuffer = new ComputeBuffer(256 * 16, sizeof(int));
     _triangleConnectionBuffer.SetData(MarchingCubesTables.TriangleConnectionTable);

     print( MarchingCubesTables.TriangleConnectionTable[127,4]);
     print( MarchingCubesTables.TriangleConnectionTable[255,15]);

     dimensions = sdf.dimensions;
    total= dimensions*dimensions*dimensions;
    triSize = total*5*3;

    numGroups = total / numThreads;//(total+(numThreads-1))/numThreads;
    print( numGroups);

    _buffer = new ComputeBuffer(triSize, sizeof(float) * 6 );


		
	}

  void OnRenderObject(){

//    print("hey");

    material.SetPass(0);

    material.SetInt("_NumVerts",triSize);
    material.SetBuffer("_vertBuffer",_buffer);
    Graphics.DrawProcedural(MeshTopology.Triangles, triSize);

  }
	
	// Update is called once per frame
	void LateUpdate () {

    if( sdf._buffer != null ){

            cubeShader.SetInt("_VolDim", dimensions);
            cubeShader.SetInt("_CubeDimensions", dimensions);

            cubeShader.SetFloat("_Target",surfaceDistance);
            /*
            cubeShader.SetInt("_Border", 1);
            */
            cubeShader.SetBuffer(0, "_volumeBuffer", sdf._buffer);
            cubeShader.SetBuffer(0, "_vertBuffer", _buffer);
            cubeShader.SetBuffer(0, "_cubeEdgeBuffer", _cubeEdgeBuffer);
            cubeShader.SetBuffer(0, "_triangleConnectionBuffer", _triangleConnectionBuffer);

            cubeShader.Dispatch(0, numGroups, 1, 1);
          
	}else{
    print("na");
  }
}
}
