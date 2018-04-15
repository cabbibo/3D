
using UnityEngine;
using System.Collections;

public class VertBuffer : MonoBehaviour {
  

  public Mesh mesh;

  public int vertCount;
  public ComputeBuffer _buffer; 
  public float[] values;

  public Vector3[] vertices;
  public Vector3[] normals;
  public Vector2[] uvs;

  public int structSize; 

  public void Awake(){
  
    CreateBuffer();
  }

  void OnDisable(){
    ReleaseBuffer();
  }

  public void ReleaseBuffer(){
    _buffer.Release(); 
  }


  public virtual void CreateBuffer(){


  }
}