using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBoneField : MonoBehaviour {

  public int numberOfLimbs;
  private int numberOfBonesPerLimb;
  private int numberOfPointsPerLimb;


  public VertBuffer baseVertBuffer;
  public TriangleBuffer baseTriBuffer;
  public Transform baseTransform;


  public BoneBuffer boneBuffer;
  public SkinnedTriangleBuffer triBuffer;
  public SkinnedVertBuffer vertBuffer;

  public Material pointMaterial;
  public Material boneMaterial;

  public Color color1;
  public Color color2;


  public Material material;

  public ComputeShader physics;
  public ComputeShader skinning;
  public ComputeShader transform;
  public ComputeShader constraint;


  private int physicsKernel;
  private int constraintKernel;
  private int transformKernel;
  private int skinningKernel;

  public ComputeBuffer fullBoneBuffer;
  public ComputeBuffer fullVertBuffer;
  public ComputeBuffer fullPointBuffer;


  private int numThreads = 64;
  private int physicsGroups;
  private int transformGroups;
  private int skinningGroups;
  private int constraintGroups;


  private float[] vertValues;
  private float[] boneValues;
  private float[] pointValues;

  private int totalVerts;

  private int totalNumberBones;
  private int totalNumberVerts;
  private int totalNumberPoints;

  struct Bone{
    public float id;
    public float idInBone;
    public Matrix4x4 transform;
    public Matrix4x4 bindPose;
  };



  struct Point{
    public float distanceUp;
    public Vector3 pos;
    public Vector3 oPos;
    public Vector3 nor;
    public Vector2 uv;
    public Vector3 debug;

  };


  private int boneStructSize = 1+1+16+16;
  private int pointStructSize = 1+3+3+3+2+3;


public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
    // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
    Quaternion q = new Quaternion();
    q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
    q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
    q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
    q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
    q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
    q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
    q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
    return q;
}

	// Use this for initialization
	void Start () {

    physicsKernel = physics.FindKernel("CSMain");
    skinningKernel = skinning.FindKernel("CSMain");


    skinningKernel = skinning.FindKernel("CSMain");
    transformKernel = transform.FindKernel("CSMain");

    physicsKernel = physics.FindKernel("CSMain");
    constraintKernel = constraint.FindKernel("CSMain");

    numberOfBonesPerLimb = boneBuffer.boneCount;
    numberOfPointsPerLimb = numberOfBonesPerLimb + 1;

    print( vertBuffer.vertCount );
    totalNumberBones = numberOfBonesPerLimb * numberOfLimbs;
    totalNumberVerts = vertBuffer.vertCount * numberOfLimbs;
    totalNumberPoints = (numberOfPointsPerLimb )  * numberOfLimbs;

    print( totalNumberVerts );
    print( totalNumberBones );
		
    fullPointBuffer = new ComputeBuffer( totalNumberPoints , pointStructSize * sizeof(float) );
    fullBoneBuffer = new ComputeBuffer( totalNumberBones , boneStructSize * sizeof(float) );
    fullVertBuffer = new ComputeBuffer( totalNumberVerts , vertBuffer.structSize * sizeof(float) );

    boneValues = new float[ totalNumberBones * boneStructSize ];
    vertValues = new float[ totalNumberVerts * vertBuffer.structSize ];
    pointValues = new float[ totalNumberPoints * pointStructSize ];

    int index = 0;
    Vector3 basePosition;
    Vector3 fPos;
    for( int i = 0; i < numberOfLimbs; i++ ){



      // Resets using same hairID, so RandomPointInTriangle shoudl work
      float randomVal = HelperFunctions.getRandomFloatFromSeed( i * 20 );

      int tri0 = (int)(randomVal * (float)(baseTriBuffer.triangles.Length/3)) * 3;
      //int tri0 = (int)(randomVal *  1000 + 1000 ) * 3;
      int tri1 = tri0 + 1;
      int tri2 = tri0 + 2;

   

      tri0 = baseTriBuffer.triangles[tri0];
      tri1 = baseTriBuffer.triangles[tri1];
      tri2 = baseTriBuffer.triangles[tri2];


      Vector3 pos = HelperFunctions.GetRandomPointInTriangle( i , baseVertBuffer.vertices[ tri0 ] , baseVertBuffer.vertices[ tri1 ]  , baseVertBuffer.vertices[ tri2 ]  );
          
      float a0 = HelperFunctions.AreaOfTriangle( pos , baseVertBuffer.vertices[tri1] , baseVertBuffer.vertices[tri2] );
      float a1 = HelperFunctions.AreaOfTriangle( pos , baseVertBuffer.vertices[tri0] , baseVertBuffer.vertices[tri2] );
      float a2 = HelperFunctions.AreaOfTriangle( pos , baseVertBuffer.vertices[tri0] , baseVertBuffer.vertices[tri1] );
      float aTotal = a0 + a1 + a2;

      float p0 = a0 / aTotal;
      float p1 = a1 / aTotal;
      float p2 = a2 / aTotal;


      Vector3 nor     = baseVertBuffer.normals[tri0]  * p0 + baseVertBuffer.normals[tri1]  * p1 + baseVertBuffer.normals[tri2]  * p2;
      nor = nor.normalized;


      basePosition = baseTransform.TransformPoint(pos);//new Vector3( i * .1f, 0 , 0);



      for( int j = 0; j < numberOfBonesPerLimb; j++ ){

        fPos= boneBuffer.bones[j].transform.position + basePosition;


        pointValues[index++] = 1;

        pointValues[index++] = fPos.x;
        pointValues[index++] = fPos.y;
        pointValues[index++] = fPos.z;

        pointValues[index++] = fPos.x;
        pointValues[index++] = fPos.y;
        pointValues[index++] = fPos.z;


        pointValues[index++] = nor.x;
        pointValues[index++] = nor.y;
        pointValues[index++] = nor.z;


        pointValues[index++] = (float)i/(float)numberOfLimbs;
        pointValues[index++] = (float)j/(float)numberOfBonesPerLimb;

        pointValues[index++] = 0;
        pointValues[index++] = 1;
        pointValues[index++] = 0;

      }

      
      fPos = boneBuffer.bones[numberOfBonesPerLimb-1].transform.position + basePosition + Vector3.up * .1f;

      pointValues[index++] = 1;

      pointValues[index++] = fPos.x;
      pointValues[index++] = fPos.y;
      pointValues[index++] = fPos.z;

      pointValues[index++] = fPos.x;
      pointValues[index++] = fPos.y;
      pointValues[index++] = fPos.z;


      pointValues[index++] = 0;
      pointValues[index++] = 1;
      pointValues[index++] = 0;


      pointValues[index++] = (float)i/(float)numberOfLimbs;
      pointValues[index++] = 1;

      pointValues[index++] = 0;
      pointValues[index++] = 1;
      pointValues[index++] = 0;



    }

    fullPointBuffer.SetData(pointValues);




    index = 0;
    for( int i = 0; i < numberOfLimbs; i++ ){

      float rot = 360 * Random.value;
      for( int j = 0; j < numberOfBonesPerLimb; j++ ){
        
        boneValues[index++] = i;
        boneValues[index++] = j;

        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[0,0];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[1,0];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[2,0];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[3,0];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[0,1];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[1,1];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[2,1];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[3,1];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[0,2];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[1,2];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[2,2];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[3,2];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[0,3];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[1,3];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[2,3];
        boneValues[index++] = boneBuffer.bones[j].transform.localToWorldMatrix[3,3];



        var rotation = Matrix4x4.Rotate(boneBuffer.bones[j].rotation);


        var quat = QuaternionFromMatrix( boneBuffer.bones[j].transform.localToWorldMatrix );
        //undo rotations of blender
        var tmpMat = Matrix4x4.Rotate(quat) * boneBuffer.bindPoses[j] ; 

        //adding random y rotation!
        tmpMat = Matrix4x4.Rotate(Quaternion.AngleAxis( rot , Vector3.up)) * tmpMat;


        boneValues[index++] = tmpMat[0,0];
        boneValues[index++] = tmpMat[1,0];
        boneValues[index++] = tmpMat[2,0];
        boneValues[index++] = tmpMat[3,0];
        boneValues[index++] = tmpMat[0,1];
        boneValues[index++] = tmpMat[1,1];
        boneValues[index++] = tmpMat[2,1];
        boneValues[index++] = tmpMat[3,1];
        boneValues[index++] = tmpMat[0,2];
        boneValues[index++] = tmpMat[1,2];
        boneValues[index++] = tmpMat[2,2];
        boneValues[index++] = tmpMat[3,2];
        boneValues[index++] = tmpMat[0,3];
        boneValues[index++] = tmpMat[1,3];
        boneValues[index++] = tmpMat[2,3];
        boneValues[index++] = tmpMat[3,3];


      }
    }

    fullBoneBuffer.SetData(boneValues);



    index = 0;
    for( int j = 0; j < numberOfLimbs; j++ ){
      for( int i = 0; i < vertBuffer.vertCount; i++ ){

        // used 
        vertValues[ index++ ] = j;

        // positions
        vertValues[ index++ ] = vertBuffer.vertices[i].x * .99f;
        vertValues[ index++ ] = vertBuffer.vertices[i].y * .99f;
        vertValues[ index++ ] = vertBuffer.vertices[i].z * .99f;

        // vel
        vertValues[ index++ ] = 0;
        vertValues[ index++ ] = 0;
        vertValues[ index++ ] = 0;

        // normals
        vertValues[ index++ ] = vertBuffer.normals[i].x;
        vertValues[ index++ ] = vertBuffer.normals[i].y;
        vertValues[ index++ ] = vertBuffer.normals[i].z;

        // uvs
        vertValues[ index++ ] = vertBuffer.uvs[i].x;
        vertValues[ index++ ] = vertBuffer.uvs[i].y;


        // target pos
        vertValues[ index++ ] = vertBuffer.vertices[i].x;
        vertValues[ index++ ] = vertBuffer.vertices[i].y;
        vertValues[ index++ ] = vertBuffer.vertices[i].z;



        // bindPositions
        vertValues[ index++ ] = vertBuffer.vertices[i].x;
        vertValues[ index++ ] = vertBuffer.vertices[i].y;
        vertValues[ index++ ] = vertBuffer.vertices[i].z;


        // bindNor
        vertValues[ index++ ] = vertBuffer.normals[i].x;
        vertValues[ index++ ] = vertBuffer.normals[i].y;
        vertValues[ index++ ] = vertBuffer.normals[i].z;

        // bone weights
        vertValues[ index++ ] = vertBuffer.weights[i].weight0;
        vertValues[ index++ ] = vertBuffer.weights[i].weight1;
        vertValues[ index++ ] = vertBuffer.weights[i].weight2;
        vertValues[ index++ ] = vertBuffer.weights[i].weight3;

        // bone indices
        vertValues[ index++ ] = vertBuffer.weights[i].boneIndex0;
        vertValues[ index++ ] = vertBuffer.weights[i].boneIndex1;
        vertValues[ index++ ] = vertBuffer.weights[i].boneIndex2;
        vertValues[ index++ ] = vertBuffer.weights[i].boneIndex3;

        // Debug
        vertValues[ index++ ] = 1;
        vertValues[ index++ ] = 0;
        vertValues[ index++ ] = 0;

      } 
    }
    
    fullVertBuffer.SetData(vertValues);




    transformGroups = ((totalNumberBones)+(numThreads-1))/numThreads;
    constraintGroups = ((totalNumberPoints)+(numThreads-1))/numThreads;
    skinningGroups = ((totalNumberVerts)+(numThreads-1))/numThreads;
    physicsGroups = ((totalNumberPoints)+(numThreads-1))/numThreads;

	}
	

  void OnRenderObject(){



    /*pointMaterial.SetPass(0);

    pointMaterial.SetBuffer("_vertBuffer", fullPointBuffer );
    pointMaterial.SetInt( "_TotalVerts", totalNumberPoints );

    Graphics.DrawProcedural(MeshTopology.Triangles, 3  * totalNumberPoints );*/



    material.SetPass(0);

    material.SetBuffer("_vertBuffer", fullVertBuffer );
    material.SetBuffer("_triBuffer", triBuffer._buffer );

    material.SetInt("_VertsPerMesh" , vertBuffer.vertCount );
    material.SetInt("_TriCount", triBuffer.triCount );
    material.SetInt( "_TotalVerts", totalNumberVerts );

    material.SetVector("_Color1", color1);
    material.SetVector("_Color2", color2);

    Graphics.DrawProcedural(MeshTopology.Triangles, triBuffer.triCount * numberOfLimbs );

  }

  void FixedUpdate(){


    physics.SetInt( "_NumPoints" , totalNumberPoints );
    physics.SetFloat( "_DeltaTime" , Time.deltaTime );
    physics.SetFloat( "_Time" , Time.time );
    physics.SetBuffer( physicsKernel  , "pointBuffer"     , fullPointBuffer );
    physics.Dispatch( physicsKernel , physicsGroups,1,1 );


    /*constraint.SetInt( "_PassID" , 0 );

    constraint.SetInt( "_VertsPerHair" , numberOfPointsPerLimb );
    constraint.SetInt( "_NumVerts" , totalNumberPoints );

    constraint.SetBuffer( constraintKernel , "vertBuffer"     , fullPointBuffer );
    constraint.Dispatch( constraintKernel, (constraintGroups / 2) + 1 , 1 , 1);

    constraint.SetInt( "_PassID" , 1 );

    constraint.SetInt( "_NumVerts" , totalNumberPoints );
    constraint.SetInt( "_VertsPerHair" , numberOfPointsPerLimb);
    constraint.SetBuffer( constraintKernel , "vertBuffer"     , fullPointBuffer );
    constraint.Dispatch( constraintKernel, (constraintGroups / 2) + 1 , 1 , 1);*/




    transform.SetInt( "_NumBones" , totalNumberBones );
    transform.SetInt( "_NumPoints" , totalNumberPoints );

    transform.SetInt( "_NumBonesPerLimb" , numberOfBonesPerLimb );
    transform.SetInt( "_NumPointsPerLimb" , numberOfPointsPerLimb );

    transform.SetBuffer( transformKernel  , "boneBuffer"     , fullBoneBuffer );
    transform.SetBuffer( transformKernel  , "pointBuffer"     , fullPointBuffer );
    transform.Dispatch( transformKernel , transformGroups,1,1 );
      

    skinning.SetInt( "_NumVerts" , totalNumberVerts );
    skinning.SetInt( "_NumBones" , totalNumberBones );
    skinning.SetInt( "_NumBonesPerLimb" , numberOfBonesPerLimb );

    skinning.SetBuffer( skinningKernel  , "vertBuffer"     , fullVertBuffer );
    skinning.SetBuffer( skinningKernel  , "boneBuffer"     , fullBoneBuffer );
    skinning.Dispatch( skinningKernel , skinningGroups,1,1 );
 
 

    // First! Update the physics for the bone field


    // Second! Skin dem bones!


  }
}
