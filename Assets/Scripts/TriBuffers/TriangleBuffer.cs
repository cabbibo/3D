using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriangleBuffer : MonoBehaviour {

  public Mesh mesh;

  public int triCount;
  public ComputeBuffer _buffer;  
  public int[] triangles;

  // Use this for initialization
  void Awake () {
  
    GetMesh();
    CreateBuffer();  
   
  
  }

  void OnDisable(){
    ReleaseBuffer();
  }

  public void ReleaseBuffer(){
    _buffer.Release(); 
  }

  public virtual void GetMesh(){

    if( mesh == null){
      mesh = gameObject.GetComponent<MeshFilter>().mesh;
    }
    

  }

  public virtual void CreateBuffer(){

    triangles =  mesh.triangles;
    triCount = mesh.triangles.Length;

    _buffer = new ComputeBuffer( triCount , sizeof(int) ); 
    _buffer.SetData(triangles);
  }

}