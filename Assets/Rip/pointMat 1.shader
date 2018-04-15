Shader "Custom/pointMat 1" {
	Properties {
        _NormalMap( "Normal Map" , 2D ) = "white" {}
        _CubeMap( "Cube Map" , Cube ) = "white" {}
        _TexMap( "Tex Map" , 2D ) = "white" {}
        _SizeMultiplier( "Size Multiplier" , float ) = 1
    }
  SubShader{




    Cull off
    Pass{


      CGPROGRAM
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Chunks/hsv.cginc"


uniform float _RibbonsIn;
uniform sampler2D _AudioMap;
uniform int _NumVerts;
uniform int _TailSize;
uniform int _TotalOld;


      StructuredBuffer<float4> _buffer;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos 			: SV_POSITION;
          float3 nor 			: TEXCOORD0;
          float2 uv  			: TEXCOORD1;
          float3 eye      : TEXCOORD5;
          //float3 worldPos : TEXCOORD6;
          float3 debug    : TEXCOORD7;

      };


      varyings vert (uint id : SV_VertexID){

        varyings o;


        int quadID = floor( (float(id) / 6));
        int particleID = floor( quadID / (_TailSize-1));

       //	if( particleID < _RibbonsIn ){


        int rowID = quadID % (_TailSize-1);
        int vertID = int(floor( (float(id) / 6)));
        int triID = id % 6;


if( id < _TotalOld ){
        float4 v1  = _buffer[id];
        o.debug = float3(0 ,v1.w,0);

        o.uv = float2(0,0);
        o.nor = float3(0,0,0);
        o.eye = float3(0,0,0);


				o.pos = mul (UNITY_MATRIX_VP, float4(v1.xyz,1.0f));

}


        return o;


      }
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        float3 col = float3(1,1,1);//float3( .5,.3,.1);
        return float4( col , 1. );

      }

      ENDCG

    }
  }

  Fallback Off

}