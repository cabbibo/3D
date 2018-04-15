using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DFVolume;

public class Rip : MonoBehaviour {

  public VolumeData volume;
  public Transform cam;

  public int Resolution;

  public int count;
  public int smoothedCount;

  public float drawCountPerStroke;
  private float drawCount = 0;

  private float strokeLength;

  public float playFrameTime;
  private float currentPlayTimer = 0;
  public ComputeShader physics;

  //public ComputeShader gatherShader;
  private Vector3 oPosition;
  private Vector3 velocity;
  private Vector3 oVelocity;

  public bool currentlyActive = false;
  private bool inside = false;

  private bool canDraw = false;


  private float loopVal = 0;


  struct Vert{
      public Vector3 pos;
      public Vector3 oPos;
      public Vector3 nor;
      public Vector2 uv;
      public float id;
      public float width;
      public float cap; // 0 for no cap, 1 for start , 2 for end
      public Vector3 targetPos;
      public Vector3 debug;
  };

  private int vertStructSize = 3+3+3+2+1+1+1+3+3;

  public int nrThreads = 64;
  private int nrGroups;

  public int currentVert = 0;
  public int oVert = 0;

  public bool drawing = false;
  public bool oDrawing = false;

  public float width = 0;
  public float cap = 0;


  private int _kernel;

  public ComputeBuffer _buffer;

  private float[] values;

  public Material ripMat;
  public Material debugLineMat;
  public Material debugPointMat;

  
  // Use this for initialization
  void Start () {

    
    _buffer = new ComputeBuffer( count , vertStructSize * sizeof(float));
    values = new float[ count * vertStructSize ];

    
    //print( nrGroups);
    nrGroups = (count+(nrThreads-1))/nrThreads;

/*
    // for data leaving compute shader
    _outBuffer = new ComputeBuffer( nrGroups , 4* sizeof(float) );
    outValues = new float[ 4 * nrGroups ];

    // for gathering said data into usable values!
    _gatherBuffer = new ComputeBuffer( 1 , 4* sizeof(float) );
    data = new float[ 4 ];*/
    

    int index = 0;


    for( int i = 0; i < count; i++ ){

    

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
      values[ index++ ] = i;//(float)j/(float)numVertsPerHair;//(float)j/((float)numVertsPerHair);
      values[ index++ ] = 0;//(float)i/(float)totalHairs;//(float)i/((float)totalHairs);

      values[ index++ ] = i;
      values[ index++ ] = 0;
      values[ index++ ] = 1;

      // target pos
      values[ index++ ] = 0;
      values[ index++ ] = 0;
      values[ index++ ] = 0;


      // Debug
      values[ index++ ] = 1;
      values[ index++ ] = 0;
      values[ index++ ] = 0;


      
    }

    _buffer.SetData(values);


  }



  public void toggleActivate(){
    currentlyActive = !currentlyActive;
  }

  void OnRenderObject(){

    debugLineMat.SetPass(0);
  
    debugLineMat.SetInt( "_Count" , count);
    debugLineMat.SetBuffer("_vertBuffer", _buffer );
  
    Graphics.DrawProcedural( MeshTopology.Lines , count*2 );

    ripMat.SetPass(0);
    ripMat.SetInt( "_count" , count );
    ripMat.SetInt( "_SmoothedCount" , smoothedCount );
    ripMat.SetBuffer("_vertBuffer", _buffer );

//    ripMat.SetTexture( "_Audio", alt.AudioTexture);
  
    Graphics.DrawProcedural( MeshTopology.Triangles , (smoothedCount-1) * 3 * 2 );


  }

  private void assignTransform(){

    Matrix4x4 m = transform.localToWorldMatrix;

    float[] matrixFloats = new float[] 
    { 
    m[0,0], m[1, 0], m[2, 0], m[3, 0], 
    m[0,1], m[1, 1], m[2, 1], m[3, 1], 
    m[0,2], m[1, 2], m[2, 2], m[3, 2], 
    m[0,3], m[1, 3], m[2, 3], m[3, 3] 
    }; 

    physics.SetFloats("transform", matrixFloats);

  }

  
  // Update is called once per frame
  void FixedUpdate () {

    oVert = currentVert;

    print(Input.GetMouseButton(0));

    if( Input.GetMouseButton(0) == true && currentlyActive == true && drawing == false && canDraw == true){
      drawing = true;
    }else if( Input.GetMouseButton(0)== false && currentlyActive == true ){
      drawing = false;
      canDraw = true;
    }

    // Currently Drawing
    if( oDrawing == true && drawing == true ){

      

        drawCount += strokeLength;

        width = 1;

        currentVert += Resolution;
        currentVert = currentVert % count;

        cap = 0;

        if( drawCount >= drawCountPerStroke ){
          drawing = false;
          canDraw = false;
        }
   /*currentPlayTimer += Random.Range( .8f , 1.6f );
        if( currentPlayTimer >= playFrameTime ){
          currentPlayTimer -= playFrameTime;
        audio.PlaySound( clip , Random.Range( .5f , .8f ) );

        }*/
      

    }

    // Just Started Drawing
    if( drawing == true && oDrawing == false ){
    
      width = 0;
      drawCount = 0;
    
      currentVert += Resolution;
      currentVert = currentVert % count;
    
      cap = 1;
      currentPlayTimer = 0;
      strokeLength = Random.Range( .5f , .8f );
//      GetComponent<AudioSource>().PlaySound( clip , Random.Range( .5f , .8f ) );

    
    }

    // Just Finished Drawing
    if( drawing == false && oDrawing == true ){
    
      width = 0;
      
      currentVert += Resolution;
      currentVert = currentVert % count;
      
      drawCount = 0;
      cap = 2;
    
    }

    
    Dispatch();

    oDrawing = drawing;

    if( drawing ){
      loopVal += .04f;
    }else{
      loopVal -= .04f;
    }

    loopVal = Mathf.Clamp( loopVal , 0, 1);

//    audio.volume = loopVal;
    
  }

  void Dispatch(){


    physics.SetInt( "_Reset" , 0 );

    physics.SetInt("_CurrentVert" , currentVert);
    
    physics.SetFloat("_Width", width);
    physics.SetFloat("_Cap", cap);


    physics.SetFloat( "_DeltaTime" , Time.deltaTime );
    physics.SetFloat( "_Time" , Time.time );


    velocity = velocity * .9f + .1f *(cam.position - oPosition);
    
    physics.SetVector( "_Velocity" , velocity);
    physics.SetVector( "_Position" , cam.position + cam.forward );
    physics.SetVector( "_OldVelocity" , oVelocity );
    physics.SetVector( "_OldPosition" , oPosition );

    physics.SetVector( "_CamPos", cam.position);
    physics.SetVector( "_CamDir", cam.forward );
    physics.SetFloat( "_TouchDown" , Input.GetMouseButtonDown(0) ? 1 : 0 );
    physics.SetBool( "_Drawing" , currentlyActive );

    physics.SetInt( "_Resolution" , Resolution );
    physics.SetInt( "_OldVert" , oVert );
    
    physics.SetBuffer(0 , "vertBuffer"     , _buffer );


    physics.SetInt( "_NumVerts" , count );
    physics.SetInt( "_NumGroups", nrGroups );

    physics.Dispatch(0, nrGroups,1,1 );

    oPosition = cam.position ;
    oVelocity = velocity;
    

  }
}