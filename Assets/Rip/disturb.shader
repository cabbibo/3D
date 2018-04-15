// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/disturb" {
	Properties {
		}


  SubShader{
//        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
    Cull Off
    Pass{

      //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

      CGPROGRAM
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Chunks/noise.cginc"
      #include "Chunks/hsv.cginc"
      #include "Chunks/sdfStruct.cginc"

		  uniform int _Count;
      
struct Vert{
  float life;
  float3 pos;
  float3 vel;
  float3 nor;
  float2 uv;
  float3 targetPos;
  float3 debug;
};


uniform int _VolDim;

RWStructuredBuffer<SDF> _volumeBuffer;


      //uniform float4x4 worldMat;
       struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };


      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float3 debug    : TEXCOORD3;
          float2 uv       : TEXCOORD4;
          float  noiseVal : TEXCOORD5;
      };


float GetID( float3 pos ){


  if( abs(pos.x) < 1 &&abs(pos.y) < 1 &&abs(pos.z) < 1){
    int x = clamp( floor(((pos.x + 1)/2) * float(_VolDim)),0,_VolDim); 
    int y = clamp( floor(((pos.y + 1)/2) * float(_VolDim)),0,_VolDim); 
    int z = clamp( floor(((pos.z + 1)/2) * float(_VolDim)),0,_VolDim); 

    return x  + y * _VolDim + z * _VolDim * _VolDim;
    
    //return pos.x * _VolDim;// + sin(pos.y*100) + +sin(pos.z*100);//return floor(pos.x *100);//floor((pos.x +1)/2);//x * _VolDim * _VolDim + y * _VolDim + z;
  }else{
    return -1;
  }


}

      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (appdata v){

        varyings o;

      
      	float3 mPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1.0f)).xyz;
      	float3 mNor = mul(unity_ObjectToWorld, float4(v.normal.xyz,0.0f)).xyz;

        float id = GetID(mPos);
        if( id >= 0 ){
      		SDF vol = _volumeBuffer[int(id)];

      		//mPos += .02 * mNor / (.1 + vol.dist);
      		o.debug = vol.nor * .5 +.5;//float3(vol.debug.x , vol.debug.y,.3);// * 10;// *vol.nor * .5 + .5;//float3( vol.dist * 10, vol.dist *20, mPos.y);//mPos.xyz;//(vol.nor* .5 +.5);///vol.dist;//float3(sin(vol.dist*20),0,0);
	        o.pos = mul(UNITY_MATRIX_VP, float4(mPos,1.0f));
        }
       	
        return o;

      }




      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {


      		

      	/*if( v.debug.x > .3){
      		discard;
      	}*/
      		float3 col = v.debug;//hsv( v.debug.x ,1,1);//* .5 + .5;//float3(1,1,1);//normalize(v.debug) * .5 + .5;
          //col = float3( v.uv.x , v.uv.y , .5);
          return float4( col , 1 );

      }

      ENDCG

    }
  }

  Fallback Off


}