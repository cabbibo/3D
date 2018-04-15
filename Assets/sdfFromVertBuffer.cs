using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sdfFromVertBuffer : MonoBehaviour {


  public ComputeShader calcShader;
  public int dimensions;


  public int numTrisPerFrame;
  public ComputeBuffer _buffer;

  public string name;

  private int numThreads = 64;

  private int currTri = 0;

  public float loaded;

  private TriangleBuffer tri;
  private VertBuffer vert;


  private bool calculated = true;
  private bool normalsCalculated = true;
  private float[] values;

  private SaveBuffer save;

  private int vertCount;
  private int numGroups;
  private int k_depth;
  private int k_normal;
  private int k_finalDepth;
	
  // Use this for initialization
	void OnEnable () {


    save = GetComponent<SaveBuffer>();

    if( name  != null ){
      float[] data = save.Load(name);
      if( data.Length == 1){
        SetUp();
      }else{
        values = data;

        loaded = 1;
      }
    }

   
   
    _buffer = new ComputeBuffer(dimensions*dimensions*dimensions, sizeof(float)*8);

    _buffer.SetData(values);

    
	}

  void SetUp(){



    tri = GetComponent<TriangleBuffer>();
    vert = GetComponent<VertBuffer>();   

    values = new float[dimensions*dimensions*dimensions * 8];
    
    for(int i = 0; i < dimensions * dimensions * dimensions; i++){
      values[i*8+0]=1000;
      values[i*8+1]=1000;
      values[i*8+2]=1000;
      values[i*8+3]=1000;
      values[i*8+4]=1000;
      values[i*8+5]=1000;
      values[i*8+6]=1000;
      values[i*8+7]=1000;
     }

    vertCount = dimensions * dimensions * dimensions;
    numGroups = (vertCount+(numThreads-1))/numThreads; 
    k_depth = calcShader.FindKernel("GetDepth");
    k_normal = calcShader.FindKernel("GetNormal");
    k_finalDepth = calcShader.FindKernel("GetFinalDist");

    calculated= false;
    normalsCalculated = false;

  }



  void OnDisable(){
    if( _buffer != null ){ _buffer.Release(); }
  }
	
	// Update is called once per frame
	void FixedUpdate () {


    if( calculated == false){// && _buffer == null ){

//      print("hey");
      if(vert._buffer != null && tri._buffer != null){
        // print("hey1");
      for( int i = 0; i < numTrisPerFrame; i++){
        Calculate();
      }}
    }

		
	}

  void Calculate(){



    if( currTri < tri.triCount){

      calcShader.SetInt("_NumTris",tri.triCount);
      calcShader.SetInt("_NumVerts",vert.vertCount);
      calcShader.SetInt("_VolDim", dimensions);
      calcShader.SetInt("_CurrentTri", currTri);

      calcShader.SetBuffer(k_depth,"vertBuffer", vert._buffer);
      calcShader.SetBuffer(k_depth,"triBuffer", tri._buffer);
      calcShader.SetBuffer(k_depth,"volumeBuffer", _buffer);
      calcShader.Dispatch(k_depth, numGroups ,1,1);

      currTri += 3;

      loaded = (float)currTri / tri.triCount;

    }else{

      if( normalsCalculated == false ){


        calcShader.SetInt("_NumTris",tri.triCount);
        calcShader.SetInt("_NumVerts",vert.vertCount);
        calcShader.SetInt("_VolDim", dimensions);



        calcShader.SetBuffer(k_finalDepth,"vertBuffer", vert._buffer);
        calcShader.SetBuffer(k_finalDepth,"triBuffer", tri._buffer);
        calcShader.SetBuffer(k_finalDepth,"volumeBuffer", _buffer);
        calcShader.Dispatch(k_finalDepth, numGroups ,1,1);

        calcShader.SetBuffer(k_normal,"vertBuffer", vert._buffer);
        calcShader.SetBuffer(k_normal,"triBuffer", tri._buffer);
        calcShader.SetBuffer(k_normal,"volumeBuffer", _buffer);
        calcShader.Dispatch(k_normal, numGroups ,1,1);

        normalsCalculated = true;
        calculated = true;


        _buffer.GetData(values);
        save.Save( values , name );

      }


    }




  }
}
