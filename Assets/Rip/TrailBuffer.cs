using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailBuffer : MonoBehaviour {


    public float RibbonsIn;

    public Color col;

    public RandomVertBuffer vertBuffer;

  public sdfFromVertBuffer sdf;

    public Material realMaterial;

    public Material material;
    public Material pointMat;
    private Material mat;

    private int _kernel;

    private int numThreads = 64;
    private int numGroups;
    private int numTransferGroups;
    private int total;

    public int tailSize;
    public ComputeShader computeShader;
    public ComputeShader transferShader;

    private ComputeBuffer _buffer;
    private ComputeBuffer _transferBuffer;

    private float[] values;

    // Use this for initialization
    void Start() {

      if( vertBuffer == null){ vertBuffer = GetComponent<RandomVertBuffer>(); }
      if( sdf == null){sdf = GetComponent<ParticleSym>().sdf; }

      mat = new Material( material );

      realMaterial = new Material(realMaterial);


      _kernel = computeShader.FindKernel("CSMain");

      Set();

    }



    void createOldBuffer(){

     
      total = vertBuffer.vertCount * tailSize;
      if( total > 0 &&  _buffer == null ){
   
        // Only care about positions
        _buffer = new ComputeBuffer( total , 4 * sizeof(float));
        _transferBuffer = new ComputeBuffer( total * 2 , sizeof(float) * (3+3+3+2+3) );

        makeMesh();
      }



      numGroups = (vertBuffer.vertCount+(numThreads-1))/numThreads;
      numTransferGroups = (total*2+(numThreads-1))/numThreads;

    }

    void makeMesh(){

        int numTriangles = 3 * 2 * (tailSize-1) * vertBuffer.vertCount;
        int numVerts = total * 2;

          

        int[] triangles = new int[numTriangles];

        int maxID = 0;

        for( int i = 0; i < numTriangles; i++){

          int quadID = ((i / 6));

          int qIDInTail = quadID % (tailSize-1);

          int tailID = quadID / (tailSize-1);



          int triID = i%6;

          int fID = 0;

          fID = tailID * ((tailSize)*2) + qIDInTail * 2;

          if( triID == 0 ){ fID += 0; }
          if( triID == 1 ){ fID += 1; }
          if( triID == 2 ){ fID += 3; }
          if( triID == 3 ){ fID += 0; }
          if( triID == 4 ){ fID += 3; }
          if( triID == 5 ){ fID += 2; }


         // print("tail_" + tailID);
          //print("quad_" + quadID);
          //print(  "fID :  " + i + "   :   " + fID + " : tailId : " + tailID + " : QUAD : " + qIDInTail);

          triangles[i] = fID;
          if(fID > maxID ){ maxID = fID;}
        }


    
        Vector3[] vertices = new Vector3[numVerts];


        Mesh mesh = new Mesh ();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.bounds = new Bounds (Vector3.zero, Vector3.one * 10000f);
        mesh.UploadMeshData(true);

        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = realMaterial;

        
        realMaterial.SetBuffer("_transferBuffer" , _transferBuffer);


    }

    void OnDestroy(){

      if( _buffer != null ){
        _buffer.Release();
        _transferBuffer.Release();
      }

    }

    void OnRenderObject(){


      mat.SetPass(0);

      mat.SetInt("_RibbonsIn", (int)(RibbonsIn *  (float)vertBuffer.vertCount));
      mat.SetInt("_TailSize" , tailSize );
      mat.SetInt("_TotalOld" , total );
      mat.SetInt("_NumVerts" , vertBuffer.vertCount );
      mat.SetBuffer("_buffer", _buffer );

     // Graphics.DrawProcedural(MeshTopology.Triangles,  3 * 2 * (tailSize-1) * vertBuffer.vertCount);

    
    }


    void DoPhysics( int Set){

      if( vertBuffer._buffer != null && _buffer != null && _transferBuffer != null){

//        print( numTransferGroups );


        computeShader.SetInt("_TailSize" , tailSize );
        computeShader.SetInt("_NumVerts" , vertBuffer.vertCount );
        computeShader.SetInt("_TotalOld" , total );
  
        computeShader.SetBuffer( _kernel , "vertBuffer"  , vertBuffer._buffer );
        computeShader.SetBuffer( _kernel , "oldBuffer"  , _buffer );
        computeShader.Dispatch( _kernel , numGroups,1,1 );
  


  transferShader.SetInt( "_VolDim" , sdf.dimensions );
        transferShader.SetInt("_TailSize" , tailSize );
        transferShader.SetInt("_NumVerts" , vertBuffer.vertCount );
        transferShader.SetInt("_TotalVerts" , total * 2);
        transferShader.SetInt("_TotalOld" , total );
        transferShader.SetVector("_Camera" , Camera.main.transform.position );
  
        transferShader.SetBuffer( 0 , "vertBuffer"      , _buffer );
        transferShader.SetBuffer( 0 , "transferBuffer"  , _transferBuffer );
        transferShader.SetBuffer( 0, "volumeBuffer"     , sdf._buffer );


        transferShader.Dispatch( 0 , numTransferGroups,1,1 );


        realMaterial.SetBuffer("_transferBuffer" , _transferBuffer);
        realMaterial.SetColor("_Color" , col);





      }else{

        createOldBuffer();
      }

    }

    void Set(){
      DoPhysics(1);
    }



    // Update is called once per frame
    void LateUpdate () {
      DoPhysics( 0 );

    }




}