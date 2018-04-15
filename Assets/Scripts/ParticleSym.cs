using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DFVolume;

public class ParticleSym : MonoBehaviour {


  public VolumeData volume;
  public sdfFromVertBuffer sdf;

  public RandomVertBuffer particles;
  public ComputeShader physics;
  public Material material;
  public bool showMesh;
  private Material mat;

  private int numThreads = 64;
  private int numGroups;

  private int _kernel;
  private int vertCount;

  private ComputeBuffer _buffer;

  public int Set = 0;
	// Use this for initialization
	void Start () {

    particles = GetComponent<RandomVertBuffer>();
    vertCount = particles.vertCount;

            _buffer = new ComputeBuffer(volume.dimensions*volume.dimensions*volume.dimensions, sizeof(float)*4);
            _buffer.SetData(volume.values);

    numGroups = (vertCount+(numThreads-1))/numThreads;

    SetBegin();
    mat = new Material( material );
		
	}



  void OnRenderObject(){

    if( showMesh == true){
      mat.SetPass(0);
      mat.SetBuffer("_vertBuffer", particles._buffer);
      mat.SetInt("_Count", vertCount);
     // Graphics.DrawProcedural(MeshTopology.Triangles, vertCount * 3 *2 );
    }

  }

  void SetBegin(){
    Set = 1;
    Dispatch(); 
    Set = 0;
  }

  void Dispatch(){

    physics.SetInt( "_Set" , Set );
    physics.SetInt( "_NumVerts" , particles.vertCount );
    physics.SetFloat( "_DT" , Time.deltaTime );
    physics.SetFloat( "_Time" , Time.time );
    physics.SetInt( "_VolDim" , sdf.dimensions );
    physics.SetVector("_Pos", transform.position);
    physics.SetVector("_Dir", transform.forward);

    if( particles._buffer != null ){

        physics.SetBuffer( 0, "vertBuffer"     , particles._buffer );
        physics.Dispatch( 0, numGroups,1,1 );
      }
 
  }
  
  // Update is called once per frame
  void LateUpdate () {

    Dispatch();


  }
	
}
