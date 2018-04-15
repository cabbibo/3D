Shader "Custom/marchingCubesTest" {
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

		  uniform int _NumVerts;
      
struct Vert{
  float3 pos;
  float3 nor;
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

        //if( id >= _NumVerts ) return o;

        Vert v = _vertBuffer[id];
       	o.worldPos = v.pos;
        o.nor = v.nor;

	      o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

       	
    
        return o;

      }




      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {
      		float3 col = v.nor * .5 + .5;//normalize(v.debug) * .5 + .5;
          //col = float3( v.uv.x , v.uv.y , .5);
          return float4( col , 1 );

      }

      ENDCG

    }
  }

  Fallback Off


}