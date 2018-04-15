Shader "Custom/Tail" {
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



        if( particleID * _TailSize + rowID  < _TotalOld ){
        float4 v1  = _buffer[particleID * _TailSize + rowID    ];
        float4 v2  = _buffer[particleID * _TailSize + rowID + 1];


        if( v1.w > 0.1 && v2.w > 0.1){
        float3 dir = v1.xyz -v2.xyz;
        float3 dir2 = dir;

        if( rowID + 2 < _TailSize ){
        	dir2 = v2.xyz-_buffer[particleID*_TailSize + rowID + 2].xyz;
        }


        float3 nor =  normalize(v1.xyz-_WorldSpaceCameraPos);
        float3 nor2 = normalize(v2.xyz-_WorldSpaceCameraPos);

        float3 tan1 = normalize(cross( dir , nor ));
        float3 tan2 = normalize(cross( dir2 , nor2 ));

        float y =  float(float(rowID) / float(_TailSize));
        float y2 =  float(float(rowID+1) / float(_TailSize));

        float width  = .01 * (1-y)  *min( (1-v1.w) * 5 , v1.w);
        float width2 = .01 * (1-y2) *min( (1-v1.w) * 5 , v2.w);

        float3 p1 = v1.xyz - tan1 * width;
        float3 p2 = v1.xyz + tan1 * width;
        float3 p3 = v2.xyz - tan2 * width2;
        float3 p4 = v2.xyz + tan2 * width2;

        float3 fPos = float3(0,0,0);
        float3 fEye = float3(0,0,0);
        float3 fNor = float3(0,0,0);
        float2 fUV = float2(0,0);


        if( triID == 0){ fPos = p1; fNor = nor;  fEye=dir ;fUV  = float2( 0, float(rowID) / float(_TailSize ));}
        if( triID == 1){ fPos = p3; fNor = nor2; fEye=dir2 ;fUV = float2( 0, float(rowID+1)  /float( _TailSize ));}
        if( triID == 2){ fPos = p4; fNor = nor2; fEye=dir2 ;fUV = float2( 1, float(rowID+1)  / float(_TailSize ));}
        if( triID == 3){ fPos = p4; fNor = nor2; fEye=dir2 ;fUV = float2( 1, float(rowID+1)  / float(_TailSize ));}
        if( triID == 4){ fPos = p1; fNor = nor;  fEye=dir ;fUV  = float2(  0, float(rowID) / float(_TailSize ));}
        if( triID == 5){ fPos = p2; fNor = nor;  fEye=dir ;fUV  = float2(  1, float(rowID) / float(_TailSize ));}


        o.debug = float3( float(rowID) / float(_TailSize) ,0,0);

        o.uv = fUV;
        o.nor = fNor;
        o.eye = fEye;


				o.pos = mul (UNITY_MATRIX_VP, float4(fPos,1.0f));
				}


			}

        return o;


      }
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

        float3 col =  normalize(v.eye) * .5+.5;//float3(0,0,v.debug.x * 100);//float3( .5,.3,.1);
        return float4( col , 1. );

      }

      ENDCG

    }
  }

  Fallback Off

}