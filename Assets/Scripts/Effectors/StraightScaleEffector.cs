using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightScaleEffector : Effector {

  public float Scale;

  public override void UpdateStep(Cloner info ){


    physics.SetBuffer( info._kernel , "basisBuffer" , info._buffer );
    physics.SetBuffer( info._kernel , "vertBuffer"     , info.vertBuffer._buffer );

    physics.SetInt( "_NumMeshes" , info.numberMeshes );
    
    physics.SetFloat( "_DeltaTime" , Time.deltaTime );
    physics.SetFloat( "_Time" , Time.time );
    physics.SetFloat( "_Scale" , Scale );

    physics.Dispatch( info._kernel, info.numGroups,1,1);

  }


}