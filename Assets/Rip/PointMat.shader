Shader "Custom/PointMat" {
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



      StructuredBuffer<Vert> _vertBuffer;


      //uniform float4x4 worldMat;

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


      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (uint id : SV_VertexID){

        varyings o;

        int base = id / 6;
        int alternate = id %6;
        if( base < _Count ){

        	float3 extra = float3(0,0,0);

          float3 l = UNITY_MATRIX_V[0].xyz;
          float3 u = UNITY_MATRIX_V[1].xyz;
        	if( alternate == 0 ){ extra = -l - u;}
          if( alternate == 1 ){ extra =  l - u;}
          if( alternate == 2 ){ extra =  l + u;}
          if( alternate == 3 ){ extra = -l - u;}
          if( alternate == 4 ){ extra =  l + u;}
        	if( alternate == 5 ){ extra = -l + u;}


        	Vert v = _vertBuffer[base];
          float lifeM = min( (1-v.life ) * 5 , v.life);
       		o.worldPos = v.pos + v.life *extra * .01;///* .001/(.1+length(v.debug));//*(1/(.1+max(length(v.debug),0)));//mul( worldMat , float4( v.pos , 1.) ).xyz;
	        o.debug = v.debug;
	        o.eye = _WorldSpaceCameraPos - o.worldPos;
          o.nor =float3(0,1,0);//fPos;
          o.uv = float2(0,0);

	        o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

       	}
    
        return o;

      }




      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {
      		float3 col = float3(1,1,1);//normalize(v.debug) * .5 + .5;
          //col = float3( v.uv.x , v.uv.y , .5);
          return float4( col , 1 );

      }

      ENDCG

    }
  }

  Fallback Off


}