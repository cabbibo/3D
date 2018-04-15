using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloner: MonoBehaviour {


  public GameObject toClone;

  public int numberMeshes;
  public ComputeShader physics;
  public Material material;


  public Effector[] effectors;


  // Mesh we want to spawn
  private VertBuffer cloneVertBuffer;
  private TriangleBuffer cloneTriBuffer;


  // Mesh we are cloning onto
  public VertBuffer vertBuffer;
  public TriangleBuffer triBuffer;



  [HideInInspector]
  public int numGroups;
  private int numThreads = 64;



  struct Basis{

    public float id;

    public Matrix4x4 basis;

    public Vector2 uv;
  
    public Vector3 triIDs;
    public Vector3 triWeights;
    public Vector3 debug;

  }




  private int BasisStructSize = 1+16+2+3+3+3;

  public int _kernel;

  public ComputeBuffer _buffer;
  private float[] values;
  private float[] triAreas;


  // Use this for initialization
  void Start () {


    if( vertBuffer == null ){ vertBuffer = gameObject.GetComponent<VertBuffer>(); }
    if( triBuffer == null ){ triBuffer = gameObject.GetComponent<TriangleBuffer>(); }

    if( cloneVertBuffer == null ){ cloneVertBuffer = toClone.GetComponent<VertBuffer>(); }
    if( cloneTriBuffer == null ){ cloneTriBuffer = toClone.GetComponent<TriangleBuffer>(); }

    material = new Material( material );

    numGroups = (numberMeshes+(numThreads-1))/numThreads;

    _kernel = physics.FindKernel("CSMain");

    CreateBuffers();
  
  }

  
  void CreateBuffers(){

    _buffer = new ComputeBuffer( numberMeshes , BasisStructSize * sizeof(float));
    values = new float[ numberMeshes * BasisStructSize ];
    
    triAreas = new float[triBuffer.triangles.Length/3 ];

    // Used for assigning to our buffer;
    int index = 0;

 //   print( numberMeshes );
//    print( values.Length );

    float totalArea = 0;


    for( int i = 0; i < triBuffer.triangles.Length/3; i++ ){
      int tri0 = i * 3;
      int tri1 = tri0 + 1;
      int tri2 = tri0 + 2;
      tri0 = triBuffer.triangles[tri0];
      tri1 = triBuffer.triangles[tri1];
      tri2 = triBuffer.triangles[tri2];


      float area = HelperFunctions.AreaOfTriangle( vertBuffer.vertices[tri0] , vertBuffer.vertices[tri1] , vertBuffer.vertices[tri2] );
      triAreas[i] = area;
      totalArea += area;
    }


    for( int i = 0; i < triAreas.Length; i++ ){
      triAreas[i] /= totalArea;
    }



    for (int i = 0; i < numberMeshes; i++ ){

          int id = i; 

          float randomVal = Random.value;

          int tri0 = 3 * HelperFunctions.getTri( randomVal , triAreas );
          int tri1 = tri0 + 1;
          int tri2 = tri0 + 2;

          tri0 = triBuffer.triangles[tri0];
          tri1 = triBuffer.triangles[tri1];
          tri2 = triBuffer.triangles[tri2];

          Vector3 pos = HelperFunctions.GetRandomPointInTriangle( id , vertBuffer.vertices[ tri0 ] , vertBuffer.vertices[ tri1 ]  , vertBuffer.vertices[ tri2 ]  );
          
          float a0 = HelperFunctions.AreaOfTriangle( pos , vertBuffer.vertices[tri1] , vertBuffer.vertices[tri2] );
          float a1 = HelperFunctions.AreaOfTriangle( pos , vertBuffer.vertices[tri0] , vertBuffer.vertices[tri2] );
          float a2 = HelperFunctions.AreaOfTriangle( pos , vertBuffer.vertices[tri0] , vertBuffer.vertices[tri1] );
          float aTotal = a0 + a1 + a2;

          float p0 = a0 / aTotal;
          float p1 = a1 / aTotal;
          float p2 = a2 / aTotal;


          Vector3 nor     = vertBuffer.normals[tri0]  * p0 + vertBuffer.normals[tri1]  * p1 + vertBuffer.normals[tri2]  * p2;
          nor = nor.normalized;
//          Vector3 color   = tri0.color  * p0 + tri1.color  * p1 + tri2.color  * p2;


          Vector2 uv      = vertBuffer.uvs[tri0] * p0 + vertBuffer.uvs[tri1] * p1 + vertBuffer.uvs[tri2] * p2;

          Vector3 left = Vector3.Cross( nor , Vector3.up );
          left.Normalize();
          Vector3 forward = Vector3.Cross( left , nor );
        

//          print( id );



          values[index++] = id;
          
          //transform
          for( int j = 0; j < 16; j++){
          values[index++] = 0;
          }

          // uv
          values[index++] = uv.x;
          values[index++] = uv.y;

          // triIDs
          values[index++] = tri0;
          values[index++] = tri1;
          values[index++] = tri2;

          // triWeights
          values[index++] = p0;
          values[index++] = p1;
          values[index++] = p2;

                 // debug
          values[index++] = 1;
          values[index++] = 0;
          values[index++] = 0;
          

        
    }

    _buffer.SetData(values);
    

  }


  
  void OnRenderObject(){


      material.SetPass(0);

      material.SetInt( "_VertsPerMesh" , cloneTriBuffer.triCount );
      material.SetBuffer("_vertBuffer", cloneVertBuffer._buffer );
      material.SetBuffer("_triBuffer", cloneTriBuffer._buffer );
      material.SetBuffer("_basisBuffer", _buffer );

      //Graphics.DrawProcedural(MeshTopology.Triangles ,numberMeshes * cloneTriBuffer.triCount );

  }


  void updateTransform(ComputeShader computeShader , int _kernel){

    Matrix4x4 matrix = transform.localToWorldMatrix; 
    float[] matrixFloats = new float[] 
    { 
    matrix[0,0], matrix[1, 0], matrix[2, 0], matrix[3, 0], 
    matrix[0,1], matrix[1, 1], matrix[2, 1], matrix[3, 1], 
    matrix[0,2], matrix[1, 2], matrix[2, 2], matrix[3, 2], 
    matrix[0,3], matrix[1, 3], matrix[2, 3], matrix[3, 3] 
    }; 

    computeShader.SetFloats( "transform" , matrixFloats );

  }
  // Update is called once per frame
  void FixedUpdate () {
  

    updateTransform( physics , _kernel );

    physics.SetInt( "_Reset" , 0 );
    physics.SetInt( "_NumMeshes" , numberMeshes );
    physics.SetFloat( "_DeltaTime" , Time.deltaTime );
    physics.SetFloat( "_Time" , Time.time );

    physics.SetBuffer( _kernel , "basisBuffer"     , _buffer );
    physics.SetBuffer( _kernel , "vertBuffer"     , vertBuffer._buffer );

    physics.Dispatch( _kernel, numGroups,1,1);

    for( int i = 0; i < effectors.Length; i++ ){
      effectors[i].UpdateStep( this );
    }

    
  }
}