using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effector : MonoBehaviour {


  public ComputeShader physics;

  // Use this for initialization
  void Start () {
    
  }
  
  // Update is called once per frame
  void Update () {
    
  }

  public virtual void UpdateStep( Cloner info ){} 


}
