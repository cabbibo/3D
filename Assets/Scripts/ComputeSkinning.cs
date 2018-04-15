using UnityEngine;
using System.Collections;

public class ComputeSkinning : MonoBehaviour {

  public BoneBuffer boneBuffer;
  public SkinnedVertBuffer vertBuffer;
  public SkinnedTriangleBuffer triBuffer;

  public ComputeShader computeShader;
  
  public Material material;

  public bool showMesh = true;


  private int numThreads = 64;
  private int numGroups;

  private int _kernel;
  private int vertCount;

  public int Set = 0;

  private Material mat;

  // Use this for initialization
  void Start () {

    if( boneBuffer == null ){ boneBuffer = this.GetComponent<BoneBuffer>(); }
    if( vertBuffer == null ){ vertBuffer = this.GetComponent<SkinnedVertBuffer>(); }
    if( triBuffer == null ){ triBuffer = this.GetComponent<SkinnedTriangleBuffer>(); }

    vertCount = vertBuffer.vertCount;
    _kernel = computeShader.FindKernel("CSMain");


    numGroups = (vertCount+(numThreads-1))/numThreads;

    SetBegin();

    mat = new Material( material );
  
  }


  void OnRenderObject(){

    if( showMesh == true){
      mat.SetPass(0);

      mat.SetBuffer("_vertBuffer", vertBuffer._buffer);
      mat.SetBuffer("_triBuffer", triBuffer._buffer);
    
      Graphics.DrawProcedural(MeshTopology.Triangles, triBuffer.triCount );
    }

  }


  void SetBindPoses(){

  }

  void SetBegin(){

    Set = 1;
    Dispatch(); 
    Set = 0;
    

  }

  void Dispatch(){

    computeShader.SetInt( "_Set" , Set );
    computeShader.SetInt( "_NumVerts" , vertBuffer.vertCount );

    if( vertBuffer._buffer != null &&
        boneBuffer._boneBuffer != null ){

        computeShader.SetBuffer( _kernel , "vertBuffer"     , vertBuffer._buffer );
        computeShader.SetBuffer( _kernel , "boneBuffer"     , boneBuffer._boneBuffer );
        computeShader.Dispatch( _kernel, numGroups,1,1 );
      }
 
  }
  
  // Update is called once per frame
  void FixedUpdate () {

    Dispatch();


  }
}