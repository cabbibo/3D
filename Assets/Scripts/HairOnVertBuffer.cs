using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HairOnVertBuffer : MonoBehaviour {

  public VertBuffer vertBuffer;
  public TriangleBuffer triBuffer;

  public ComputeShader collisionShader;
  public ComputeShader constraintShader;

  public Material material;
  public Color hairColor;

  public bool showHairs = true;

  public int totalHairs;
  public int numVertsPerHair = 6;
  public float hairLength = 2;
  public float distBetweenHairs { get { return hairLength / numVertsPerHair; }}

  public int fullVertCount;
  private int numGroups;
  private int numThreads = 64;

  

  struct Vert{

    public Vector3 pos;
    public Vector3 oPos;
    public Vector3 vel;
    public Vector3 nor;
    public Vector2 uv;
    public Vector3 debug;

    public Vector3 triIDs;
    public Vector3 triWeights;

  };

  public int vertStructSize = 3+ 3+ 3 + 3 + 2 + 3 + 3 + 3;

  private int _kernelCollision;
  private int _kernelConstraint;

  public ComputeBuffer _buffer;
  private float[] values;

  private float[] triAreas;


  // Use this for initialization
  void Start () {

    if( vertBuffer == null ){ vertBuffer = gameObject.GetComponent<VertBuffer>(); }
    if( triBuffer == null ){ triBuffer = gameObject.GetComponent<TriangleBuffer>(); }

    fullVertCount = totalHairs * numVertsPerHair;

    material = new Material( material );

    numGroups = (fullVertCount+(numThreads-1))/numThreads;

    _kernelCollision = collisionShader.FindKernel("CSMain");
    _kernelConstraint = constraintShader.FindKernel("CSMain");

    CreateBuffers();
  
  }
  
  void CreateBuffers(){

    _buffer = new ComputeBuffer( fullVertCount , vertStructSize * sizeof(float));
    values = new float[ fullVertCount * vertStructSize ];

    // Used for assigning to our buffer;
    int index = 0;



  triAreas = new float[triBuffer.triangles.Length/3];
  

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

    for (int i = 0; i < fullVertCount; i++ ){

          int id = i; 
          //print( index);
          //print( inValues.Length );

          int idInHair = id % numVertsPerHair;
          int hairID = (int)Mathf.Floor( (float)id / (float)numVertsPerHair );

          // Resets using same hairID, so RandomPointInTriangle shoudl work
          float randomVal = HelperFunctions.getRandomFloatFromSeed( hairID * 20 );
          
          int tri0 = 3 * HelperFunctions.getTri( randomVal ,triAreas );
          int tri1 = tri0 + 1;
          int tri2 = tri0 + 2;



          tri0 = triBuffer.triangles[tri0];
          tri1 = triBuffer.triangles[tri1];
          tri2 = triBuffer.triangles[tri2];

          Vector3 pos = HelperFunctions.GetRandomPointInTriangle( hairID , vertBuffer.vertices[ tri0 ] , vertBuffer.vertices[ tri1 ]  , vertBuffer.vertices[ tri2 ]  );
          
          float a0 = HelperFunctions.AreaOfTriangle( pos , vertBuffer.vertices[tri1] , vertBuffer.vertices[tri2] );
          float a1 = HelperFunctions.AreaOfTriangle( pos , vertBuffer.vertices[tri0] , vertBuffer.vertices[tri2] );
          float a2 = HelperFunctions.AreaOfTriangle( pos , vertBuffer.vertices[tri0] , vertBuffer.vertices[tri1] );
          float aTotal = a0 + a1 + a2;

          float p0 = a0 / aTotal;
          float p1 = a1 / aTotal;
          float p2 = a2 / aTotal;


          Vector3 nor     = vertBuffer.normals[tri0]  * p0 + vertBuffer.normals[tri1]  * p1 + vertBuffer.normals[tri2]  * p2;
          nor = nor.normalized;
          /*float3 tang    = tri0.tang  * p0 + tri1.tang  * p1 + tri2.tang  * p2;
          float3 color   = tri0.color  * p0 + tri1.color  * p1 + tri2.color  * p2;
          float2 uv      = tri0.uv * p0 + tri1.uv * p1 + tri2.uv * p2;*/
          

          float idVal = (float)id / (float)fullVertCount;
          float uvX = (float)idInHair / (float)numVertsPerHair;
          float uvY = (float)hairID / (float)totalHairs;

          int xID = hairID % 180;
          int zID = (int)Mathf.Floor( (float)hairID / (float)180 );

          float xPos = (float)xID/180;
          float zPos = (float)zID/180;

          Vector3 fPos = pos + nor * hairLength * uvX;
          
          // pos
          values[index++] = fPos.x;
          values[index++] = fPos.y;
          values[index++] = fPos.z;

          // oPos
          values[index++] = fPos.x;
          values[index++] = fPos.y;
          values[index++] = fPos.z;

          //vel
          values[index++] = 0;
          values[index++] = 0;
          values[index++] = 0;

          // nor
          values[index++] = nor.x;
          values[index++] = nor.y;
          values[index++] = nor.z;

          // uv
          values[index++] = uvX;
          values[index++] = uvY;

          // debug
          values[index++] = 1;
          values[index++] = 0;
          values[index++] = 0;


          // triIDs
          values[index++] = tri0;
          values[index++] = tri1;
          values[index++] = tri2;

          // triWeights
          values[index++] = p0;
          values[index++] = p1;
          values[index++] = p2;
          

        
    }

    _buffer.SetData(values);
    

  }
  
  void OnRenderObject(){

    if( showHairs == true ){

      material.SetPass(0);

      material.SetInt( "_VertsPerHair" , numVertsPerHair );
      material.SetBuffer("_vertBuffer", _buffer );
      material.SetColor("_Color", hairColor );

      Graphics.DrawProcedural(MeshTopology.Lines, totalHairs * (numVertsPerHair-1) * 2 );

    }

  }


  // Update is called once per frame
  void FixedUpdate () {
  
  
    collisionShader.SetInt( "_Reset" , 0 );
    collisionShader.SetInt( "_NumVerts" , fullVertCount );
    collisionShader.SetFloat( "_DeltaTime" , Time.deltaTime );
    collisionShader.SetFloat( "_Time" , Time.time );

    collisionShader.SetFloat( "_SpringDistance" , distBetweenHairs );

    collisionShader.SetBuffer( _kernelCollision , "hairBuffer"     , _buffer );
    collisionShader.SetBuffer( _kernelCollision , "baseBuffer"     , vertBuffer._buffer );
    collisionShader.Dispatch( _kernelCollision, numGroups,1,1);

    constraintShader.SetInt( "_PassID" , 0 );

    constraintShader.SetFloat( "_SpringDistance" , distBetweenHairs );
    constraintShader.SetInt( "_VertsPerHair" , numVertsPerHair );
    constraintShader.SetInt( "_NumVerts" , fullVertCount );

    constraintShader.SetBuffer( _kernelConstraint , "vertBuffer"     , _buffer );
    constraintShader.Dispatch( _kernelConstraint, (numGroups / 2) + 1 , 1 , 1);


    constraintShader.SetInt( "_PassID" , 1 );

    constraintShader.SetFloat( "_SpringDistance" , distBetweenHairs );

    constraintShader.SetInt( "_NumVerts" , fullVertCount );
    constraintShader.SetInt( "_VertsPerHair" , numVertsPerHair );
    constraintShader.SetBuffer( _kernelConstraint , "vertBuffer"     , _buffer );
    constraintShader.Dispatch( _kernelConstraint, (numGroups / 2) + 1 , 1 , 1);
    
  }
}